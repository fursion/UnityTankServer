using LockStepServer1._0.Core;
using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.Room
{
    class RoomMC
    {
        private Dictionary<string, Room> RoomList = new Dictionary<string, Room>();
        public void CreateRoom(Player player, object[] vs)
        {
            Room room = new Room();
            room.roomInfo.RoomOpenid = player.Openid;
            room.roomInfo._RoomState = RoomState.ready;
            room.roomInfo.playerMax = (int)vs[1];
        }
        public void IntoRoom(string RoomOpenid, Player player)
        {
            ProtocolBytes bytes = new ProtocolBytes();
            if (!RoomList.ContainsKey(RoomOpenid))
            {
                bytes.AddData(-1);//房间已经销毁
                player.conn.Send(bytes);
                return;
            }
            if (RoomList[RoomOpenid].players.Count >= RoomList[RoomOpenid].playerMax)
            {
                bytes.AddData(1);//房间人数已满
                player.conn.Send(bytes);
                return;
            }
            RoomList[RoomOpenid].players.Add(player);
            bytes.AddData(0);//加入成功
            player.conn.Send(bytes);
        }
    }
}
