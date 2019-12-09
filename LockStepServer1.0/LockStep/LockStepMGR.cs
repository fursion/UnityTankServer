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
using Newtonsoft.Json;

namespace LockStepServer1._0.LockStep
{
    class LockStepMGR
    {
        Timer timer = new Timer(50);
        public ProtocolBase Proto;
        private int S_Farmid = 0;
        public LogicFrame NowLogicFrame;
        public List<LogicFrame> HistoryFrame = new List<LogicFrame>();
        public List<EndPoint> P_UDP_IP = new List<EndPoint>();
        public float Last_FPS_Time = 0;
        public LockStepMGR()
        {
            Console.WriteLine("启动 Room FPSMgr");
            Console.WriteLine("启动RecvFPS");
        }
        public void SendFps(object sender, ElapsedEventArgs e)
        {

        }
        public void Start()
        {
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(SendTest);
        }
        public void SendTest(object sender, ElapsedEventArgs e)
        {
            ProtocolBytes LogicFrameStr = new ProtocolBytes();
            LogicFrameStr.AddData("LockStep");
            LogicFrameStr.AddData(JsonConvert.SerializeObject(NowLogicFrame));
            foreach(EndPoint endPoint in P_UDP_IP)
            {
                UDP.instance.SocketSend(LogicFrameStr, endPoint);
            }
            NowLogicFrame = new LogicFrame();
            NowLogicFrame.LogicFrameID++;
        }
        public void PackageLogicFrame(InstructFrame instructFrame)
        {
            NowLogicFrame.instructFrames.Add(instructFrame);
        }
    }
}
