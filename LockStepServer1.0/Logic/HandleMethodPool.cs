using System;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Core;
using LockStepServer1._0.ROOM.Team;
using Newtonsoft.Json;
using Fursion.Protocol;
using LockStepServer1._0.ROOM;
using System.Threading;
using System.Web.Security;
using Fursion.ClassLibrary;

namespace LockStepServer1._0.Logic
{
    class HandleConnMethodPool
    {
        public void Reconnect(TCP conn, ProtocolBase proto)
        {
            try
            {
                ProtocolBytes bytes = (ProtocolBytes)proto;
                object[] vs = bytes.GetDecode();
                LoginReceipt receipt = JsonConvert.DeserializeObject<LoginReceipt>(bytes.GetDecode()[1].ToString());
                for (int i = 0; i < NMC.instance.tcps.Length; i++)
                {
                    if (NMC.instance.tcps[i] == null)
                        continue;
                    if (NMC.instance.tcps[i].Player == null)
                        continue;
                    if (NMC.instance.tcps[i].Player.Openid == receipt.UserOpenid)
                    {
                        if (NMC.instance.tcps[i].Player.NowDeviceUID != receipt.DeviceUID)
                        {
                            Console.WriteLine("ReConnection Check failed");
                            NMC.instance.CloseTCP(conn);
                            return;
                        }
                        conn.Player = NMC.instance.tcps[i].Player;
                        conn.Player.Conn = conn;
                        NMC.instance.tcps[i] = null;
                        ProtocolBytes Ret = new ProtocolBytes();
                        Ret.SetProtocol(Fursion_Protocol.ReConnectRet);
                        conn.Send(Ret);
                        return;
                    }
                    else
                        NMC.instance.CloseTCP(conn);
                }
                Console.WriteLine(" This User Not Find ");
                Thread ChechOpenidT = new Thread(new ThreadStart(delegate { DataMgr.instance.CheckOpenid(conn, receipt,true); }))
                {
                    Name = receipt.UserOpenid
                };
                ChechOpenidT.Start();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " ReConnect");
            }


        }
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="proto"></param>
        public void HearBeat(TCP conn, ProtocolBase proto)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            //Console.WriteLine("[更新心跳时间]" + conn.GetAddress());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protocoBase"></param>
        public void Register(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes protoco = (ProtocolBytes)protocoBase;
            object[] vs = protoco.GetDecode();
            string protoName = vs[0].ToString();
            string strFromat = "[收到注册协议]" + conn.GetAddress();
            protoco = new ProtocolBytes();
            protoco.SetProtocol(Fursion_Protocol.Register);
            UserData UD = JsonConvert.DeserializeObject<UserData>(vs[1].ToString());
            string Openid = UD.Openid;
            string NickName = UD.NickNeam;
            Console.WriteLine(strFromat + "   " + Openid + "   " + NickName);
            var reg = DataMgr.instance.Register(Openid, NickName, UD);
            if (reg)//
            {
                protoco.AddData(0);//
            }
            else
            {
                protoco.AddData(1);
                conn.Send(protoco);
                return;
            }
            conn.Send(protoco);
            Console.WriteLine("**&&**&&**");
        }
        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="bytes"></param>
        public void MSG(ProtocolBytes bytes)
        {
            Console.WriteLine(bytes.GetDecode()[1]);
            switch (bytes.GetDecode()[1])
            {
                case "TeamMSG": break;
                case "WorldMSG": World.instance.NewMSG(bytes); break;
            }
        }
        /// <summary>
        /// 登录校验
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protocoBase"></param>
        public void CheckOpenid(TCP conn, ProtocolBase protocoBase)//登录校验
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            LoginReceipt LReceipt = JsonConvert.DeserializeObject<LoginReceipt>(bytes.GetDecode()[1].ToString());
            Thread ChechOpenidT = new Thread(new ThreadStart(delegate { DataMgr.instance.CheckOpenid(conn, LReceipt,false); }))
            {
                Name = LReceipt.UserOpenid
            };
            ChechOpenidT.Start();
        }
        public static void TrueCheckOpenid(TCP conn, LoginReceipt receipt,bool ISReConn)
        {
            ProtocolBytes RetBytes = new ProtocolBytes();
            RetBytes.SetProtocol(Fursion_Protocol.CheckOpenid);
            if (receipt.OnlineRec)
            {
                if (Player.Kickoff(receipt.UserOpenid, false))
                {
                    RetBytes.AddData(2);
                    conn.Send(RetBytes);
                    return;
                }
            }
            else
            {
                if (Player.Kickoff(receipt.UserOpenid, true))
                {
                    RetBytes.AddData(2);
                    conn.Send(RetBytes);
                    return;
                }
            }
            PlayerData playerData = DataMgr.instance.GetPlayerData(receipt.UserOpenid);
            if (playerData == null)
            {
                RetBytes.AddData(-1);
                conn.Send(RetBytes);
                return;
            }
            conn.Player = new Player(receipt.UserOpenid, conn)
            {
                Data = playerData
            };
            conn.Player.NowDeviceUID = receipt.DeviceUID;
            RetBytes.AddData(0);//Success
            conn.Player.UserData = DataMgr.instance.GetUserData(conn.Player.Openid);
            Friend friend = FriendMC.A.InitFriendListInfo(conn.Player);
            if (friend.GoodList.Keys.Count != 0)
            {
                ProtocolBytes onlineRet = new ProtocolBytes();
                onlineRet.SetProtocol(Fursion_Protocol.Friend_OnlineNotice);
                onlineRet.AddData(conn.Player.Openid);
                foreach (string id in friend.GoodList.Keys)
                {
                    if (FriendMC.OnlinePlayerList.ContainsKey(id))
                        FriendMC.OnlinePlayerList[id].Send(onlineRet);
                }
            }
            string FriendListStr = JsonConvert.SerializeObject(friend);
            RetBytes.AddData(FriendListStr);
            ServerMC.This.GetServerInfo(RetBytes, conn);
            if (ISReConn)
            {
                ProtocolBytes Ret = new ProtocolBytes();
                Ret.SetProtocol(Fursion_Protocol.ReConnectRet);
                conn.Send(Ret);
            }
            Console.WriteLine("登录成功******发送   ");
        }
        public static void FalseCheckOpenid(TCP conn)
        {
            ProtocolBytes RetBytes = new ProtocolBytes();
            RetBytes.SetProtocol(Fursion_Protocol.CheckOpenid);
            RetBytes.AddData(1);
            conn.Send(RetBytes);
        }

        public void Logout(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes protocoBytes = new ProtocolBytes();
            protocoBytes.SetProtocol(Fursion_Protocol.Logout);
            protocoBytes.AddData(0);
            if (conn.Player == null)
            {
                conn.Send(protocoBytes);
                NMC.instance.CloseTCP(conn);
            }
            else
            {
                conn.Send(protocoBytes);
                conn.Player.Logout();
            }
        }

        public void JSONTEST(TCP conn, ProtocolBase protocoBase)
        {
            Friend friend = new Friend();
            string jsonstr = JsonConvert.SerializeObject(friend);
            Console.WriteLine("JSON测试    " + jsonstr);
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData("JSONTEST");
            bytes.AddData(jsonstr);
            conn.Send(bytes);
        }
    }
    class HandFriendEventMethodPool
    {
        public void AddFriend(TCP conn, object[] OB)
        {
            FriendMC.A.AddFriend(conn.Player, OB);
        }
        public void DelFriend(TCP conn, object[] OB)
        {
            FriendMC.A.DelFriend(conn.Player, OB);
        }
        public void AddBlackList(TCP conn, object[] OB)
        {
            FriendMC.A.AddBlackList(conn.Player, OB);
        }
        public void DelBlackList(TCP conn, object[] OB)
        {
            FriendMC.A.DelBlackList(conn.Player, OB);
        }
        public void AddApply(TCP conn, object[] OB)
        {
            FriendMC.A.ApplyAddFriend(conn.Player, OB);
        }
        public void DelApply(TCP conn, object[] OB)
        {
            FriendMC.A.delApply(conn.Player, OB);
        }
        public void GetFriendListInfo(TCP conn, object[] OB)
        {
            Thread GetfriendT = new Thread(new ThreadStart(delegate { FriendMC.A.GetFriendListInfo(conn.Player); }));
            GetfriendT.Start();
        }
        public void FindUser(TCP conn, object[] OB)
        {
            FriendMC.A.FindUser(conn.Player, OB);
        }
    }
    class HandTeamEventMethodPool
    {
        public void TeamInvitation(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            TeamMC.A.TeamInvitation(conn.Player, vs);
        }
        public void IntoTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamOpenid = vs[1].ToString();
            TeamMC.A.IntoTeam(TeamOpenid, conn.Player);
        }
        public void ExitTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            try
            {

                TeamMC.A.ExitTeam(conn.Player);

            }
            catch (Exception e)
            {
                Console.WriteLine("HandleConnMsg     MsgExitTeam  " + e.Message);
            }
        }
        public void Ready(TCP conn, ProtocolBase protocoBase)
        {
            conn.Player.GameReady = true;
            if (conn.Player.Team != null)
                conn.Player.Team.UpdateTeam();
        }
        public void DisReady(TCP conn, ProtocolBase protocoBase)
        {
            conn.Player.GameReady = false;
            if (conn.Player.Team != null)
                conn.Player.Team.UpdateTeam();
        }
        public void CreateTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            try
            {
                TeamMC.A.CreateTeam(conn.Player, vs);
            }
            catch (Exception e)
            {
                Console.WriteLine("MsgCreateTeam   207" + e.Message);
            }

        }
        public void TeamStart(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamID = vs[1].ToString();
            if (conn.Player.TeamOpenid == TeamID)
            {
                TeamMC.A.TeamDict[TeamID].Start();
            }
        }
    }
}
