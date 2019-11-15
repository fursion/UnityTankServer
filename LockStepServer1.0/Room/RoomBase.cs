using LockStepServer1._0.Core;
using LockStepServer1._0.Room.Team;
using System.Collections.Generic;
public enum RoomState
{
    ready,
    ongoing
}
public enum RoomType
{
    Combat,//自定义
    ordinary,//普通
}
namespace LockStepServer1._0.Room
{
    class RoomBase
    {
        public struct RoomInfoBase
        {
            public string RoomOpenid;
            public RoomState _RoomState;
            public RoomType _RoomType;
            public int playerMax;
        };
        public int playerMax;
        public int Actual_number;
        public List<TeamBase> Teams;
        public List<Player> players;
        public RoomInfoBase roomInfo;
        public bool CheckPlayer(Player player)
        {
            int count = players.Count;
            if (count == 0)
                return false;
            for(int i = 0; i < count; i++)
            {
                if (player == players[i])
                    return true;
            }
            return false;
        }
    }
}
