using LockStepServer1._0.Core;
using LockStepServer1._0.Protocol;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum TeamState
{
    ready,
    ongoing
}
public enum TeamType
{
    Combat,//自定义
    ordinary,//普通
}
namespace LockStepServer1._0.Room.Team
{
    class TeamBase
    {
        public TeamBase()
        {

        }
        public struct TeamInfoBase
        {
            public string TeamOpenid;
            public TeamState _TeamState;
            public TeamType _TeamType;
            public int playerMax;
            public string MasterOpenid;
        };
        //public int playerMax;
        public int Actual_number;
        public Player[] Players;
        public TeamInfoBase TeamInfo;
        public bool CheckPlayer(string TargetOpenid)
        {
            int count = TeamInfo.playerMax;
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
        public int EffectivePlayerNumber()
        {
            try
            {
                int number = 0;
                int count = TeamInfo.playerMax;
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
        public void AddPlayer(Player player)
        {
            for (int i = 0; i < TeamInfo.playerMax; i++)
            {
                if (Players[i] == null)
                {
                    Players[i] = player;
                    return;
                }
            }
        }
        public void BordCast(ProtocolBytes bytes)
        {
            int count = TeamInfo.playerMax;
            if (EffectivePlayerNumber() == 0)
                return;
            for (int i = 0; i < count; i++)
            {
                if (Players[i] != null)
                    Players[i].Send(bytes);
            }
            return;
        }
        public ProtocolBytes TeamPlayerInfo(ProtocolBytes bytes)
        {
            Console.WriteLine("Team :" + TeamInfo.TeamOpenid + " PlayerMAX  " + TeamInfo.playerMax);
            TeamMemberList TML = new TeamMemberList
            {
                MembetList = new UserData[TeamInfo.playerMax]
            };
            for (int i = 0; i < TeamInfo.playerMax; i++)
            {
                if (Players[i]!= null)
                {
                    TML.MembetList[i] = Players[i].UserData;
                    Console.WriteLine(Players[i].Openid);
                }
            }
            string MemberListStr = JsonConvert.SerializeObject(TML);
            bytes.AddData(MemberListStr);
            return bytes;
        }
        public string RetFastPlayerID()
        {
            for (int i = 0; i < TeamInfo.playerMax; i++)
            {
                if (Players[i] != null)
                {
                    return Players[i].Openid;
                }
            }
            return 0.ToString();
        }
    }
}
