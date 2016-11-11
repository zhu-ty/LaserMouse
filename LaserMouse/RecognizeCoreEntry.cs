using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaserMouseCore;
using System.Drawing;
using WobbrockLib;
using WobbrockLib.Genetic;
using System.Runtime.InteropServices;

namespace LaserMouseCore
{
    public class RecognizeCoreEntry
    {

        public bool add_gesture(string filename)
        {
            return r.LoadGesture(filename);
        }

        public async Task<bool> get_results(LinkedList<long> time_p, LinkedList<PointF> point_p, bool golden = true)
        {
            bool ret = false;
            List<TimePointF> tpf = new List<TimePointF>();
            LinkedListNode<long> node_t = time_p.First;
            LinkedListNode<PointF> node_p = point_p.First;
            for (int i = 0; i < time_p.Count; i++)
            {
                tpf.Add(new TimePointF(node_p.Value, node_t.Value));
                node_p = node_p.Next;
                node_t = node_t.Next;
            }
            ResultEventArgs rea = new ResultEventArgs();
            await Task.Run(() =>
            {
                var re_list = r.Recognize(tpf, golden);
                int c = re_list.Names.Length;
                for (int i = 0; i < c; i++)
                {
                    rea.add_result(re_list[i].Name, re_list[i].Score);
                }
            });
            ResultsCalculatedEvent(this, rea);
            ret = true;
            return ret;
        }

        /// <summary>
        /// How to use:
        /// <para>Recognize e;</para>
        /// <para>e.ResultsCalculatedEvent += new ResultEventHandler([your function])</para>
        /// </summary>
        public event ResultEventHandler ResultsCalculatedEvent;

        public delegate void ResultEventHandler(object obj, ResultEventArgs e);

        private Recognizer r = new Recognizer();

        public class ResultEventArgs
        {
            /// <summary>
            /// Tips: Use "results.Max" to get the best result.
            /// </summary>
            public SortedSet<result_pair> results = new SortedSet<result_pair>();
            public bool add_result(string result, double percent)
            {
                return results.Add(new result_pair(percent, result));
            }

            public struct result_pair : IComparable
            {
                public double percent;
                public string result;

                public result_pair(double _percent, string _result)
                {
                    percent = _percent;
                    result = _result;
                }

                public int CompareTo(object obj)
                {
                    return percent.CompareTo(((result_pair)obj).percent);
                }
            }
        }
    }

    public class Mouse_Keyboard_Press
    {
        /// <summary>
        /// 移动鼠标，注意x和y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="basex">横向分辨率</param>
        /// <param name="basey">纵向分辨率</param>
        /// <returns></returns>
        public static bool mouse_move(int x, int y, int basex, int basey)
        {
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x * 65536 / basex, y * 65536 / basey, 0, 0);
            return true;
        }
        /// <summary>
        /// 按相对坐标移动鼠标，注意x和y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="basex">横向分辨率</param>
        /// <param name="basey">纵向分辨率</param>
        /// <returns></returns>
        public static bool mouse_move_relative(int x, int y, int basex, int basey)
        {
            mouse_event(MOUSEEVENTF_MOVE, x * 65536 / basex, y * 65536 / basey, 0, 0);
            return true;
        }
        /// <summary>
        /// 左键按下
        /// </summary>
        /// <returns></returns>
        public static bool mouse_left_down()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            return true;
        }
        /// <summary>
        /// 左键抬起
        /// </summary>
        /// <returns></returns>
        public static bool mouse_left_up()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            return true;
        }
        /// <summary>
        /// 右键按下
        /// </summary>
        /// <returns></returns>
        public static bool mouse_right_down()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            return true;
        }
        /// <summary>
        /// 右键抬起
        /// </summary>
        /// <returns></returns>
        public static bool mouse_right_up()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            return true;
        }

        /// <summary>
        /// 键值表：https://msdn.microsoft.com/en-us/library/dd375731(v=vs.85).aspx
        /// </summary>
        /// <param name="key_value"></param>
        /// <returns></returns>
        public static bool key_down(byte key_value)
        {
            keybd_event(key_value, 0, 0, 0);
            return true;
        }

        /// <summary>
        /// 键值表：https://msdn.microsoft.com/en-us/library/dd375731(v=vs.85).aspx
        /// </summary>
        /// <param name="key_value"></param>
        /// <returns></returns>
        public static bool key_up(byte key_value)
        {
            keybd_event(key_value, 0, 2, 0);
            return true;
        }

        #region dll_import
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(
          byte bVk,    //虚拟键值
          byte bScan,// 一般为0
          int dwFlags,  //这里是整数类型  0 为按下，2为释放
          int dwExtraInfo  //这里是整数类型 一般情况下设成为 0
        );
        #endregion
    }
}
