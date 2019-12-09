 using LockStepServer1._0.Core;
using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.ROOM.Team
{
    class TeamMC
    {
        public bool Mark = false;
        public static TeamMC A;
        public TeamMC()
        {
            A = this;
            Mark = true;
            Console.WriteLine("TeamMC 启动成功");
        }
        private Dictionary<string, TeamBase> TeamList = new Dictionary<string, TeamBase>();
        public void TeamInvitation(Player player,object[] vs)
        {
            string TeamID = vs[1].ToString();
            string Openid = vs[2].ToString();
            if (!TeamList.ContainsKey(TeamID))
                return;
            ProtocolBytes ret = new ProtocolBytes();
            ret.AddData(TeamVar.RetInit);
            foreach (Player Ply in TeamList[TeamID].Players)
            {
                if (Ply != null)
                {
                    if (Openid == Ply.Openid)
                    {
                        ret.AddData(1);
                        player.Send(ret);
                        return;
                    }
                }     
            }
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
                ret.AddData(0);
                player.Send(ret);
            }
        }
        public void CreateTeam(Player player, object[] vs)
        {
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(TeamVar.CreateTeam);
            try
            {
                TeamBase Team = new TeamBase();
                Team.TeamInfo.TeamOpenid = player.Openid;
                Team.TeamInfo.MasterOpenid = player.Openid;
                Team.TeamInfo.playerMax = (int)vs[TeamVar.Team_Max_player];
                Team.Players = new Player[Team.TeamInfo.playerMax];
                Team.AddPlayer(player);
                //Team.players = new List<Player>();
                //Team.players.Add(player);
                TeamList.Add(Team.TeamInfo.TeamOpenid, Team);
                bytes.AddData(0);//返回创建结果
                bytes.AddData(Team.TeamInfo.TeamOpenid);//返回队伍ID
                bytes.AddData(Team.TeamInfo.MasterOpenid);//返回房主ID
                bytes = Team.TeamPlayerInfo(bytes);//返回队伍成员信息
                object[] v = bytes.GetDecode();
                string s = "";
                for (int i = 0; i < v.Length; i++)
                {
                    s += "  " + v[i].ToString();
                }
                Console.WriteLine(s);
                player.TeamOpenid = Team.TeamInfo.TeamOpenid;
                player.Send(bytes);
            }
            catch (Exception e)
            {
                bytes.AddData(1);
                player.Send(bytes);
                Console.WriteLine("CreateTeam  " + e.Message);
            }

        }
        /// <summary>
        /// 加入队伍
        /// </summary>
        /// <param name="TeamOpenid"></param>
        /// <param name="player"></param>
        public void IntoTeam(string TeamOpenid, Player player)
        {
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(TeamVar.OneIntoTeam);
            if (!TeamList.ContainsKey(TeamOpenid))
            {
                bytes.AddData(-1);//房间已经销毁
                player.conn.Send(bytes);
                return;
            }
            if (TeamList[TeamOpenid].EffectivePlayerNumber() >= TeamList[TeamOpenid].TeamInfo.playerMax)
            {
                bytes.AddData(1);//房间人数已满
                player.conn.Send(bytes);
                return;
            }
            foreach(Player Ply in TeamList[TeamOpenid].Players)
            {
                if (Ply != null)
                {
                    if (player.Openid == Ply.Openid)
                    {
                        bytes.AddData(11);
                        player.Send(bytes);
                        return;
                    }
                }
                     
            }
            TeamList[TeamOpenid].AddPlayer(player);
            player.TeamOpenid = TeamList[TeamOpenid].TeamInfo.TeamOpenid;
            bytes.AddData(0);//加入成功
            bytes.AddData(TeamList[TeamOpenid].TeamInfo.TeamOpenid);//返回队伍ID
            bytes.AddData(TeamList[TeamOpenid].TeamInfo.MasterOpenid);//返回房主ID
            bytes = TeamList[TeamOpenid].TeamPlayerInfo(bytes);//返回队伍成员信息
            player.conn.Send(bytes);//发送给加入者
            ProtocolBytes Broid = new ProtocolBytes();
            Broid.AddData(TeamVar.IntoTeam);
            Broid.AddData(0);
            UserData intoer = player.UserData;
            Broid.AddData(JsonConvert.SerializeObject(intoer));
            TeamList[TeamOpenid].BordCast(Broid);//广播给队伍成员
        }
        /// <summary>
        /// 退出队伍
        /// </summary>
        /// <param name="TeamOpenid"></param>
        /// <param name="player"></param>
        /// <param name="TargetOpenid"></param>
        public void ExitTeam(string TeamOpenid, Player player, string TargetOpenid)
        {
            ProtocolBytes Broid = new ProtocolBytes();
            Broid.AddData(TeamVar.ExitTeam);
            try
            {
                if (!TeamList.ContainsKey(TeamOpenid))
                {
                    return;
                }
                if (!TeamList[TeamOpenid].CheckPlayer(TargetOpenid))
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "92");
            }
            if (player.Openid == TargetOpenid && TeamList[TeamOpenid].TeamInfo.MasterOpenid != TargetOpenid)//成员自己退出
            {
                try
                {
                    for (int i = 0; i < TeamList[TeamOpenid].Players.Length; i++)
                    {
                        if (TeamList[TeamOpenid].Players[i].Openid == TargetOpenid)
                        {
                            try
                            {
                                TeamList[TeamOpenid].Players[i] = null;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message + "  104");
                            }
                            ProtocolBytes ExitRet = new ProtocolBytes();
                            ExitRet.AddData(TeamVar.ExitTeam);
                            ExitRet.AddData(0);
                            ExitRet.AddData(TargetOpenid);
                            player.TeamOpenid = null;
                            player.Send(ExitRet);//发送给退出者
                            Console.WriteLine("退出成功");
                            Broid.AddData(0);
                            Broid.AddData(TargetOpenid);
                            TeamList[TeamOpenid].BordCast(Broid);//广播给队伍里剩余人
                            return;
                        }
                    }

                }
                catch (Exception e) { Console.WriteLine(e.Message + "112"); }
            }
            else if (player.Openid == TeamList[TeamOpenid].TeamInfo.MasterOpenid && player.Openid != TargetOpenid)//被踢出
            {
                try
                {
                    Player P;
                    for (int i = 0; i < TeamList[TeamOpenid].Players.Length; i++)
                    {
                        if (TeamList[TeamOpenid].Players[i].Openid == TargetOpenid)
                        {
                            P = TeamList[TeamOpenid].Players[i];
                            TeamList[TeamOpenid].Players[i] = null;
                            ProtocolBytes KickRet = new ProtocolBytes();
                            KickRet.AddData(TeamVar.Kick_Out);
                            KickRet.AddData(1);
                            KickRet.AddData(TargetOpenid);
                            P.TeamOpenid = null;
                            P.Send(KickRet);//发给被踢者
                            Broid.AddData(0);
                            Broid.AddData(TargetOpenid);
                            TeamList[TeamOpenid].BordCast(Broid);//广播给队伍里剩余人
                            return;
                            //return;
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message + "136"); }

            }
            else if (player.Openid != TeamList[TeamOpenid].TeamInfo.MasterOpenid && player.Openid != TargetOpenid)
            {
                try
                {
                    ProtocolBytes bytes = new ProtocolBytes();
                    bytes.AddData(TeamVar.ExitTeam);
                    bytes.AddData(0);
                    player.Send(bytes);//发送给踢人者
                }
                catch (Exception e) { Console.WriteLine(e.Message + "148"); }
            }
            else if (player.Openid == TeamList[TeamOpenid].TeamInfo.MasterOpenid && player.Openid == TargetOpenid)//房主退出
            {
                try
                {
                    for (int i = 0; i < TeamList[TeamOpenid].Players.Length; i++)
                    {
                        if (TeamList[TeamOpenid].Players[i] == null)
                            continue;
                        if (TeamList[TeamOpenid].Players[i].Openid == TargetOpenid)
                        {
                            try
                            {
                                try
                                {
                                    TeamList[TeamOpenid].Players[i] = null;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message + "178");
                                }
                                int count = TeamList[TeamOpenid].EffectivePlayerNumber();
                                Console.WriteLine("count= " + count);
                                if (count > 0)
                                {
                                    try
                                    {
                                        string newTeamOpenid = TeamList[TeamOpenid].RetFastPlayerID();
                                        string newMasterOpenid = newTeamOpenid;
                                        TeamList[TeamOpenid].TeamInfo.MasterOpenid = newMasterOpenid;
                                        TeamList[TeamOpenid].TeamInfo.TeamOpenid = newTeamOpenid;
                                        TeamList.Add(newTeamOpenid, TeamList[TeamOpenid]);
                                        TeamList.Remove(TeamOpenid);
                                        ProtocolBytes ExitRet = new ProtocolBytes();
                                        ExitRet.AddData(TeamVar.ExitTeam);
                                        ExitRet.AddData(0);
                                        ExitRet.AddData(TargetOpenid);
                                        player.TeamOpenid = null;
                                        player.Send(ExitRet);//发送给退出者
                                        Broid.AddData(-2);
                                        Broid.AddData(TargetOpenid);
                                        Broid.AddData(newTeamOpenid);
                                        Broid.AddData(newMasterOpenid);
                                        TeamList[newTeamOpenid].BordCast(Broid);//广播给队伍里剩余人
                                        return;
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message + "200");
                                    }

                                }
                                else
                                {
                                    Broid.AddData(0);
                                    Broid.AddData(TargetOpenid);
                                    player.TeamOpenid = null;
                                    player.Send(Broid);//发送给退出者
                                    try
                                    {
                                        DestoryTeam(TeamOpenid);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("DestoryTeam " + e.Message);
                                    }
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message + "179");
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "192");
                }

            }
        }
        public void DestoryTeam(string TeamOpenid)
        {
            try
            {
                TeamList[TeamOpenid] = null;
                TeamList.Remove(TeamOpenid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "DestoryTeam   203");
            }

        }
        public void PrintTeamList()
        {
            Console.WriteLine("================================TeamList==================================");
            int count = TeamList.Count;
            if (count == 0)
            {
                Console.WriteLine("TeamList.Count  =  " + TeamList.Count);
            }
            for (int i = 0; i < count; i++)
            {
                string t = TeamList.ToList()[i].Key.ToString();
                int cou = TeamList.ToList()[i].Value.TeamInfo.playerMax;
                string s = "";
                Console.WriteLine("TeamID :" + t);
                for (int a = 0; a < cou; a++)
                {
                    if (TeamList.ToList()[i].Value.Players[a] == null)
                        s = "";
                    else
                        s = TeamList.ToList()[i].Value.Players[a].Openid;
                    Console.WriteLine("成员:" + s);
                }
            }
            Console.WriteLine("==========================================================================");
        }
    }
}
