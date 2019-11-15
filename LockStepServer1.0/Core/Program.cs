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
using LockStepServer1._0.Room.Team;
using LockStepServer1._0.Room;

namespace LockStepServer1._0
{
    class Program
    {
        static void Main(string[] args)
        {
            //printfi();
            string hostName = Dns.GetHostName();   //获取本机名
            IPHostEntry localhost = Dns.GetHostByName(hostName);    //方法已过期，可以获取IPv4的地址
            IPAddress localaddr = localhost.AddressList[0];
            string ipAddress = localaddr.ToString();
            Console.WriteLine(ipAddress);
            UDP uDP = new UDP();
            uDP.Start();
            DataMgr dataMgr = new DataMgr();//连接数据库
            NMC serverNet = new NMC();//网络连接管理
            serverNet.Start(ipAddress, 1234);
            serverNet.Proto = new ProtocolBytes();
            TeamMC teamMC = new TeamMC();//组队管理
            RoomMC roomMC = new RoomMC();//队伍管理
            FriendMC FMC = new FriendMC();//好友管理
            //LockStepMGR fPS = new LockStepMGR();//测试
            //fPS.Start();
            //Scene scene = new Scene();
            //RoomMgr roomMgr = new RoomMgr();
            World world = new World();
           // FMC.test("醉梦");
            //TeamMGR teamMGR = new TeamMGR();
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        serverNet.Close();
                        return;
                    case "print":
                        serverNet.Print();
                        break;
                    case "check":
                        Check();
                        break;
                    case "teamlist":
                        TeamMC.A.PrintTeamList();
                        break;
                }
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
    }
}
