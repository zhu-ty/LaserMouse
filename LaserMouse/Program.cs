using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaserMouseCore;
using System.Drawing;
using WobbrockLib;
using WobbrockLib.Genetic;

namespace LaserMouseCore
{
    class Recognize
    {

        public bool add_gesture(string filename)
        {
            return r.LoadGesture(filename);
        }

        public async Task<bool> get_results(Dictionary<long, PointF> time_point_list, bool golden = true)
        {
            bool ret = false;
            List<TimePointF> tpf = new List<TimePointF>();
            foreach (KeyValuePair<long, PointF> iter in time_point_list)
            {
                tpf.Add(new TimePointF(iter.Value, iter.Key));
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

        public delegate void ResultEventHandler(object obj, ResultEventArgs e);

        Recognizer r = new Recognizer();
    }
}
