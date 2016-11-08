﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace LaserMouseMain
{
    class Client
    {
        /// <summary>
        /// 连接最长等待时间
        /// </summary>
        public const int max_connect_senconds = 10;

        public const int data_len = 16;
        

        public bool connect(IPAddress target_ip,int listen_port)
        {
            //socket_lock.WaitOne();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult connect_result = socket.BeginConnect(target_ip, listen_port, null, null);
            connect_result.AsyncWaitHandle.WaitOne(max_connect_senconds * 1000);//10s
            if (!connect_result.IsCompleted)
            {
                socket.Close();
                return false;
            }
            //socket_lock.ReleaseMutex();
            return true;
        }

        public async Task<ReceiveEventArgs> send_and_receive(byte[] buffer)
        {
            ReceiveEventArgs ret = new ReceiveEventArgs();
            try
            {
                socket.Send(buffer);
                byte[] rec_buf = new byte[data_len];
                await Task.Run(()=>
                {
                    socket.Receive(rec_buf);
                });
                ret.data = rec_buf;
                ret.time = DateTime.Now;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ret;
        }


        public class ReceiveEventArgs
        {
            public byte[] data;
            public DateTime time;
        }

        //private Mutex socket_lock = new Mutex();
        private Socket socket;
        private byte[] byte_connect(List<byte[]> btlist)
        {
            int length = 0;
            int now = 0;
            for (int i = 0; i < btlist.Count; i++)
                length += btlist[i].Length;
            byte[] ret = new byte[length];
            for (int i = 0; i < btlist.Count; i++)
            {
                Array.Copy(btlist[i], 0, ret, now, btlist[i].Length);
                now += btlist[i].Length;
            }
            return ret;
        }
    }
}