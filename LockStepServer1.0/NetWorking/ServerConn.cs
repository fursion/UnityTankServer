
using LockStepServer1._0.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Fursion.Protocol;
using System.Text;
using System.Threading.Tasks;


namespace LockStepServer1._0.NetWorking
{
    public class ServerConn
    {
        public Socket socket;
        public float UNitl = 0;
        public string IP;
        public int Port;
        public string Server_Vrsion;
        public bool IsUse = false;
        public int BUFFER_SIZE = 2048;
        public byte[] ReadBuffer;
        public int bufferCount = 0;
        public byte[] lenbyte = new byte[sizeof(Int32)];
        public Int32 MsgLen = 0;
        public void Init(Socket SK)
        {
            IsUse = true;
            ReadBuffer = new byte[BUFFER_SIZE];
            socket = SK;
            bufferCount = 0;
        }
        public int BuffeMain()
        {
            return BUFFER_SIZE - bufferCount;
        }
        public void Close()
        {
            if (socket == null)
                return;
            //if (!socket.Connected)
            //    return;
            if (!IsUse)
                return;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            IsUse = false;
        }
        public void Send(ProtocolBytes PB)
        {
            ServerMC.This.Send(this, PB);
        }
        public string GetAddress()
        {
            if (!IsUse)
                return "无法获取地址";
            return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
        }
    }
}