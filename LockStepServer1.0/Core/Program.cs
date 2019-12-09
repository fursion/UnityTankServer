using LockStepServer1._0.Core;
using LockStepServer1._0.Logic;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.LockStep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.ROOM.Team;
using LockStepServer1._0.ROOM;
using System.Net.Sockets;
using System.Threading;

namespace LockStepServer1._0
{
    class Program
    {
        public static Thread Con;
        public static Thread MSG_Server_Utili;
        static void Main(string[] args)
        {
            //printfi();
            Console.Title = "Tank_Main_Server";
            string hostName = Dns.GetHostName();   //获取本机名
            IPHostEntry localhost = Dns.GetHostByName(hostName);    //方法已过期，可以获取IPv4的地址
            IPAddress localaddr = localhost.AddressList[0];
            string ipAddress = localaddr.ToString();
            Console.WriteLine(ipAddress);
            UDP uDP = new UDP();
            uDP.Start();
            DataMgr dataMgr = new DataMgr();//连接数据库
            NMC _NMC = new NMC();//网络连接管理
            _NMC.Start(ipAddress, 1234);
            _NMC.Proto = new ProtocolBytes();
            TeamMC teamMC = new TeamMC();//组队管理
            RoomMC roomMC = new RoomMC();//队伍管理
            FriendMC FMC = new FriendMC();//好友管理
            ServerMC SMC = new ServerMC();//服务器管理
            SMC.Start(ipAddress, 2012);
            LockStepMGR fPS = new LockStepMGR();//测试
            fPS.Start();
            Core.Room room = new Core.Room();
            //Scene scene = new Scene();
            //RoomMgr roomMgr = new RoomMgr();
            World world = new World();
            Con = new Thread(COC);
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        _NMC.Close();
                        return;
                    case "print":
                        _NMC.Print();
                        break;
                    case "check":
                        Check();
                        break;
                    case "teamlist":
                        TeamMC.A.PrintTeamList();
                        break;
                    case "getserver":
                        SMC.printinfo();
                        break;
                }
            }
        }
        public static void COC()
        {
            for(int i = 0; i < 500; i++)
            {
                TCP newTcp = new TCP();
                newTcp.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newTcp.socket.Connect("192.168.137.1", 2222);
                //ProtocolBytes bytes = new ProtocolBytes();
                //bytes.AddData("SendMSGTo");
                //NewMSG newMSG = new NewMSG();
                //newMSG.MSGState = State.Wrold;
                //newMSG.MSGText = "ceshi"+i.ToString();
                //bytes.AddData(newMSG.SerializableJSON());
                //newTcp.Send(bytes);
                Thread.Sleep(0);
            }
        }
        private static void printfi()
        {
            long J = 22;
            Console.WriteLine(J.GetType());
        }
        private static void Check()
        {
            bool TeamMC_Mark;
            try
            {
                TeamMC_Mark = TeamMC.A.Mark;
            }
            catch (Exception e)
            {
                TeamMC_Mark = false;
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("TeamMC_Mark   " + TeamMC_Mark.ToString());
        }
        public static void Test()
        {
            Thread T1 = new Thread(COC);
            Thread T2 = new Thread(COC);
            T1.Start();
            T2.Start();
        }
    }
}
