using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WobbrockLib;

using LaserMouseCore;
using System.IO;
using System.Threading;

namespace LaserWindowMain
{
    public partial class Form1 : Form
    {
        Keys key1 = Keys.Up;
        Keys key2 = Keys.Down;
        KeyboardHook kh;
        bool pressing = false;
        Client c = new Client();
        bool recording = false;
        List<TimePointF> tpf = new List<TimePointF>();
        DateTime base_time;

        RecognizeCoreEntry rce = new RecognizeCoreEntry();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            kh = new KeyboardHook();
            kh.SetHook();
            kh.Blocked_Keys.Add(key1);
            kh.Blocked_Keys.Add(key2);
            kh.OnKeyDownEvent += KeyDownX;
            kh.OnKeyUpEvent += KeyUpX;
            new ConsoleHelper(this.textBox3);
            base_time = DateTime.Now;
            rce.ResultsCalculatedEvent += Recognized;
            rce.add_gesture("circle.xml");
            rce.add_gesture("N.xml");
        }

        void Recognized(object obj, RecognizeCoreEntry.ResultEventArgs e)
        {
            if (e.results.Max.percent > 0.8)
                Console.WriteLine("Recognized Patten:" + e.results.Max.result);
            else
                Console.WriteLine("Recognized but not a fair result");
        }

        void KeyDownX(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyData == key1 && !pressing)
            {
                Mouse_Keyboard_Press.mouse_left_down();
                pressing = true;
                //Mouse_Keyboard_Press.mouse_left_up();
            }
            if (e.KeyData == key2 && !recording)
            {
                recording = true;
            }
        }

        void KeyUpX(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyData == key1)
            {
                //Mouse_Keyboard_Press.mouse_left_down();
                Mouse_Keyboard_Press.mouse_left_up();
                pressing = false;
            }
            if (e.KeyData == key2 && !recording)
            {
                recording = false;
                rce.get_results_sync(tpf);
                tpf.Clear();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            kh.UnHook();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(textBox1.Text);
                c.connect(ip, int.Parse(textBox2.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!timer1.Enabled && c.connected)
                {
                    Console.WriteLine("采集开始");
                    kh.SetHook();
                    button2.Text = "停止";
                    timer1.Enabled = true;
                }
                else if (timer1.Enabled)
                {
                    Console.WriteLine("采集结束");
                    timer1.Enabled = false;
                    kh.UnHook();
                    button2.Text = "开始";
                }
                else
                {
                    Console.WriteLine("未连接");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            List<byte[]> to_send = new List<byte[]>();
            to_send.Add(new byte[] { (byte)'G', (byte)'E', (byte)'T', (byte)'X' });
            to_send.Add(BitConverter.GetBytes(0));
            to_send.Add(BitConverter.GetBytes(0));
            to_send.Add(BitConverter.GetBytes(0));
            var re = c.send_and_receive_sync(Client.byte_connect(to_send));
            int x = (BitConverter.ToInt32(re.data, 0));
            int y = (BitConverter.ToInt32(re.data, 4));

            Console.WriteLine("Received: x=" + x.ToString() + " y=" + y.ToString());
            if (recording && x != -1 && y != -1)
            {
                tpf.Add(new TimePointF(x, y, (long)Math.Round((re.time - base_time).TotalMilliseconds)));
            }
        }
    }


    public class ConsoleHelper : TextWriter
    {

        int count = 0;

        private System.Windows.Forms.TextBox _textBox { set; get; }//如果是wpf的也可以换做wpf的输入框

        public ConsoleHelper(System.Windows.Forms.TextBox textBox)
        {
            this._textBox = textBox;
            Console.SetOut(this);
        }

        public override void Write(string value)
        {
            if (_textBox.IsHandleCreated)
                _textBox.BeginInvoke(new ThreadStart(() =>
                {
                    _textBox.AppendText("[" + count.ToString() + "]" + value + " ");
                    count++;
                }));
        }

        public override void WriteLine(string value)
        {
            if (_textBox.IsHandleCreated)
                _textBox.BeginInvoke(new ThreadStart(() =>
                {
                    _textBox.AppendText("[" + count.ToString() + "]" + value + "\r\n");
                    count++;
                }));
        }

        public override Encoding Encoding//这里要注意,重写wirte必须也要重写编码类型
        {
            get { return Encoding.UTF8; }
        }


    }
}
