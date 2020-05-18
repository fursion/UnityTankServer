using Fursion.Protocol;
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
        public void FPS(ProtocolBase protocoBase)
        {
            
        }
        public void LockStep(ProtocolBase protocoBase)
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
