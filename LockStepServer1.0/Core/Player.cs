using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.Room.Team;

namespace LockStepServer1._0.Core
{
    class Player
    {
        public string Name;
        public string Openid;
        public UserData UserData;
        public string TeamOpenid;
        public Friend friend;
        public PlayerData data;
        public PlayerTempData tempData;
        public TCP conn;
        public Player(string Openid, TCP conn)
        {
            this.Openid = Openid;
            this.conn = conn;
            tempData = new PlayerTempData();
        }
        public void Send(ProtocolBase proto)
        {
            if (conn == null)
                return;
            NMC.instance.Send(conn, proto);
        }
        public static bool Kickoff(String Openid, bool T)
        {
            TCP[] conns = NMC.instance.tcps;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                    continue;
                if (!conns[i].isUse)
                    continue;
                if (conns[i].Player == null)
                    continue;
                if (conns[i].Player.Openid == Openid)//防止重复登录
                {
                    lock (conns[i].Player)
                    {
                        if (T)
                        {
                            ProtocolBytes Logout = new ProtocolBytes();
                            Logout.AddData(ProtocolConst.Logout);
                            conns[i].Player.Send(Logout);
                            return conns[i].Player.Logout();
                        }
                        else
                            return true;
                    }
                }
            }
            return false;
        }

        public bool Logout()
        {
            try
            {
                NMC.instance.CloseTCP(conn);
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            //NMC.instance.CloseTCP(conn);
            //conn.Close();
            //return true;
        }
        public void Close()
        {
            if (TeamOpenid != null)
                TeamMC.A.ExitTeam(TeamOpenid, this, Openid);
        }
    }
}
