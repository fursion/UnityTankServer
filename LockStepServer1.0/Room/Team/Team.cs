using LockStepServer1._0.Core;
using System;
using Newtonsoft.Json;
using Fursion.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fursion.ClassLibrary;
using Fursion.Tools;

namespace LockStepServer1._0.ROOM.Team
{
    class TeamBase
    {
        public TeamBase()
        {

        }
        private const int MaxNumber = 5;
        public string TeamOpenid { get; set; }
        public TeamState TeamState { get; set; }
        public TeamMode TeamMode { get; set; }
        public TeamType TeamType { get; set; }
        private int m_PresetNumber;
        public int PresetPlayerMax
        {
            get { return m_PresetNumber; }
            set
            {
                m_PresetNumber = Math.Abs(value);
                Players = new Player[m_PresetNumber];
            }
        }
        public string m_MasterOpenid;
        public string MasterOpenid
        {
            get { return m_MasterOpenid; }
            set
            {
                m_MasterOpenid = value;//广播
            }
        }
        public Player[] Players;
        public void StartGameing()
        {
            bool[] readys = TeamReadyInfo();
            if (TeamMode == TeamMode.Custom)
            {
                if (readys.GetBoolArryTureNumber() == m_PresetNumber)
                {
                    Room
                    Console.WriteLine("开始");
                }
                else
                {
                    Console.WriteLine("有玩家没有准备");
                }
            }
            else
            {
                if (readys.GetBoolArryTureNumber() >= EffectivePlayerNumber())
                {
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("有玩家没有准备");
                }
            }
        }
        /// <summary>
        /// 检查玩家是否在队伍中
        /// </summary>
        /// <param name="TargetOpenid">玩家的Openid</param>
        /// <returns></returns>
        public bool CheckPlayerInTeam(string TargetOpenid)
        {
            int count = PresetPlayerMax;
            if (count == 0)
                return false;
            for (int i = 0; i < count; i++)
            {
                if (Players[i] != null)
                {
                    if (TargetOpenid == Players[i].Openid)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 统计房间内的玩家数
        /// </summary>
        /// <returns></returns>
        public int EffectivePlayerNumber()
        {
            try
            {
                int number = 0;
                int count = PresetPlayerMax;
                if (count == 0)
                    return 0;
                for (int i = 0; i < count; i++)
                {
                    if (Players[i] == null)
                        continue;
                    if (Players[i].Openid != null)
                        number++;
                }
                return number;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "EffectivePlayerNumber");
                return 0;
            }

        }
        /// <summary>
        /// 加入队伍
        /// </summary>
        /// <param name="player"></param>
        public void JoinTeam(Player player)
        {
            for (int i = 0; i < PresetPlayerMax; i++)
            {
                if (Players[i] == null)
                {
                    Players[i] = player;
                    {
                        player.Team = this;
                        player.GameReady = true;
                        player.TeamOpenid = this.TeamOpenid;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 队伍内广播
        /// </summary>
        /// <param name="bytes">广播内容</param>
        public void BordCast(ProtocolBytes bytes)
        {
            int count = PresetPlayerMax;
            if (EffectivePlayerNumber() == 0)
                return;
            for (int i = 0; i < count; i++)
            {
                if (Players[i] != null)
                    Players[i].Send(bytes);
            }
            return;
        }
        /// <summary>
        /// 返回队伍中玩家的信息
        /// </summary>
        /// <returns></returns>
        public UserData[] TeamPlayerInfo()
        {
            Console.WriteLine("Team :" + TeamOpenid + " PlayerMAX  " + PresetPlayerMax);
            UserData[] UDs = new UserData[m_PresetNumber];
            for (int i = 0; i < PresetPlayerMax; i++)
            {
                if (Players[i] != null)
                {
                    UDs[i] = Players[i].UserData;
                }
            }
            return UDs;
        }
        /// <summary>
        /// 获取队伍里玩家的准备信息
        /// </summary>
        /// <returns></returns>
        public bool[] TeamReadyInfo()
        {
            bool[] Readys = new bool[m_PresetNumber];
            for (int i = 0; i < PresetPlayerMax; i++)
            {
                if (Players[i] != null)
                {
                    Readys[i] = Players[i].GameReady;
                }
            }
            return Readys;
        }
        /// <summary>
        /// 返回第一个有效玩家的id,没有则返回Null
        /// </summary>
        /// <returns></returns>
        public string RetFastPlayerID()
        {
            for (int i = 0; i < PresetPlayerMax; i++)
            {
                if (Players[i] != null)
                {
                    return Players[i].Openid;
                }
            }
            return null;
        }
        public void Start()
        {
            Core.Room room = new Core.Room();
            room.Init(Players);
        }
        /// <summary>
        /// 向成员更新队伍信息
        /// </summary>
        public void UpdateTeam()
        {
            ProtocolBytes UpdateRe = new ProtocolBytes();
            UpdateRe.SetProtocol(Fursion_Protocol.Team_UpdateTeam);
            UpdateRe.AddData(GetTeamInfo(0));
            BordCast(UpdateRe);
        }
        public TeamReceipt GetTeamInfo(int Ret)
        {
            TeamReceipt Receipt = new TeamReceipt
            {
                ret = Ret,
                TeamOpenid = this.TeamOpenid,
                MasterOpenid = this.MasterOpenid,
                TeamMode = this.TeamMode,
                TeamType = this.TeamType,
                TeamMembers = this.TeamPlayerInfo(),
                ReadyInfo = this.TeamReadyInfo()
            };
            return Receipt;
        }
    }
}
