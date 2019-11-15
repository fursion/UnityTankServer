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
        public void MsgGetRoomList(Player player,ProtocolBase protocoBase)
        {
            player.Send(RoomMgr.instance.GetRoomList());
            Console.WriteLine("MsgGetRoomList flash "+player.Name);
        }
        //创建房间
        public void MsgCreatRoom(Player player,ProtocolBase protocoBase)
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.Addstring("CreatRoom");
            if (player.tempData.status != PlayerTempData.Status.None)
            {
                Console.WriteLine("MsgCreatRoom Fail"+player.Name);
                proto.AddInt(-1);
                player.Send(proto);
                return;
            }
            RoomMgr.instance.CreateRoom(player);
            proto.AddInt(0);
            proto.AddInt(player.tempData.room.RoomID);
            player.Send(proto);
            Console.WriteLine("MsgCreatRoom OK "+player.Name);
        }
        //加入房间
        public void MsgEnterRoom(Player player,ProtocolBase protocoBase)
        {
            int start = 0;
            ProtocolBytes proto = (ProtocolBytes)protocoBase;
            string protoname = proto.GetString(start, ref start);
            int index = proto.GetInt(start, ref start);
            Console.WriteLine("[收到]MsgEnterRoom "+player.Name+" "+index);
            ProtocolBytes protoRet = new ProtocolBytes();
            protoRet.Addstring("EnterRoom");
            if (index < 0 || index >= RoomMgr.instance.list.Count)
            {
                Console.WriteLine("MsgEnterRoom index error "+player.Name);
                protoRet.AddInt(-1);
                protoRet.AddInt(index);
                player.Send(protoRet);
                return;
            }
            Room room = RoomMgr.instance.list[index];
            if (room.status != Room.Status.Prepare)
            {
                Console.WriteLine("MsgEnterRoom status error "+player.Name);
                protoRet.AddInt(-1);
                player.Send(protoRet);
                return;
            }
            //添加
            if (room.AddPlayer(player))
            {
                room.Brodcast(room.GetRoomInfo());
                protoRet.AddInt(0);
                player.Send(protoRet);
            }
            else
            {
                Console.WriteLine("MsgEnterRoom Maxplayer error " + player.Name);
                protoRet.AddInt(-1);
                player.Send(protoRet);
            }
        }
        //获取房间信息
        public void MsgGetRoomInfo(Player player,ProtocolBase protocoBase)
        {
            if (player.tempData.status != PlayerTempData.Status.Room)
            {
                Console.WriteLine("MsgGetRoomInfo status err "+player.Name);
                return;
            }
            Room room = player.tempData.room;
            player.Send(room.GetRoomInfo());
        }
        //离开房间
        public void MsgLeaveRoom(Player player,ProtocolBase protocoBase)
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.Addstring("LeaveRoom");
            if (player.tempData.status != PlayerTempData.Status.Room)
            {
                Console.WriteLine("MsgLeaveRoom statu err "+player.Name);
                proto.AddInt(-1);
                player.Send(proto);
                return;
            }
            
            Room room = player.tempData.room;
            RoomMgr.instance.LeaveRoom(player);
            proto.AddInt(0);
            player.Send(proto);
            if (room != null)
                room.Brodcast(room.GetRoomInfo());

        }
    }
    class HandRoomMsg
    {

    }
}
