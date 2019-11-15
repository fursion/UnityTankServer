using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.Core;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.Room;
using LockStepServer1._0.Room.Team;
using Newtonsoft.Json;

namespace LockStepServer1._0.Logic
{
    class HandlePlayerEvent
    {
        public void MsgTeamInvitation(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            TeamMC.A.TeamInvitation(conn.Player, vs);
        }
        public void MsgIntoTeam(TCP conn, ProtocolBase protocoBase)
        {
            ProtocolBytes bytes = (ProtocolBytes)protocoBase;
            object[] vs = bytes.GetDecode();
            string TeamOpenid = vs[1].ToString();
            TeamMC.A.IntoTeam(TeamOpenid, conn.Player);
        }
        public void OnLogin(Player player)
        {
            Scene.instance.AddPlayer(player.Name);
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
            if (player.data.Contacts.ContainsKey(TargetFriendName))
                return;
            player.data.Contacts.Add(TargetFriendName, TargetFriendID);
            
        }
        public void DelFriends(Player player, string TargetFriendName)
        {

        }
    }
}
