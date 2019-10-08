using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankServerTest.Core;

namespace TankServerTest.Logic
{
    partial class HandlePlayerEvent
    {
        public void OnLogin(Player player)
        {
            Scene.instance.AddPlayer(player.id);
        }
        public void OnLogOut(Player player)
        {
            Scene.instance.DelPlayer(player.id);
            if (player.tempData.status == PlayerTempData.Status.Room)
            {
                Room room = player.tempData.room;
                RoomMgr.instance.LeaveRoom(player);
                if (room != null)
                {
                    room.Brodcast(room.GetRoomInfo());
                }
            }
        }
    }
}
