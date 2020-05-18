using LockStepServer1._0.Core;
using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankServerTest.Logic
{   partial class HandlePlayerMsg
    {
        //获取房间列表
        public void GetRoomList(Player player,ProtocolBase protocoBase)
        {

        }
        //获取房间信息
        public void GetRoomInfo(Player player,ProtocolBase protocoBase)
        {
            if (player.TempData.status != PlayerTempData.Status.Room)
            {
                Console.WriteLine("MsgGetRoomInfo status err "+player.Name);
                return;
            }
            Room room = player.TempData.Room;
        }
    }
    class HandRoomMsg
    {

    }
}
