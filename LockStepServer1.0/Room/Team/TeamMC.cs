using LockStepServer1._0.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Fursion.Protocol;
using System.Text;
using System.Threading.Tasks;
using Fursion.Tools;
using Fursion.ClassLibrary;

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
        public int TeamsMax = 1000;
        public Dictionary<string, TeamBase> TeamDict = new Dictionary<string, TeamBase>();
        public Dictionary<string, TeamBase> OnePlayerTeamDict = new Dictionary<string, TeamBase>();
        public Dictionary<string, TeamBase> TwoPlayerTeamDict = new Dictionary<string, TeamBase>();
        public Dictionary<string, TeamBase> ThreePlayerTeamDict = new Dictionary<string, TeamBase>();
        public Dictionary<string, TeamBase> FivePlayerTeamDict = new Dictionary<string, TeamBase>();
        public void CreateTeam()
        {

        }
        /// <summary>
        /// 组队邀请
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vs"></param>
        public void TeamInvitation(Player player, object[] vs)
        {
            InviationReceipt receipt = JsonConvert.DeserializeObject<InviationReceipt>(vs[1].ToString());
            if (!TeamDict.ContainsKey(receipt.TeamOpenid))
                return;
            ProtocolBytes ret = new ProtocolBytes();
            ret.SetProtocol(Fursion_Protocol.Team_RetInit);
            foreach (Player Ply in TeamDict[receipt.TeamOpenid].Players)
            {
                if (Ply != null)
                {
                    if (receipt.ReceiverId == Ply.Openid)
                    {
                        ret.AddData(1);
                        player.Send(ret);
                        return;
                    }
                }
            }
            if (FriendMC.OnlinePlayerList.ContainsKey(receipt.ReceiverId))
            {
                ProtocolBytes Inret = new ProtocolBytes();
                Inret.SetProtocol(Fursion_Protocol.Team_TeamInvitation);
                receipt.InviterData = player.UserData;
                Inret.AddData(receipt);
                FriendMC.OnlinePlayerList[receipt.ReceiverId].Send(Inret);
                Console.WriteLine("Send end");
            }
        }
        /// <summary>
        /// 创建队伍
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vs"></param>
        public void CreateTeam(Player player, object[] vs)
        {
            if (player.TeamOpenid != null)
                return;
            CreatTeamApply apply = JsonConvert.DeserializeObject<CreatTeamApply>(vs[1].ToString());
            ProtocolBytes CreatRecp = new ProtocolBytes();
            CreatRecp.SetProtocol(Fursion_Protocol.Team_CreateTeam);
            try
            {
                TeamBase Team = new TeamBase
                {
                    TeamOpenid = FursionTools.GetFursionGUID(),//给队伍添加唯一识别符
                    TeamType = apply.TeamType,
                    TeamMode = apply.TeamMode,
                    MasterOpenid = player.Openid
                };
                switch (Team.TeamType)
                {
                    case TeamType.One: Team.PresetPlayerMax = 1; break;
                    case TeamType.Three: Team.PresetPlayerMax = 3; break;
                    case TeamType.Five: Team.PresetPlayerMax = 5; break;
                }
                switch (Team.TeamMode)
                {
                    case TeamMode.Custom: Team.PresetPlayerMax *= 2; break;
                }
                Team.Players = new Player[Team.PresetPlayerMax];
                Team.JoinTeam(player);//将创建者加入队伍
                player.TeamOpenid = Team.TeamOpenid;
                player.GameReady = true;
                TeamDict.Add(Team.TeamOpenid, Team);//将队伍加入全服队伍列表
                //switch (Team.TeamInfo.PresetPlayerMax)
                //{
                //    case 1: OnePlayerTeamDict.Add(Team.TeamInfo.TeamOpenid, Team); break;
                //    case 2: TwoPlayerTeamDict.Add(Team.TeamInfo.TeamOpenid, Team); break;
                //    case 3: ThreePlayerTeamDict.Add(Team.TeamInfo.TeamOpenid, Team); break;
                //    case 5: FivePlayerTeamDict.Add(Team.TeamInfo.TeamOpenid, Team); break;
                //    default: break;
                //}
                CreatRecp.AddData(Team.GetTeamInfo(0));
                player.Send(CreatRecp);
            }
            catch (Exception e)
            {
                CreatRecp.AddData(1);
                player.Send(CreatRecp);
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
            ProtocolBytes RecpToJoin = new ProtocolBytes();
            RecpToJoin.SetProtocol(Fursion_Protocol.Team_IntoTeam);
            if (!TeamDict.ContainsKey(TeamOpenid))
            {
                RecpToJoin.AddData(TeamDict[TeamOpenid].GetTeamInfo(-1));
                player.Conn.Send(RecpToJoin);
                return;
            }
            else if (TeamDict[TeamOpenid].EffectivePlayerNumber() == TeamDict[TeamOpenid].PresetPlayerMax)
            {
                RecpToJoin.AddData(TeamDict[TeamOpenid].GetTeamInfo(1));
                player.Conn.Send(RecpToJoin);
                return;
            }
            else if (TeamDict[TeamOpenid].CheckPlayerInTeam(player.Openid))
            {
                RecpToJoin.AddData(TeamDict[TeamOpenid].GetTeamInfo(11));
                player.Send(RecpToJoin);
                return;
            }
            TeamDict[TeamOpenid].JoinTeam(player);
            player.TeamOpenid = TeamDict[TeamOpenid].TeamOpenid;
            RecpToJoin.AddData(TeamDict[TeamOpenid].GetTeamInfo(0));
            player.Send(RecpToJoin);//发送给加入者
            ProtocolBytes Broid = new ProtocolBytes();
            Broid.SetProtocol(Fursion_Protocol.Team_IntoTeam);
            Broid.AddData(TeamDict[TeamOpenid].GetTeamInfo(0));
            TeamDict[TeamOpenid].BordCast(Broid);//广播给队伍成员
        }
        /// <summary>
        /// 退出队伍
        /// </summary>
        /// <param name="TeamOpenid"></param>
        /// <param name="player"></param>
        /// <param name="TargetOpenid"></param>
        public void ExitTeam(Player player)
        {

            if (player.TeamOpenid != null)
            {
                if (!TeamDict.ContainsKey(player.TeamOpenid))
                {
                    ProtocolBytes ExitRet = new ProtocolBytes();
                    ExitRet.SetProtocol(Fursion_Protocol.Team_SelfExit);
                    TeamReceipt Receipt = new TeamReceipt
                    {
                        ret = 0
                    };
                    ExitRet.AddData(Receipt);
                    player.Send(ExitRet);//玩家不在队伍中，告知玩家不在队伍中
                    return;
                }
                string TeamOpenid = player.TeamOpenid;
                for (int i = 0; i < TeamDict[TeamOpenid].Players.Length; i++)
                {
                    if (TeamDict[TeamOpenid].Players[i] == player)
                    {
                        ProtocolBytes ExitRet = new ProtocolBytes();
                        ExitRet.SetProtocol(Fursion_Protocol.Team_SelfExit);
                        TeamDict[TeamOpenid].Players[i] = null;
                        if (player.Openid == TeamDict[TeamOpenid].MasterOpenid)
                        {
                            player.TeamOpenid = null;
                            ExitRet.AddData(TeamDict[TeamOpenid].GetTeamInfo(0));
                            player.Send(ExitRet);
                            if (TeamDict[TeamOpenid].EffectivePlayerNumber() == 0)//
                            {
                                DestoryTeam(TeamOpenid);
                                return;
                            }
                            else
                                TeamDict[TeamOpenid].MasterOpenid = TeamDict[TeamOpenid].RetFastPlayerID();
                        }
                        else
                        {
                            player.TeamOpenid = null;
                            ExitRet.AddData(TeamDict[TeamOpenid].GetTeamInfo(0));
                            player.Send(ExitRet);
                        }
                        ProtocolBytes BordExitRet = new ProtocolBytes();
                        BordExitRet.SetProtocol(Fursion_Protocol.Team_ExitTeam);
                        BordExitRet.AddData(TeamDict[TeamOpenid].GetTeamInfo(0));
                        if (TeamDict.ContainsKey(TeamOpenid))
                            TeamDict[TeamOpenid].BordCast(BordExitRet);//广播给队伍里剩余人
                        return;
                    }
                }
            }
            else
            {
                ProtocolBytes ExitRet = new ProtocolBytes();
                ExitRet.SetProtocol(Fursion_Protocol.Team_SelfExit);
                TeamReceipt Receipt = new TeamReceipt
                {
                    ret = 0
                };
                ExitRet.AddData(Receipt);
                player.Send(ExitRet);//玩家不在队伍中，告知玩家不在队伍中
            }
        }
        /// <summary>
        /// 销毁队伍
        /// </summary>
        /// <param name="TeamOpenid">队伍id</param>
        public void DestoryTeam(string TeamOpenid)
        {
            try
            {
                TeamDict[TeamOpenid] = null;
                TeamDict.Remove(TeamOpenid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "DestoryTeam   203");
            }

        }
        public void PrintTeamList()
        {
            Console.WriteLine("================================TeamList==================================");
            int count = TeamDict.Count;
            if (count == 0)
            {
                Console.WriteLine("TeamList.Count  =  " + TeamDict.Count);
            }
            for (int i = 0; i < count; i++)
            {
                int cou = TeamDict.ToList()[i].Value.PresetPlayerMax;
                string s = "";
                Console.WriteLine("TeamID :" + TeamDict.ToList()[i].Key + TeamDict.ToList()[i].Value.MasterOpenid);
                for (int a = 0; a < cou; a++)
                {
                    if (TeamDict.ToList()[i].Value.Players[a] == null)
                        s = "";
                    else
                        s = TeamDict.ToList()[i].Value.Players[a].Openid;
                    Console.WriteLine("成员:" + s);
                }
            }
            Console.WriteLine("==========================================================================");
        }
    }
}
