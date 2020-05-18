using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.ROOM.Team;
using LockStepServer1._0.LockStep;
using Fursion.Protocol;
using Fursion.ClassLibrary;

public enum PlayerState
{
    Playing,
    None
}
namespace LockStepServer1._0.Core
{
    class GameInfo
    {
        public Room Room { get; set; }
        public SelectModel SelectModel { get; set; } = new SelectModel();
    }
    class Player
    {
        public string Name { get; set; }
        public string Openid { get; set; }
        public string NowDeviceUID { get; set; }//当前登录设备的唯一识别码
        public string ReConnectCheckCode { get; set; }//断线重连校验码
        public PlayerState NowState { get; set; } = PlayerState.None;//玩家状态
        public UserData UserData { get; set; }
        public string TeamOpenid;
        public string RoomID;
        public Room room { get; set; }
        public TeamBase Team { get; set; }
        public Friend friend;
        public bool GameReady { get; set; }
        public PlayerData Data { get; set; }
        public PlayerTempData TempData { get; set; }
        public TCP Conn { get; set; }
        public EndPoint UDPClient { get; set; }
        public Player(string Openid, TCP conn)
        {
            this.Openid = Openid;
            this.Conn = conn;
            TempData = new PlayerTempData();
        }
        public void Send(ProtocolBase proto)
        {
            if (Conn == null)
                return;
            NMC.instance.Send(Conn, proto);
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
                    if (conns[i].Player.NowState == PlayerState.None)
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
                NMC.instance.CloseTCP(Conn);
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
                TeamMC.A.ExitTeam(this);
        }
    }
}
