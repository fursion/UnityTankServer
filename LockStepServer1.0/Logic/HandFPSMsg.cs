using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankServerTest.Core;
using TankServerTest.FPS;

namespace TankServerTest.Logic
{
    class HandFPSMsg
    {
        public void MsgFPS(ProtocoBase protocoBase)
        {
            Console.WriteLine("处理FPS");
            ProtocoBytes proto = (ProtocoBytes)protocoBase;
            int start = 0;
            string ProtoName = proto.GetString(start, ref start);
            int RoomId = proto.GetInt(start, ref start);
            int C_FPS_id = proto.GetInt(start, ref start);
            RoomMgr.instance.list[RoomId].FPS.receFPS.RecFps(protocoBase);
            //ReceFPS.instance.RecFps(player,protocoBase);
        }
    }
}
