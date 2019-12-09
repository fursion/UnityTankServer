using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.ROOM.Team;

public enum PlayerState
{
    Playing,
    None
}
namespace LockStepServer1._0.Core
{
    class Player
    {
        public string Name;
        public string Openid;
        public string ReConnectCheckCode;//断线重连校验码
        public PlayerState NowState = PlayerState.None;//玩家状态
        public UserData UserData;
        public string TeamOpenid;//队伍ID
        public string RoomID;//游戏房间ID，场次ID
        public Friend friend;
        public PlayerData data;
        public PlayerTempData tempData;
        public TCP conn;
        public EndPoint UDPClient;
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
                    if(conns[i].Player.NowState ==PlayerState.None)
                        return !conns[i].Player.Logout();
                    lock (conns[i].Player)
                    {
                        if (T)
                        {
                            ProtocolBytes Logout = new ProtocolBytes();
                            Logout.AddData(ProtocolConst.Logout);
                            conns[i].Player.Send(Logout);
                            return !conns[i].Player.Logout();
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
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void Close()
        {
            if (TeamOpenid != null)
                TeamMC.A.ExitTeam(TeamOpenid, this, Openid);
        }
    }
}
