using LockStepServer1._0.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Fursion.Protocol;

namespace LockStepServer1._0.NetWorking
{
    class TCP
    {
        public Socket socket;
        public byte[] readbuffer;
        public bool isUse = false;
        public const int BUFFER_SIZE = 2048;
        public int buffercount = 0;

        //粘包分包
        public byte[] lenbytes = new byte[sizeof(Int32)];
        public Int32 msgLenght = 0;
        //心跳协议
        public long lastTickTime = long.MinValue;
        public Player Player;//用户数据
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
                return "Unable to get IPAdress";
            return socket.RemoteEndPoint.ToString();
        }

        public void Send(ProtocolBytes protoco)
        {
            NMC.instance.Send(this, protoco);
        }

        public void Close()
        { 
            if (Player != null)
            {
                if (Player.NowState == PlayerState.Playing)
                    return;
                //Processing Player
                Player.Close();
                Player = null;
                Console.WriteLine("Player Processing Save Player Data");
                //return;
            }
            if (socket == null)
                return;
            if (!isUse)
            {
                return;
            }
            if (!socket.Connected)
                return;
            Console.WriteLine("DisConnect");
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;         
        }
    }
}
