using LockStepServer1._0.Protocol;
using System;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Core;
using LockStepServer1._0.ROOM.Team;
using Newtonsoft.Json;
using LockStepServer1._0.ROOM;
using System.Threading;
using System.Web.Security;

namespace LockStepServer1._0.Logic
{
    class HandleConnMsg
    {
        public void MsgReConnect(TCP conn, ProtocolBase proto)
        {
            try
            {
                ProtocolBytes bytes = (ProtocolBytes)proto;
                object[] vs = bytes.GetDecode();
                string Openid = vs[1].ToString();
                string ReConnectCheckCode = vs[2].ToString();
                for (int i = 0; i < NMC.instance.tcps.Length; i++)
                {
                    if (NMC.instance.tcps[i] == null)
                        continue;
                    if (NMC.instance.tcps[i].Player == null)
                        continue;
                    if (NMC.instance.tcps[i].Player.Openid == Openid)
                    {
                        if (NMC.instance.tcps[i].Player.ReConnectCheckCode == null || NMC.instance.tcps[i].Player.ReConnectCheckCode != ReConnectCheckCode)
                        {
                            Console.WriteLine("ReConnection Check failed");
                            NMC.instance.CloseTCP(conn);
                            return;
                        }
                        TCP NewTCP = NMC.instance.tcps[i];
                        conn.Player = NewTCP.Player;
                        conn.Player.conn = conn;
                        NMC.instance.tcps[i] = null;
                        conn.Player.ReConnectCheckCode = Membership.GeneratePassword(20, 0);
                        ProtocolBytes bytes1 = new ProtocolBytes();
                        bytes1.AddData("ReConnectRet");
                        bytes1.AddData(1);
                        bytes1.AddData(conn.Player.ReConnectCheckCode);
                        conn.Send(bytes1);
                        return;
                    }
                }
                Console.WriteLine(" This User Not Find ");
                NMC.instance.CloseTCP(conn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " ReConnect");
            }


        }
        public void MsgFindUser(string NickName)
        {

        }
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="proto"></param>
        public void MsgHearBeat(TCP conn, ProtocolBase proto)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            //Console.WriteLine("[更新心跳时间]" + conn.GetAddress());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protocoBase"></param>
        public void MsgRegister(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes protoco = (ProtocolBytes)protocoBase;
            object[] vs = protoco.GetDecode();
            string protoName = vs[0].ToString();
            string strFromat = "[收到注册协议]" + conn.GetAddress();
            protoco = new ProtocolBytes();
            protoco.AddData("Register");
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
        public void MsgMSG(ProtocolBytes bytes)
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
        public void MsgCheckOpenid(TCP conn, ProtocolBase protocoBase)//登录校验
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string Openid = vs[1].ToString();
            ProtocolBytes RetBytes = new ProtocolBytes();
            RetBytes.AddData(ProtocolConst.CheckOpenid);
            Thread ChechOpenidT = new Thread(new ThreadStart(delegate { DataMgr.instance.CheckOpenid(conn, vs); }));
            ChechOpenidT.Name = Openid;
            ChechOpenidT.Start();
        }
        public static void TrueCheckOpenid(TCP conn, object[] vs)
        {
            string Openid = vs[1].ToString();
            ProtocolBytes RetBytes = new ProtocolBytes();
            RetBytes.AddData(ProtocolConst.CheckOpenid);
            if ((int)vs[2] == ProtocolConst.False)
            {
                if (Player.Kickoff(Openid, false))
                {
                    RetBytes.AddData(2);
                    conn.Send(RetBytes);
                    return;
                }
            }
            else
            {
                if (Player.Kickoff(Openid, true))
                {
                    RetBytes.AddData(2);
                    conn.Send(RetBytes);
                    return;
                }
            }
            PlayerData playerData = DataMgr.instance.GetPlayerData(Openid);
            if (playerData == null)
            {
                RetBytes.AddData(-1);
                conn.Send(RetBytes);
                return;
            }
            conn.Player = new Player(Openid, conn)
            {
                data = playerData
            };
            conn.Player.ReConnectCheckCode = Membership.GeneratePassword(20, 0);
            RetBytes.AddData(ProtocolConst.True);//Success
            conn.Player.UserData = DataMgr.instance.GetUserData(conn.Player.Openid);
            Friend friend = FriendMC.A.InitFriendListInfo(conn.Player);
            if (friend.GoodList.Keys.Count != 0)
            {
                ProtocolBytes onlineRet = new ProtocolBytes();
                onlineRet.AddData(FriendVar.OnlineNotice);
                onlineRet.AddData(conn.Player.Openid);
                foreach (string id in friend.GoodList.Keys)
                {
                    if (FriendMC.A.OnlinePlayerList.ContainsKey(id))
                        FriendMC.A.OnlinePlayerList[id].Send(onlineRet);
                }
            }
            string FriendListStr = JsonConvert.SerializeObject(friend);
            RetBytes.AddData(FriendListStr);
            ServerMC.This.GetServerInfo(RetBytes, conn);
            Console.WriteLine("登录成功******发送   ");
        }
        public static void FalseCheckOpenid(TCP conn)
        {
            ProtocolBytes RetBytes = new ProtocolBytes();
            RetBytes.AddData(ProtocolConst.CheckOpenid);
            RetBytes.AddData(ProtocolConst.False);
            conn.Send(RetBytes);
        }
        public void MsgTeamInvitation(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamID = vs[1].ToString();
            string Openid = vs[2].ToString();
            if (FriendMC.A.OnlinePlayerList.ContainsKey(Openid))
            {
                ProtocolBytes Inret = new ProtocolBytes();
                Inret.AddData(TeamVar.Team_Invitation);
                Inret.AddData("3V3");
                Inret.AddData(TeamID);

                UserData UD = new UserData();
                UD = DataMgr.instance.GetUserData(Openid);
                string UDStr = JsonConvert.SerializeObject(UD);
                Inret.AddData(UDStr);
                FriendMC.A.OnlinePlayerList[Openid].Send(Inret);
            }
        }
        public void MsgLogout(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes protocoBytes = new ProtocolBytes();
            protocoBytes.AddData("Logout");
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
        public void MsgIntoTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamOpenid = vs[1].ToString();
            TeamMC.A.IntoTeam(TeamOpenid, conn.Player);
        }
        public void MsgExitTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamOpenid = vs[1].ToString();
            string TargetOpenid = vs[2].ToString();
            try
            {

                TeamMC.A.ExitTeam(TeamOpenid, conn.Player, TargetOpenid);

            }
            catch (Exception e)
            {
                Console.WriteLine("HandleConnMsg     MsgExitTeam  " + e.Message);
            }

        }
        public void MsgCreateTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamOpenid = vs[1].ToString();
            try
            {
                TeamMC.A.CreateTeam(conn.Player, vs);
            }
            catch (Exception e)
            {
                Console.WriteLine("MsgCreateTeam   207" + e.Message);
            }

        }
        public void MsgJSONTEST(TCP conn, ProtocolBase protocoBase)
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
    class HandFriendEvent
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
}
