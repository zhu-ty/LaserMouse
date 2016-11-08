using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaserMouseCore;
using System.Drawing;
using System.Threading;

namespace LaserMouseMain
{
    class LaserMouseMain
    {
        static LinkedList<long> point_time_list = new LinkedList<long>();
        static LinkedList<PointF> point_list = new LinkedList<PointF>();
        static Mutex list_lock = new Mutex();
        static Client c = new Client();
        static RecognizeCoreEntry r = new RecognizeCoreEntry();
        static DateTime base_time;

        const int MAX_NODE = 1000;

        const int offsetX = 0;
        const int offsetY = 0;
        const int X_MAX = 1366;
        const int Y_MAX = 768;

        static void Main(string[] args)
        {
            base_time = DateTime.Now;
            const int tar_port = 5874;
            while (true)
            {
                try
                {
                    string command = Console.ReadLine();
                    if (command == "e")
                        break;
                    string[] split = command.Split(new char[] { ' ' });
                    if (split[0] == "c")
                    {
                        System.Net.IPAddress ip = System.Net.IPAddress.Parse(split[1]);
                        c.connect(ip, tar_port);
                    }
                    if (split[0] == "s")
                    {
                        send_rev(split[1]);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static async void send_rev(string mes)
        {
            byte[] mes_ = Encoding.ASCII.GetBytes(mes);
            var rec = await c.send_and_receive(mes_);
            if(rec.data != null)
                add_data(rec);
        }

        static void add_data(Client.ReceiveEventArgs data)
        {
            float x = (float)(BitConverter.ToInt32(data.data, 0));
            float y = (float)(BitConverter.ToInt32(data.data, 4));
            long t = (long)Math.Round((data.time - base_time).TotalMilliseconds);
            list_lock.WaitOne();
            if (point_time_list.Count >= MAX_NODE)
            {
                point_time_list.RemoveFirst();
                point_list.RemoveFirst();
            }
            point_time_list.AddLast(t);
            point_list.AddLast(new PointF(x, y));
            list_lock.ReleaseMutex();
        }
    }
}
