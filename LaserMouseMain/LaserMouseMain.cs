using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaserMouseCore;

namespace LaserMouseMain
{
    class LaserMouseMain
    {
        static void Main(string[] args)
        {
            Client c = new Client();
            Recognize r = new Recognize();
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
                        
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
