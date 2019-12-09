using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.ROOM
{
    class Room:RoomBase
    {
        public Room()
        {
            Teams = new List<Team.TeamBase>();
            players = new List<Core.Player>();
        }
    }
}
