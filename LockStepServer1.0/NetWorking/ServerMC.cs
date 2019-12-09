using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using LockStepServer1._0.Protocol;
using System.Threading;
using LockStepServer1._0.Core;
using LockStepServer1._0.NetWorking;
using Newtonsoft.Json;

namespace LockStepServer1._0.NetWorking
{
    class ServerMC
    {
        public static ServerMC This;
        Socket socket;
        public ServerMC()
        {
            This = this;
            Init();
        }
        public ServerConn[] SCS;
        public int ServerMaxNamber = 50;
        public List<ServerConn> OnlineServer;
        public ProtocolBase PB;
        private string ThreadServerInfo;
        public long Temp = 0;
        public int Te = 0;
        Thread MSG_Server_Utili = new Thread(Get_MSG_Server_Utili);
        public DateTime StarTime = DateTime.Now;
        public void Init()
        {
            SCS = new ServerConn[ServerMaxNamber];
            for (int i = 0; i < ServerMaxNamber; i++)
            {
                SCS[i] = new ServerConn();
                OnlineServer = new List<ServerConn>();
                PB = new ProtocolBytes();
            }
            MSG_Server_Utili.Start();
        }
        public int NewIndex()
        {
            if (SCS == null)
                return -1;
            for (int i = 0; i < SCS.Length; i++)
            {
                if (SCS[i] == null)
                    return i;
                if (!SCS[i].IsUse)
                    return i;
            }
            return -1;
        }
        public void Start(string IP, int Prot)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress IpAdd = IPAddress.Parse(IP);
            IPEndPoint IpEnd = new IPEndPoint(IpAdd, Prot);
            socket.Bind(IpEnd);
            socket.Listen(ServerMaxNamber);
            socket.BeginAccept(AcceptCb, null);
            Console.WriteLine("SMC 启动  " + IpEnd.ToString());
        }
        private void AcceptCb(IAsyncResult ar)
        {
            Socket RetSocket = socket.EndAccept(ar);
            int index = NewIndex();
            if (index < 0)
            {
                RetSocket.Close();
                return;
            }
            ServerConn SC = SCS[index];
            OnlineServer.Add(SC);
            SC.Init(RetSocket);
            SC.socket.BeginReceive(SC.ReadBuffer, SC.bufferCount, SC.BuffeMain(), SocketFlags.None, ReceiveCb, SC);
            socket.BeginAccept(AcceptCb, null);
        }
        private void ReceiveCb(IAsyncResult ar)
        {
            ServerConn SC = (ServerConn)ar.AsyncState;
            try
            {
                if (SC.IsUse == false)
                    return;
                int count = SC.socket.EndReceive(ar);
                if (count <= 0)
                {
                    SC.Close();
                    OnlineServer.Remove(SC);
                    return;
                }
                SC.bufferCount += count;
                PrcessByte(SC);
                SC.socket.BeginReceive(SC.ReadBuffer, SC.bufferCount, SC.BuffeMain(), SocketFlags.None, ReceiveCb, SC);
            }
            catch (Exception e)
            {
                SC.Close();
                OnlineServer.Remove(SC);
            }
        }
        private void PrcessByte(ServerConn SC)
        {
            try
            {
                if (SC.bufferCount < sizeof(Int32))//接收到消息小于包头长度
                    return;
                Array.Copy(SC.ReadBuffer, SC.lenbyte, sizeof(Int32));
                SC.MsgLen = BitConverter.ToInt32(SC.lenbyte, 0);
                if (SC.bufferCount < SC.MsgLen + sizeof(Int32))
                    return;
                ProtocolBase Pb = PB.Decode(SC.ReadBuffer, sizeof(Int32), SC.MsgLen);
                ProtocolBytes pb = (ProtocolBytes)Pb;
                object[] vs = new object[] { SC, pb };
                object O = vs;
                Thread Hand = new Thread(new ParameterizedThreadStart(HandMSG));
                Hand.Name = DateTime.Now.ToLocalTime().ToString();
                Hand.Start(O);
                //HandMSG(SC, pb);
                int count = SC.bufferCount - SC.MsgLen - sizeof(Int32);
                Array.Copy(SC.ReadBuffer, sizeof(Int32) + SC.MsgLen, SC.ReadBuffer, 0, count);
                SC.bufferCount = count;
                if (SC.bufferCount > 0)
                    PrcessByte(SC);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "115");
            }

        }
        private void HandMSG(object O)
        {
            object[] vs = (object[])O;
            ServerConn SC = (ServerConn)vs[0]; ProtocolBytes PB = (ProtocolBytes)vs[1];
            try
            {

                object[] OB = PB.GetDecode();
                string ProtoName = OB[0].ToString();
                if (ProtoName == ServerMCVar.ServerInfo)
                {
                    SC.Server_Vrsion = OB[1].ToString();
                    SC.IP = SC.GetAddress();  
                    SC.Port = (int)OB[3];
                    (SC.Server_Vrsion + " " + SC.IP + " " + SC.Port).ColorWord(ConsoleColor.DarkRed);
                }
                if (ProtoName == "RetServer")
                {
                    return;
                    //string S = (float)OB[1] + "%";
                    //S.ColorWord(ConsoleColor.Green);    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " HandMSG");
            }

        }
        public void Send(ServerConn SC, ProtocolBytes PBS)
        {
            byte[] bytes = PBS.Encode();
            byte[] Lenght = BitConverter.GetBytes(bytes.Length);
            byte[] SendBuff = Lenght.Concat(bytes).ToArray();
            try
            {
                SC.socket.BeginSend(SendBuff, 0, SendBuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                return;
            }

        }
        public void GetServer()
        {
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData("GetServer");
            foreach (ServerConn SC in OnlineServer)
            {
                SC.Send(bytes);
            }
        }
        private static void Get_MSG_Server_Utili()
        {
            while (true)
            {
                This.GetServer();
                Thread.Sleep(5000);
            }
        }
        public string GetServerInfo(ProtocolBytes bytes, TCP conn)
        {
            object[] vs = new object[] { bytes, conn };
            object O = vs;
            Thread ThreadGetServerInfo = new Thread(new ParameterizedThreadStart(GetINFO));
            ThreadGetServerInfo.Name = "ThreadGetServerInfo";
            ThreadGetServerInfo.Start(O);
            return ThreadServerInfo;
        }
        public void GetINFO(object O)
        {
            string serverinfoStr;
            ServerInfo serverinfo = new ServerInfo();
            ServerConn[] ServerArr = new ServerConn[OnlineServer.Count];
            object[] vs = (object[])O;
            ProtocolBytes bytes = (ProtocolBytes)vs[0];
            TCP conn = (TCP)vs[1];
            int i = 0;
            foreach (ServerConn SC in OnlineServer)
            {
                ServerArr[i] = SC;
                i++;
            }
            for (int a = 0; a < ServerArr.Length - 1; a++)
            {
                for (int b = a + 1; b < ServerArr.Length; b++)
                {
                    if (ServerArr[a].UNitl > ServerArr[b].UNitl)
                    {
                        ServerConn Temp = ServerArr[a];
                        ServerArr[a] = ServerArr[b];
                        ServerArr[b] = Temp;
                    }
                }
            }
            if (ServerArr.Length == 0)
            {
                bytes.AddData(1);
                conn.Send(bytes);
                "MSG_Server: 没有启动".ColorWord(ConsoleColor.DarkRed);
                return;
            }
            bytes.AddData(0);
            serverinfo.IP = ServerArr[0].IP;
            serverinfo.Port = ServerArr[0].Port;
            serverinfo.ServerVrsion = ServerArr[0].Server_Vrsion;
            serverinfoStr = JsonConvert.SerializeObject(serverinfo);

            Console.WriteLine("ToClient " + serverinfo.IP + " " + serverinfo.Port.ToString());
            bytes.AddData(serverinfoStr);
            bytes.AddData(conn.Player.ReConnectCheckCode);
            conn.Send(bytes);
        }
        public void printinfo()
        {
            Console.WriteLine("==============SCS==============");
            for (int i = 0; i < SCS.Length; i++)
            {
                if (SCS[i].IsUse)
                    SCS[i].IP.ColorWord(ConsoleColor.Red);
            }
            Console.WriteLine("===============================");
            Console.WriteLine("=============Online============" + OnlineServer.Count);
            foreach (ServerConn SC in OnlineServer)
            {
                SC.Server_Vrsion.ColorWord(ConsoleColor.Green);
            }
            Console.WriteLine("===============================");
        }
    }
}
