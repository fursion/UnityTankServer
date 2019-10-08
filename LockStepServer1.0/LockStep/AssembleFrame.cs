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
        public static AssembleFrame instance;
        public AssembleFrame()
        {
            instance = this;
        }
        public float Last_FPS_Time;
        public int FPS_id = 0;
        public int count = 0;
        public bool Proing;//RecFps状态指示
        public ProtocolBytes DataFPS;
        public ProtocolBytes TempFPS;
        public void FPSInit()
        {
            count = 0;
            DataFPS = new ProtocolBytes();
            TempFPS = new ProtocolBytes();
            DataFPS.Addstring("Lockstep");
            DataFPS.AddInt(FPS_id);
            DataFPS.AddInt(1);
            FPS_id++;
        }
        public bool RecFps(ProtocolBase protoc)
        {
            Proing = true;
            count++;
            ProtocolBytes pro = (ProtocolBytes)protoc;
            int start = 0;
            string proName = pro.GetString(start, ref start);
            int RoomId = pro.GetInt(start, ref start);
            int C_FPS_id = pro.GetInt(start, ref start);
            string playerid = pro.GetString(start, ref start);
            string opName = pro.GetString(start, ref start);
            float op = pro.GetFloat(start, ref start);
            Console.WriteLine(proName + " " + RoomId + " " + C_FPS_id + " " + playerid + " " + opName + " " + op);
            TempFPS.Addstring(playerid);
            TempFPS.Addstring(opName);
            TempFPS.AddFloat(op);
            Console.WriteLine(playerid + " " + opName + " " + op);
            //Proing = false;
            //return TempFPS;
            return Proing = false;
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
