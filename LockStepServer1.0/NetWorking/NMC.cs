
#define _DEBUG
using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LockStepServer1._0.Logic;
using LockStepServer1._0.Core;
using System.Threading;

namespace LockStepServer1._0.NetWorking
{
    class NMC
    {
        private UDP m_UDP;
        private TCP m_TCP;
        public Socket listenfd;
        public TCP[] tcps;
        public int maxConn = 30000;
        public static NMC instance;
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        public long hearBeatTime = 300;
        public ProtocolBase Proto;
        //消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
        public HandFPSMsg handFPSMsg = new HandFPSMsg();
        public HandFriendEvent HFE = new HandFriendEvent();
        public NMC()
        {
            instance = this;
        }
        public int NewIndex()
        {
            if (tcps == null)
                return -1;
            for (int i = 0; i < tcps.Length; i++)
            {
                if (tcps[i] == null)
                {
                    tcps[i] = new TCP();
                    return i;
                }
                else if (tcps[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }
        public void Start(string host, int port)
        {
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(HandleMainTimer);
            //初始化连接池
            tcps = new TCP[maxConn];
            //for (int i = 0; i < tcps.Length; i++)
            //{
            //    tcps[i] = new TCP();
            //}
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAdd = IPAddress.Parse(host);
            IPEndPoint iPEnd = new IPEndPoint(iPAdd, port);
            Console.WriteLine("NMC  " + iPEnd.ToString());
            try
            {
                listenfd.Bind(iPEnd);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            listenfd.Listen(maxConn);
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("【服务器】启动成功");
        }

        public void HandleMainTimer(object sender, ElapsedEventArgs e)
        {
            //处理心跳
            hearBeat();
            timer.Start();
        }

        public void hearBeat()
        {
            long timeNow = Sys.GetTimeStamp();
            for (int i = 0; i < tcps.Length; i++)
            {
                TCP conn = tcps[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;
                if (conn.lastTickTime < timeNow - hearBeatTime)
                {
                    Console.WriteLine("[心跳引起断开连接]" + conn.GetAddress());
                    lock (conn)
                        CloseTCP(conn);
                    Console.WriteLine("断开");
                }
            }
        }

        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket sock = listenfd.EndAccept(ar);
                int index = NewIndex();
                tcps[index] = new TCP();
                if (index < 0)
                {
                    sock.Close();
                    Console.WriteLine("【警告】连接已满");
                }
                else
                {
                    TCP conn = tcps[index];
                    conn.Init(sock);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端连接[" + adr + "]连接池connsID: " + index);
                    conn.socket.BeginReceive(conn.readbuffer, conn.buffercount, conn.BuffRmain(), SocketFlags.None, ReceiveCb, conn);
                    listenfd.BeginAccept(AcceptCb, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败" + e.Message);
            }
        }
        public void ReceiveCb(IAsyncResult ar)
        {
            TCP conn = (TCP)ar.AsyncState;
            if (conn.isUse == false)
                return;
            try
            {
                int count = conn.socket.EndReceive(ar);
                //关闭信号
                if (count <= 0)
                {
                    Console.WriteLine("收到[" + conn.GetAddress() + "]断开连接");
                    NMC.instance.CloseTCP(conn);
                    return;
                }
                conn.buffercount += count;
                ProcessData(conn);
                //继续接收
                if (conn.isUse && conn.socket != null)
                {
                    conn.socket.BeginReceive(conn.readbuffer, conn.buffercount, conn.BuffRmain(), SocketFlags.None, ReceiveCb, conn);
                }
            }
            catch (Exception e)
            {
                NMC.instance.CloseTCP(conn);
                Console.WriteLine("【异常】" + e.Message);
            }
        }

        private void ProcessData(object con)
        {
            TCP conn = (TCP)con;
            if (conn.buffercount < sizeof(Int32))
                return;
            //
            Array.Copy(conn.readbuffer, conn.lenbytes, sizeof(Int32));
            conn.msgLenght = BitConverter.ToInt32(conn.lenbytes, 0);
            if (conn.buffercount < conn.msgLenght + sizeof(Int32))
                return;
            //处理消息
            ProtocolBase protoco = Proto.Decode(conn.readbuffer, sizeof(Int32), conn.msgLenght);
            try
            {
                HandleMsg(conn, protoco);
            }
            catch (Exception e)
            {
                Console.WriteLine("HandleMsg  " + e.Message);
            }
            //发送消息
            int count = conn.buffercount - conn.msgLenght - sizeof(Int32);
            Array.Copy(conn.readbuffer, sizeof(Int32) + conn.msgLenght, conn.readbuffer, 0, count);
            conn.buffercount = count;
            if (conn.buffercount > 0)
            {
                ProcessData(conn);
            }
        }

        private void HandleMsg(TCP conn, ProtocolBase protoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protoBase;
            // Console.WriteLine("收到" + bytes.ProtocolName().ToString());
            string name = bytes.ProtocolName().ToString();
            object[] OB = bytes.GetDecode();
            string s = "";
            for (int i = 0; i < bytes.GetDecode().Length; i++)
            {
                s += bytes.GetDecode()[i].ToString();
            }
            if (s != "HearBeat")
                Console.WriteLine(s);
            string methodname = "Msg" + name;
            if (name == "Friend")
            {
                string method = OB[1].ToString();
                MethodInfo mm = HFE.GetType().GetMethod(method);
                object[] obj = new object[] { conn, OB };
                mm.Invoke(HFE, obj);
            }
            else if (name == ProtocolConst.LockStep)
            {
                try
                {
                    MethodInfo mm = handFPSMsg.GetType().GetMethod(methodname);
                    object[] obj = new object[] { protoBase };
                    mm.Invoke(handFPSMsg, obj);
                }
                catch (Exception e)
                { Console.WriteLine(e.Message + " 221"); }
            }
            else if (name == ProtocolConst.MSG)
            {
                try
                {
                    MethodInfo mm = handleConnMsg.GetType().GetMethod(methodname);
                    if (mm == null)
                    {
                        string str = "[警告]handlemsg没有处理连接方法";
                        Console.WriteLine(str + methodname);
                    }
                    object[] obj = new object[] { protoBase };
                    mm.Invoke(handleConnMsg, obj);
                }
                catch (Exception e)
                { Console.WriteLine(e.Message + "  239"); }

            }
            else if (conn.Player == null || name == ProtocolConst.HearBeat || name == ProtocolConst.Logout || name == ProtocolConst.CheckOpenid || name == TeamVar.CreateTeam || name == TeamVar.ExitTeam || name == "JSONTEST")
            {
                try
                {
                    MethodInfo mm = handleConnMsg.GetType().GetMethod(methodname);
                    if (mm == null)
                    {
                        string str = "[警告]handlemsg没有处理连接方法";
                        Console.WriteLine(str + methodname);
                    }
                    object[] obj = new object[] { conn, protoBase };
                    try
                    {
                        mm.Invoke(handleConnMsg, obj);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "256");
                    }

                }
                catch (Exception e)
                { Console.WriteLine(e.Message + "  261"); }

            }
            else
            {
                try
                {
                    MethodInfo mm = handlePlayerEvent.GetType().GetMethod(methodname);
                    if (mm == null)
                    {
                        string str = "[警告]handlemsg没有处理玩家方法";
                        Console.WriteLine(str + methodname);
                    }
                    object[] obj = new object[] { conn, protoBase };
                    Console.WriteLine("[处理玩家消息]" + conn.GetAddress() + name);
                    mm.Invoke(handlePlayerEvent, obj);
                }
                catch (Exception e)
                { Console.WriteLine(e.Message + "  247"); }
            }
        }

        public void Send(TCP conn, ProtocolBase protoco)
        {
            byte[] bytes = protoco.Encode();
            byte[] lenght = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = lenght.Concat(bytes).ToArray();
            try
            {
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[发送消息] 错误:" + e.Message);
            }
        }
        public void Broadcast(ProtocolBytes protoco)
        {
            for (int i = 0; i < tcps.Length; i++)
            {
                if (!tcps[i].isUse) continue;
                if (tcps[i] == null) continue;
                Send(tcps[i], protoco);
#if _DEBUG
                string s = "";
                for (int x = 0; x < protoco.GetDecode().Length; x++)
                {
                    s += protoco.GetDecode()[x].ToString();
                }
                Console.WriteLine("广播：" + s);
#endif
            }
        }

        public void Close()
        {
            for (int i = 0; i < tcps.Length; i++)
            {
                TCP conn = tcps[i];
                if (conn == null)
                    continue;
                if (!conn.isUse)
                    continue;
                lock (conn)
                    conn.Close();
            }
        }
        public void CloseTCP(TCP conn)
        {
            for (int i = 0; i < tcps.Length; i++)
            {
                if (tcps[i] == conn)
                {
                    tcps[i] = null;
                }
            }
            conn.Close();
        }
        public void Print()
        {
            Console.WriteLine("=======服务器登录信息======");

            int online = 0;
            for (int i = 0; i < tcps.Length; i++)
            {
                if (tcps[i] == null) continue;
                online++;
                if (!tcps[i].isUse) continue;
                string str = "[" + tcps[i].GetAddress() + "]";
                if (tcps[i].Player != null)
                {
                    str += " 玩家ID =>" + tcps[i].Player.Openid;
                    str += " NowState =>" + tcps[i].Player.NowState;
                }
                Console.WriteLine(str);
            }
            online.ToString().ColorWord(ConsoleColor.Red);
            Console.WriteLine("==========================");
        }
    }
}
