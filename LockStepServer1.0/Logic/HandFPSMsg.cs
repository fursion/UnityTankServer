using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.Core;
using LockStepServer1._0.LockStep;

namespace LockStepServer1._0.Logic
{
    class HandFPSMsg
    {
        public void MsgFPS(ProtocolBase protocoBase)
        {
            Console.WriteLine("处理FPS");
            ProtocolBytes proto = (ProtocolBytes)protocoBase;
            int start = 0;
            string ProtoName = proto.GetString(start, ref start);
            int RoomId = proto.GetInt(start, ref start);
            int C_FPS_id = proto.GetInt(start, ref start);
            //ReceFPS.instance.RecFps(player,protocoBase);
        }
        public void MsgLockStep(ProtocolBase protocoBase)
        {
            Console.WriteLine("处理LockStep");
            ProtocolBytes proto = (ProtocolBytes)protocoBase;
            string ready = proto.GetDecode()[2].ToString();
            if (ready == "Ready")
            {
                LockStepMGR fPS = new LockStepMGR();//测试
                fPS.Start();
            }
        }
    }  
}
