using LockStepServer1._0.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.LockStep
{
    class AssembleFrame
    {
        public float Last_FPS_Time;
        public int FPS_id = 0;
        public int count = 0;
        public bool Proing;//RecFps状态指示
        public ProtocolBytes DataFPS;
        public ProtocolBytes TempFPS;
        public void FPSInit()
        {
            LogicFrame logicFrame = new LogicFrame();
        }
        public bool RecFps(ProtocolBase protoc)
        {
            return true;
        }
        public void Merge()//合并fps
        {
            int start = 0;
            DataFPS.AddInt(count);
            for (int i = 0; i < count; i++)
            {
                string playerid = TempFPS.GetString(start, ref start);
                string opName = TempFPS.GetString(start, ref start);
                float op = TempFPS.GetFloat(start, ref start);
                DataFPS.Addstring(playerid);
                DataFPS.Addstring(opName);
                DataFPS.AddFloat(op);
            }
        }
    }
}
