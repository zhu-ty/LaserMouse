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
        static bool loop = false;

        const int MAX_NODE = 1000;

        const int offsetX = 0;
        const int offsetY = 0;
        const int X_MAX = 1366;
        const int Y_MAX = 768;

        static void Main(string[] args)
        {
            r.ResultsCalculatedEvent += new RecognizeCoreEntry.ResultEventHandler(ResultEvent);
            base_time = DateTime.Now;
            const int tar_port = 986;
            Mutex m = new Mutex();
            r.add_gesture("circle.xml");
            r.add_gesture("N.xml");
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
                    if (split[0] == "r")
                    {
                        if (point_list.Count > 0)
                        {
                            var a = r.get_results(point_time_list, point_list);
                        }
                    }
                    if (split[0] == "m")
                    {
                        LaserMouseCore.Mouse_Keyboard_Press.mouse_move(
                            int.Parse(split[1]), int.Parse(split[2]), X_MAX, Y_MAX);
                    }
                    if (split[0] == "sm")
                    {
                        m.WaitOne();
                        loop = true;
                        m.ReleaseMutex();
                        start_mouse(m);
                    }
                    if (split[0] == "em")
                    {
                        m.WaitOne();
                        loop = false;
                        m.ReleaseMutex();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void ResultEvent(object obj, RecognizeCoreEntry.ResultEventArgs e)
        {
            Console.WriteLine(e.results.Max.result + " " + e.results.Max.percent.ToString() + "%");
        }

        static async void start_mouse(Mutex m)
        {
            await Task.Run( () =>
            {
                while (true)
                {
                    m.WaitOne();
                    if (!loop)
                    {
                        m.ReleaseMutex();
                        break;
                    }
                    m.ReleaseMutex();
                    var tak = send_rev_task("GET");
                    while (!tak.IsCompleted) ;
                    list_lock.WaitOne();
                    if (point_list.Count > 0)
                    {
                        LaserMouseCore.Mouse_Keyboard_Press.mouse_move(
                             (int)point_list.Last.Value.X * X_MAX / 10000,
                             (int)point_list.Last.Value.Y * Y_MAX / 10000,
                             X_MAX, Y_MAX);
                    }
                    list_lock.ReleaseMutex();
                    System.Threading.Thread.Sleep(10);
                }
            });
        }

        static void send_rev_task_sync(string mes)
        {
            byte[] mes_ = Encoding.ASCII.GetBytes(mes);
            byte[] send_ = new byte[Client.data_len];
            mes_.CopyTo(send_, 0);
            var rec = c.send_and_receive_sync(send_);
            if (rec.data != null)
                add_data_sync(rec);
            if (point_list.Count > 0)
            {
                Console.WriteLine(point_time_list.Last.Value.ToString() + " " +
                    point_list.Last.Value.ToString());
            }
        }

        static async Task send_rev_task(string mes)
        {
            byte[] mes_ = Encoding.ASCII.GetBytes(mes);
            byte[] send_ = new byte[Client.data_len];
            mes_.CopyTo(send_, 0);
            var rec = await c.send_and_receive(send_);
            if (rec.data != null)
                await add_data(rec);
            list_lock.WaitOne();
            if (point_list.Count > 0)
            {
                Console.WriteLine(point_time_list.Last.Value.ToString() + " " +
                    point_list.Last.Value.ToString());
            }
            list_lock.ReleaseMutex();
        }

        static async void send_rev(string mes)
        {
            byte[] mes_ = Encoding.ASCII.GetBytes(mes);
            byte[] send_ = new byte[Client.data_len];
            mes_.CopyTo(send_, 0);
            var rec = await c.send_and_receive(send_);
            if(rec.data != null)
                await add_data(rec);
            list_lock.WaitOne();
            if (point_list.Count > 0)
            {
                Console.WriteLine(point_time_list.Last.Value.ToString() + " " +
                    point_list.Last.Value.ToString());
            }
            list_lock.ReleaseMutex();
        }

        static void add_data_sync(Client.ReceiveEventArgs data)
        {
            int pt = BitConverter.ToInt32(data.data, 8);
            if (pt == 0)
            {
                if (point_time_list.Count != 0)
                {
                    var wait = r.get_results(point_time_list, point_list);
                    while (!wait.IsCompleted) ;
                }
                point_time_list.Clear();
                point_list.Clear();
                return;
            }
            float x = (float)(BitConverter.ToInt32(data.data, 0));
            float y = (float)(BitConverter.ToInt32(data.data, 4));
            long t = (long)Math.Round((data.time - base_time).TotalMilliseconds);
            if (point_time_list.Count >= MAX_NODE)
            {
                point_time_list.RemoveFirst();
                point_list.RemoveFirst();
            }
            point_time_list.AddLast(t);
            point_list.AddLast(new PointF(x, y));
        }

        static async Task add_data(Client.ReceiveEventArgs data)
        { 
            int pt = BitConverter.ToInt32(data.data, 8);
            if (pt == 0)
            {
                list_lock.WaitOne();
                if (point_time_list.Count != 0)
                {
                    await r.get_results(point_time_list, point_list);
                }
                point_time_list.Clear();
                point_list.Clear();
                list_lock.ReleaseMutex();
                return;
            }
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
