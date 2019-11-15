using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.Core
{
    class TeamMGR
    {
        public static TeamMGR instance;
        public TeamMGR()
        {
            instance = this;
            TeamMgrInit();
            Console.WriteLine("TeamMGR 启动");
        }
        public Dictionary<int, Team> TeamDict;
        private void TeamMgrInit()
        {
            TeamDict = new Dictionary<int, Team>();
        }
        public void GreatTeam(Player player)
        {
            Team team = new Team();
            team.Init();
            team.TeamID = player.ID;
            team.players.Add(player);
            TeamDict.Add(team.TeamID, team);
        }
        public void TeamNewMSG(int TeamID,object[] vs)
        {
            if (!TeamDict.ContainsKey(TeamID))
                return;
            TeamDict[TeamID].TeamMSG.Add(vs);

        }
    }
}
