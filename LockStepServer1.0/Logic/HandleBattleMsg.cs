using LockStepServer1._0.Core;
using Fursion.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LockStepServer1._0.Logic
{
    partial class HandlePlayerMsg
    {
        public void StartFight(Player player,ProtocolBase protocoBase)
        {
            //Room room = player.tempData.room;
            //if (!room.CanStart())
            //{
            //    Console.WriteLine("MsgStartFight canstart err "+player.Name);
            //    proto.AddInt(-1);
            //    player.Send(proto);
            //    return;
            //}
            //proto.AddInt(0);
            //player.Send(proto);
            //room.StartFight();
        }
        public void UpdateUnitInfo(Player player,ProtocolBase protocoBase)
        {
            //int start = 0;
            //ProtocolBytes proto = (ProtocolBytes)protocoBase;
            //string protoName = proto.GetString(start, ref start);
            //float posX = proto.GetFloat(start, ref start);
            //float posY = proto.GetFloat(start, ref start);
            //float posZ = proto.GetFloat(start, ref start);
            //float rotX = proto.GetFloat(start, ref start);
            //float rotY = proto.GetFloat(start, ref start);
            //float rotZ = proto.GetFloat(start, ref start);
            //if (player.tempData.status != PlayerTempData.Status.Fight)
            //    return;
            //Room room = player.tempData.room;
            //player.tempData.posX = posX;
            //player.tempData.posY = posY;
            //player.tempData.posZ = posZ;
            //player.tempData.lastShootTime = Sys.GetTimeStamp();
            //ProtocolBytes protocRet = new ProtocolBytes();
            //protocRet.Addstring("UpdateUnitInfo");
            //protocRet.Addstring(player.Name);
            //protocRet.AddFloat(posX);
            //protocRet.AddFloat(posY);
            //protocRet.AddFloat(posZ);
            //protocRet.AddFloat(rotX);
            //protocRet.AddFloat(rotY);
            //protocRet.AddFloat(rotZ);
            //room.Brodcast(protocRet);
        }
        public void MSG()
        {

        }
    }
    class HandleBattleMsg
    {
    }
}
