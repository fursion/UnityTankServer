using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.NetWorking;

namespace LockStepServer1._0.Core
{
    class Team
    {
        public List<Player> players;
        public List<object[]> TeamMSG;
        public int TeamID;
        public void Init()
        {
            players = new List<Player>();
            TeamMSG = new List<object[]>();
        }
        public void MSGBroadcast(object[] vs)
        {
            for(int i = 0; i < players.Count; i++)
            {
                //NMC.instance.Send
            }
        }
    }
}
