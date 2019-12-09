using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using LockStepServer1._0.Core;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.LockStep;
using System.Threading;
using System.Reflection;
using LockStepServer1._0.Logic;
using LockStepServer1._0.ROOM;

namespace LockStepServer1._0.NetWorking
{
    class UDP
    {
        public static UDP instance;
        public Socket socket;
        public EndPoint clientEnd;//客户端
        public IPEndPoint ipEnd;//监听端口
        string recvStr;
        string SendStr;
        byte[] recvbytes = new byte[1024];
        byte[] sendbytes = new byte[1024];
        int recvLenght;
        private byte[] msglenBytes = new byte[sizeof(Int32)];
        private int msglenght;
        public ProtocolBase MsgProto = new ProtocolBytes();
        public List<EndPoint> UDPList = new List<EndPoint>();
        public  AssembleFrame ASSF = new AssembleFrame();
        private HandFPSMsg handFPSMsg = new HandFPSMsg();
        Thread connectThread;//线程
        public UDP()
        {
            instance = this;
        }
        public void InitSocket()
        {
            ipEnd = new IPEndPoint(IPAddress.Any, 8001);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ipEnd);//服务端
            //定义客户端
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            clientEnd = (EndPoint)sender;
            Console.WriteLine(sender.ToString());
            Console.WriteLine("waiting for UDP dgram");
            connectThread = new Thread(new ThreadStart(SocketReceive));
            connectThread.Start();
        }
        public void SocketSend(ProtocolBase proto, EndPoint endPoint)
        {
            byte[] bytes = proto.Encode();
            byte[] bytelenght = BitConverter.GetBytes(bytes.Length);
            byte[] senddata = bytelenght.Concat(bytes).ToArray();
            Console.WriteLine(clientEnd);
            socket.SendTo(senddata, senddata.Length, SocketFlags.None, endPoint);
        }
        //
        public void SocketReceive()
        {
            while (true)
            {
                recvbytes = new byte[1024];
                try
                {
                    recvLenght = socket.ReceiveFrom(recvbytes, ref clientEnd);
                }
                catch (Exception e)
                {
                    return;
                }
                Console.WriteLine("message from: " + clientEnd.ToString());
                recvStr = Encoding.ASCII.GetString(recvbytes, 0, recvLenght);
                HandData();
            }
        }
        void SocketQuit()
        {
            //关闭线程
            if (connectThread != null)
            {
                connectThread.Interrupt();
                connectThread.Abort();
            }
            //关闭Socket
            if (socket != null)
                socket.Close();
            Console.WriteLine("disconnect");

        }
        public void Start()
        {
            InitSocket();
        }
        private void HandData()
        {
            if (recvLenght < sizeof(Int32))
                return;
            Array.Copy(recvbytes, msglenBytes, sizeof(Int32));
            msglenght = BitConverter.ToInt32(msglenBytes, 0);
            if (recvLenght < msglenght + sizeof(Int32))
                return;
            ProtocolBase proto = MsgProto.Decode(recvbytes, sizeof(Int32), msglenght);// MsgProto.Decode(recvbytes, sizeof(Int32), msglenght);
            HangMsg(proto);
        }
        private void HangMsg(ProtocolBase protoc)
        {
            ProtocolBytes Data = (ProtocolBytes)protoc;
            object[] dat = Data.GetDecode();
            string MsgName = dat[0].ToString();
            Console.WriteLine(MsgName);
            string methodname = "Msg" + MsgName;
            if (MsgName == "UDPInit")
            {
                string openid = dat[1].ToString();
                FriendMC.A.OnlinePlayerList[openid].UDPClient = socket.RemoteEndPoint;
            }
            else if (MsgName == "FPS")
            {
                MethodInfo mm = handFPSMsg.GetType().GetMethod(methodname);
                object[] obj = new object[] { Data };
                mm.Invoke(handFPSMsg, obj);
            }
            else
            {
                ProtocolBytes send = new ProtocolBytes();
                send.Addstring(" From server " + MsgName);
                SocketSend(send, clientEnd);
            }
        }
    }
}
