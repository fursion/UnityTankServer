using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LockStepServer1._0.Core;
using LockStepServer1._0.NetWorking;
using LockStepServer1._0.Protocol;

namespace LockStepServer1._0.LockStep
{
    class LockStepMGR
    {
        //public Room room;
        public static LockStepMGR instance;
        Timer timer = new Timer(50);
        public ProtocolBase Proto;
        private int S_Farmid = 0;
        public List<ProtocolBytes> FPS = new List<ProtocolBytes>();
        public List<EndPoint> P_UDP_IP = new List<EndPoint>();
        public Dictionary<EndPoint, ProtocolBase> Rep_Fps_data = new Dictionary<EndPoint, ProtocolBase>();
        public float Last_FPS_Time = 0;
        public AssembleFrame receFPS;
        public LockStepMGR()
        {
            instance = this;
            Console.WriteLine("启动 Room FPSMgr");
            receFPS = new AssembleFrame();
            receFPS.FPSInit();
            Console.WriteLine("启动RecvFPS");
        }
        public void SendFps(object sender, ElapsedEventArgs e)
        {
            //ProtocoBytes protoc = new ProtocoBytes();
            //protoc.Addstring("FPS");
            //if (UDP.instance.clientEnd.ToString() != "0.0.0.0:0")
            //{
            //    UDP.instance.SocketSend(protoc, UDP.instance.clientEnd);
            //    Console.WriteLine("FPS");
            //}
            //return;
            //while (true)
            //    if (!receFPS.Proing)
            //    {
            //        receFPS.Merge();
            //        receFPS.FPSInit();
            //        int start = 0;
            //        string protName = proto.GetString(start, ref start);
            //        int FPS_id = proto.GetInt(start, ref start);
            //        int fps_cound = proto.GetInt(start, ref start);
            //        int count = proto.GetInt(start, ref start);
            //        Console.WriteLine("发送逻辑帧FPS_ID" + FPS_id);
            //        Console.WriteLine("count" + count);
            //        for (int i = 0; i < count; i++)
            //        {
            //            string playerid = proto.GetString(start, ref start);
            //            string opName = proto.GetString(start, ref start);
            //            float op = proto.GetFloat(start, ref start);
            //            Console.WriteLine("playerid=" + playerid + "  " + "opName=" + opName + " " + "op=" + op);
            //        }
            //        FPS.Add((ProtocolBytes)Proto);
            //        return;
            //    }
        }
        public void Start()
        {
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(SendTest);//
        }
        public void SendTest(object sender, ElapsedEventArgs e)
        {
            S_Farmid++;
            //Console.WriteLine("timer");
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddData("Lockstep");
            proto.AddData(S_Farmid);
            if ((S_Farmid % 20) == 0)
                proto.AddData(5);
            proto.AddData(0);
            int COUNT = P_UDP_IP.Count;
            //Console.WriteLine(P_UDP_IP.Count);
            for (int i = 0; i < COUNT; i++)
            {
                try
                {
                    Console.WriteLine(proto.ToString());
                    if (P_UDP_IP == null)
                        return;
                    UDP.instance.SocketSend(proto, P_UDP_IP[i]);
                    Console.WriteLine("LS");
                }
                catch (Exception a)
                {
                    Console.WriteLine("ERR");
                    Console.WriteLine(a.Message);
                }
            }
        }
        public ProtocolBase HisFps(int Fpsid)
        {
            int star = 0;
            int fps_count = receFPS.FPS_id - Fpsid;
            string name = receFPS.DataFPS.GetString(star, ref star);
            int FpsId = receFPS.DataFPS.GetInt(star, ref star);
            int defcount = receFPS.DataFPS.GetInt(star, ref star);
            int opcount = receFPS.DataFPS.GetInt(star, ref star); ;
            ProtocolBytes prot = new ProtocolBytes();
            prot.Addstring(name);
            prot.AddInt(FpsId);
            prot.AddInt(fps_count);
            for (int i = Fpsid + 1; i < FPS.Count; i++)
            {
                int start = 0;
                string ProtName = FPS[i].GetString(start, ref start);
                int fps_id = FPS[i].GetInt(start, ref start);
                int Fps_count = FPS[i].GetInt(start, ref start);
                int msg_count = FPS[i].GetInt(start, ref start);
                prot.AddInt(Fpsid);
                prot.AddInt(msg_count);
                for (int t = 0; t < msg_count; t++)
                {
                    string playerid = FPS[i].GetString(start, ref start);
                    string opname = FPS[i].GetString(start, ref start);
                    float op = FPS[i].GetFloat(start, ref start);
                    prot.Addstring(playerid);
                    prot.Addstring(opname);
                    prot.AddFloat(op);
                }
            }
            prot.AddInt(FpsId);
            for (int c = 0; c < opcount; c++)
            {
                string playerid = FPS[c].GetString(star, ref star);
                string opname = FPS[c].GetString(star, ref star);
                float op = FPS[c].GetFloat(star, ref star);
                prot.Addstring(playerid);
                prot.Addstring(opname);
                prot.AddFloat(op);
            }
            return prot;
        }
    }
}
