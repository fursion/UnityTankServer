using LockStepServer1._0.Protocol;
using System;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Core;
using LockStepServer1._0.Room.Team;
using Newtonsoft.Json;
using LockStepServer1._0.Room;

namespace LockStepServer1._0.Logic
{
    class HandleConnMsg
    {
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
        //登录    
        public void MsgCheckOpenid(TCP conn, ProtocolBase protocoBase)
        {
            try
            {
                ProtocolBytes bytes = (ProtocolBytes)protocoBase;
                object[] vs = bytes.GetDecode();
                string Openid = vs[1].ToString();
                ProtocolBytes RetBytes = new ProtocolBytes();
                RetBytes.AddData(ProtocolConst.CheckOpenid);
                if (!DataMgr.instance.CheckOpenid(Openid))
                {
                    RetBytes.AddData(ProtocolConst.False);
                    conn.Send(RetBytes);
                    return;
                }
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
                RetBytes.AddData(ProtocolConst.True);
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
                conn.Send(RetBytes);
                Console.WriteLine("登录成功******发送   ");
            }
            catch (Exception e)
            {
                Console.WriteLine("MsgCheckOpenid  " + e.Message);
            }

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
        public void MsgLogin(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes protoco = (ProtocolBytes)protocoBase;
            object[] vs = protoco.GetDecode();
            string protoName = vs[0].ToString();
            string id = vs[1].ToString();
            string pw = vs[2].ToString();
            string strFromat = "[收到登录协议]" + conn.GetAddress();
            Console.WriteLine(strFromat + "用户名" + id + "密码" + pw);

            ProtocolBytes protocoRet = new ProtocolBytes();
            protocoRet.AddData("Login");
            if (!DataMgr.instance.CheckPassWordAndId(id, pw))
            {
                protocoRet.AddData(-1);
                conn.Send(protocoRet);
                Console.WriteLine("登录失败*密码错误");
                return;
            }
            //是否已经登录
            ProtocolBytes protocoLogout = new ProtocolBytes();
            protocoLogout.AddData("Logout");
            if (Player.Kickoff(id, false))
            {
                protocoRet.AddData(-1);
                Console.WriteLine("重复登录");
                conn.Send(protocoRet);
                return;
            }
            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {
                protocoRet.AddData(-1);
                conn.Send(protocoRet);
                Console.WriteLine("没有玩家数据");
                return;
            }
            conn.Player = new Player(id, conn)
            {
                data = playerData
            };
            NMC.instance.handlePlayerEvent.OnLogin(conn.Player);
            protocoRet.AddData(0);
            conn.Send(protocoRet);
            Console.WriteLine("登录成功******发送   " + protocoRet.GetDecode().Length);
            return;
        }
        public void MsgLogout(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes protocoBytes = new ProtocolBytes();
            protocoBytes.AddData("Logout");
            protocoBytes.AddData(0);
            if (conn.Player == null)
            {
                conn.Send(protocoBytes);
                conn.Close();
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
            //friend.GoodList.Add("151353");
            //friend.GoodList.Add("ahcakcakc");
            //friend.BlackList.Add("sgssdgsg");
            //friend.ApplyList.Add("skcnak");
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
            FriendMC.A.GetFriendListInfo(conn.Player);
        }
        public void FindUser(TCP conn, object[] OB)
        {
            FriendMC.A.FindUser(conn.Player, OB);
        }
    }
}
