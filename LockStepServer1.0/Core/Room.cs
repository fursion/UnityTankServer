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
        public LockStepMGR FPS;
        public Dictionary<string, Player> list = new Dictionary<string, Player>();
        public List<EndPoint> P_UDP_IP = new List<EndPoint>();
        public Dictionary<EndPoint, int> Rep_Send_List = new Dictionary<EndPoint, int>();
        public bool AddPlayer(Player player)
        {
            lock (list)
            {
                if (list.Count > maxPlayer)
                    return false;
                PlayerTempData tempData = player.tempData;
                tempData.room = this;
                tempData.team = SwitchwitchTeam();
                tempData.status = PlayerTempData.Status.Room;
                if (list.Count == 0)
                    tempData.isOwner = true;
                string id = player.Name;
                list.Add(id, player);
            }
            return true;
        }
        public int SwitchwitchTeam()
        {
            int count1 = 0;
            int count2 = 0;
            foreach(Player player in list.Values)
            {
                if (player.tempData.team == 0) count1++;
                if (player.tempData.team == 1) count1++;
            }
            if (count1 <= count2)
                return 0;
            else
                return 1;
        }
        //删除玩家
        public void DelPlayer(string id)
        {
            lock (list)
            {
                if (!list.ContainsKey(id))
                    return;
                bool isOwner = list[id].tempData.isOwner;
                list[id].tempData.status = PlayerTempData.Status.None;
                list.Remove(id);
                if (isOwner)
                    UpdateOwner();
            }
        }
        public void UpdateOwner()
        {
            lock (list)
            {
                if (list.Count <= 0)
                    return;
                foreach (Player player in list.Values)
                    player.tempData.isOwner = false;
                Player p = list.Values.First();
                p.tempData.isOwner = true;
            }
        }
        //广播
        public void Brodcast(ProtocolBase protocoBase)
        {
            foreach(Player player in list.Values)
            {
                player.Send(protocoBase);
            }
        }
        public ProtocolBytes GetRoomInfo()
        {
            ProtocolBytes protocoBytes = new ProtocolBytes();
            protocoBytes.Addstring("GetRoomInfo");
            protocoBytes.AddInt(list.Count);
            foreach(Player player in list.Values)
            {
                protocoBytes.Addstring(player.Name);
                protocoBytes.AddInt(player.tempData.team);
                int isOwner = player.tempData.isOwner ? 1 : 0;
                protocoBytes.AddInt(isOwner);
            }
            return protocoBytes;
        }
        public bool CanStart()
        {
            if (status != Status.Prepare)
                return false;
            int count1 = 1;//
            int count2 = 1;
            foreach(Player player in list.Values)
            {
                if (player.tempData.team == 0) count1++;//
                if (player.tempData.team == 1) count2++;
            }
            if (count1 < 1 || count2 < 1)
                return false;
            return true;
        }
        public void StartFight()
        {
            ProtocolBytes prot = new ProtocolBytes();
            prot.Addstring("Fight");
            status = Status.Fight;
            int teamPos1 = 1;
            int teamPos2 = 1;
            lock (list)
            {
                prot.AddInt(list.Count);
                foreach(Player p in list.Values)
                {
                    p.tempData.Hp = 200;
                    prot.Addstring(p.Name);
                    prot.AddInt(p.tempData.team);
                    if (p.tempData.team == 1)
                        prot.AddInt(teamPos1++);
                    else
                        prot.AddInt(teamPos2++);
                    p.tempData.status = PlayerTempData.Status.Fight;
                }
                Brodcast(prot);
            }
            LockStepMGR fPS= new LockStepMGR();
            FPS = fPS;
           // FPS.room = this;
            FPS.Start();
        }
        public void BrodFPS(ProtocolBase protocoBase)
        {
            ProtocolBytes proto = (ProtocolBytes)protocoBase;
            Console.WriteLine("[Room BrodFPS 发送逻辑帧]");
            for(int i = 0; i < P_UDP_IP.Count; i++)
            {
                if (FPS.Rep_Fps_data.ContainsKey(P_UDP_IP[i]))
                {
                    UDP.instance.SocketSend(FPS.Rep_Fps_data[P_UDP_IP[i]], P_UDP_IP[i]);
                    Console.WriteLine("rep");
                    FPS.Rep_Fps_data.Remove(P_UDP_IP[i]);
                }
                else
                {
                    UDP.instance.SocketSend(proto, P_UDP_IP[i]);
                    Console.WriteLine("test");
                }
            }
        }
    }
}
