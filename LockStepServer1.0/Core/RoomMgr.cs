using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.Core
{
    class RoomMgr
    {
        public static RoomMgr instance;
        public RoomMgr()
        {
            instance = this;
        }
        //房间列表
        public List<Room> list = new List<Room>();
        //创建房间
        public void CreateRoom(Player player)
        {
            Room room = new Room();
            room.RoomID = list.Count;
            lock (list)
            {
                room.AddPlayer(player);
                list.Add(room);
            }
        }
        public void LeaveRoom(Player player)
        {
            PlayerTempData tempData = player.tempData;
            if (tempData.status == PlayerTempData.Status.None)
                return;
            Room room = tempData.room;
            lock (list)
            {
                room.DelPlayer(player.Name);
                if (room.list.Count == 0)
                    list.Remove(room);
            }
        }
        public ProtocolBytes GetRoomList()
        {
            ProtocolBytes protocoBytes = new ProtocolBytes();
            protocoBytes.Addstring("GetRoomList");
            int count = list.Count;
            protocoBytes.AddInt(count);
            for(int i = 0; i < count; i++)
            {
                Room room = list[i];
                protocoBytes.AddInt(room.list.Count);
                protocoBytes.AddInt((int)room.status);
                protocoBytes.AddInt(room.RoomID);
            }
            return protocoBytes;
        }
    }
}
