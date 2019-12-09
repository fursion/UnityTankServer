using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using LockStepServer1._0.LockStep;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.NetWorking;
using System.Timers;

namespace LockStepServer1._0.Core
{
    class Room
    {
        public enum Status
        {
            Prepare=1,
            Fight=2,
        }
        public Status status = Status.Prepare;
        public int RoomID = 0;
        public int maxPlayer = 10;
        public LockStepMGR LSM;
        public Dictionary<string, Player> list = new Dictionary<string, Player>();
        public List<Player> PlayerList = new List<Player>();
        public List<EndPoint> P_UDP_IP = new List<EndPoint>();
        public Dictionary<EndPoint, int> Rep_Send_List = new Dictionary<EndPoint, int>();
        public void Init(Player[] players)
        {
            for(int i = 0; i < players.Length; i++)
            {
                PlayerList.Add(players[i]);
            }
            Ready();
        }
        public void Ready()
        {
            Timer timer = new Timer(60000);
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(ReadStart);
        }
        public void ReadStart(object o, ElapsedEventArgs e)
        {
            Console.WriteLine("测试");
        }
        public void Start()
        {

        }
        public void End()
        {

        }
    }
}
