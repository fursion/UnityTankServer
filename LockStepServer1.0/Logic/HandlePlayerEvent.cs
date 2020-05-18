using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.Core;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.ROOM;
using LockStepServer1._0.ROOM.Team;
using Newtonsoft.Json;
using Fursion.Protocol;

namespace LockStepServer1._0.Logic
{
    class HandlePlayerEventMethodPool
    {
        public void TeamInvitation(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            TeamMC.A.TeamInvitation(conn.Player, vs);
        }
        public void TeamStart(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamID = vs[1].ToString();
            if (conn.Player.TeamOpenid == TeamID)
            {
                TeamMC.A.TeamDict[TeamID].StartGameing();
            }
        }
        public void IntoTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamOpenid = vs[1].ToString();
            TeamMC.A.IntoTeam(TeamOpenid, conn.Player);
        }
        public void OnLogin(Player player)
        {

        }
        public void OnLogOut(Player player)
        {
            //Scene.instance.DelPlayer(player.Name);
            //if (player.tempData.status == PlayerTempData.Status.Room)
            //{
            //    Room room = player.tempData.room;
            //    RoomMgr.instance.LeaveRoom(player);
            //    if (room != null)
            //    {
            //        room.Brodcast(room.GetRoomInfo());
            //    }
            //}
        }
        public void AddFriends(Player player,string TargetFriendName,int TargetFriendID)
        {
            if (TargetFriendName == "")
                return;
            if (player.Data.Contacts.ContainsKey(TargetFriendName))
                return;
            player.Data.Contacts.Add(TargetFriendName, TargetFriendID);
            
        }
        public void DelFriends(Player player, string TargetFriendName)
        {

        }
    }
}
