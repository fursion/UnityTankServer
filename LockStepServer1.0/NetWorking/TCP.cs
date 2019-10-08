using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.NetWorking
{
    class TCP
    {
        public Socket socket;
        public byte[] readbuffer;
        public bool isUse = false;
        public const int BUFFER_SIZE = 1024;
        public int buffercount = 0;

        //粘包分包
        public byte[] lenbytes = new byte[sizeof(Int32)];
        public Int32 msgLenght = 0;
        //心跳协议
        public long lastTickTime = long.MinValue;
       // public Player Player;
        public TCP()
        {
            readbuffer = new byte[BUFFER_SIZE];

        }
        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffercount = 0;
            //心跳处理
            lastTickTime = Sys.GetTimeStamp();
        }
        public int BuffRmain()
        {
            return BUFFER_SIZE - buffercount;
        }
        public string GetAddress()
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }

        public void Send(ProtocolBytes protoco)
        {
            ServerNet.instance.Send(this, protoco);
        }

        public void Close()
        {
            if (!isUse)
            {
                return;
            }
            if (Player == null)
            {
                //处理玩家
                Console.WriteLine("处理玩家");
                return;
            }
            Console.WriteLine("断开连接" + GetAddress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;
        }
    }
}
