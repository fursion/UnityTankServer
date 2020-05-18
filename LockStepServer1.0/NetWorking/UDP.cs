using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using LockStepServer1._0.Core;

using LockStepServer1._0.LockStep;
using System.Threading;
using System.Reflection;
using LockStepServer1._0.Logic;
using LockStepServer1._0.ROOM;
using Newtonsoft.Json;
using Fursion.Protocol;

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
        byte[] recvbytes = new byte[1024 * 4];
        byte[] sendbytes = new byte[1024 * 4];
        int recvLenght;
        private byte[] msglenBytes = new byte[sizeof(Int32)];
        private int msglenght;
        public ProtocolBase MsgProto = new ProtocolBytes();
        public List<EndPoint> UDPList = new List<EndPoint>();
        public Dictionary<EndPoint, Player> valuePairs = new Dictionary<EndPoint, Player>();
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
            Console.WriteLine("UDP SUCCESS");
            connectThread = new Thread(new ThreadStart(SocketReceive));
            connectThread.Start();
        }
        public void SocketSend(ProtocolBase proto, EndPoint endPoint)
        {
            byte[] bytes = proto.Encode();
            byte[] bytelenght = BitConverter.GetBytes(bytes.Length);
            byte[] senddata = bytelenght.Concat(bytes).ToArray();
            socket.SendTo(senddata, senddata.Length, SocketFlags.None, endPoint);
        }
        //
        public void SocketReceive()
        {
            while (true)
            {
                recvbytes = new byte[1024 * 4];
                EndPoint Client;
                try
                {
                    recvLenght = socket.ReceiveFrom(recvbytes, ref clientEnd);
                    Client = clientEnd;
                }
                catch (Exception e)
                {
                    return;
                }
                //Console.WriteLine("message from: " + clientEnd.ToString());
                recvStr = Encoding.ASCII.GetString(recvbytes, 0, recvLenght);
                HandData(Client);
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
        private void HandData(EndPoint Client)
        {
            if (recvLenght < sizeof(Int32))
                return;
            Array.Copy(recvbytes, msglenBytes, sizeof(Int32));
            msglenght = BitConverter.ToInt32(msglenBytes, 0);
            if (recvLenght < msglenght + sizeof(Int32))
                return;
            ProtocolBase proto = MsgProto.Decode(recvbytes, sizeof(Int32), msglenght);// MsgProto.Decode(recvbytes, sizeof(Int32), msglenght);
            HangMsg(proto, Client);
        }
        private void HangMsg(ProtocolBase protoc, EndPoint Client)
        {
            ProtocolBytes Data = (ProtocolBytes)protoc;
            object[] dat = Data.GetDecode();
            string MsgName = Data.Protocol.ToString();
            Console.WriteLine(MsgName);
            if (Data.Protocol == Fursion_Protocol.UDPInit)
            {
                lock (valuePairs)
                {
                    string openid = dat[1].ToString();
                    Thread bindingPlayerT = new Thread(new ParameterizedThreadStart(BindingPlayer))
                    {
                        Name = "BindingPlayer"
                    };
                    object[] vs = new object[] { openid, Client };
                    bindingPlayerT.Start(vs);
                    //FriendMC.A.OnlinePlayerList[openid].UDPClient = Client;
                    //FriendMC.A.OnlinePlayerList[openid].room.P_UDP_IP.Add(Client);
                    //valuePairs.Add(Client, FriendMC.A.OnlinePlayerList[openid]);
                }
            }
            else if (Data.Protocol == Fursion_Protocol.SelectMode)
            {
                lock (valuePairs)
                {
                    SelectModel selectModel = JsonConvert.DeserializeObject<SelectModel>(dat[1].ToString());
                    if (valuePairs.ContainsKey(Client))
                        valuePairs[Client].TempData.PlayerGameInfo.SelectModelNumber = selectModel.SelectedModel;
                }
            }
            else if (Data.Protocol == Fursion_Protocol.LockSelect)
            {
                lock (valuePairs)
                {
                    valuePairs[Client].TempData.PlayerGameInfo.LockSelect = true;
                    valuePairs[Client].Room.LockSelect();
                }
            }
            else if (Data.Protocol == Fursion_Protocol.SynTest)
            {
                SynTest.The.P_UDP_IP.Add(clientEnd);
                SynTest.The.Start();
                Console.WriteLine(SynTest.The.P_UDP_IP.Count);
            }
            else if (Data.Protocol == Fursion_Protocol.SynTestInstruct)
            {
                InstructFrame instruct = JsonConvert.DeserializeObject<InstructFrame>(dat[1].ToString());
                SynTest.The.mGR.PackageLogicFrame(instruct);
            }
            else if (Data.Protocol == Fursion_Protocol.LockStep_Instruct)
            {
                lock (valuePairs)
                {

                    InstructFrame instruct = JsonConvert.DeserializeObject<InstructFrame>(dat[1].ToString());
                    if (valuePairs.ContainsKey(Client))
                        valuePairs[Client].Room.LSM.PackageLogicFrame(instruct);
                }
            }
            else if (Data.Protocol == Fursion_Protocol.Loading)
            {
                lock (valuePairs)
                {
                    if (valuePairs.ContainsKey(Client))
                        valuePairs[Client].Room.LoadingMethod(valuePairs[Client], dat);
                }
            }
            else
            {
                ProtocolBytes send = new ProtocolBytes();
                send.AddData(" From server " + MsgName);
                SocketSend(send, clientEnd);
            }
        }

        private void BindingPlayer(object obj)
        {
            object[] vs = (object[])obj;
            string openid = vs[0].ToString();
            EndPoint Client = (EndPoint)vs[1];
            for (int i = 0; i < NMC.instance.tcps.Length; i++)
            {
                try
                {
                    if (NMC.instance.tcps[i] == null)
                        continue;
                    TCP Conn = NMC.instance.tcps[i];
                    if (Conn == null)
                        continue;
                    if (Conn.Player == null)
                        continue;
                    if (Conn.Player.Openid == openid)
                    {
                        Conn.Player.UDPClient = Client;
                        Conn.Player.Room.RightMethod(Conn.Player);
                        Conn.Player.Room.UDP_ClientList.Add(Client);
                        if (!valuePairs.ContainsKey(Client))
                            valuePairs.Add(Client, Conn.Player);
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
