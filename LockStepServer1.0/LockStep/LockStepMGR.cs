using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LockStepServer1._0.Core;
using LockStepServer1._0.NetWorking;
using Fursion.Protocol;
using Newtonsoft.Json;

namespace LockStepServer1._0.LockStep
{
    class LockStepMGR
    {
        Timer timer = new Timer(50);
        public ProtocolBase Proto;
        private int S_Farmid { get; set; } = 0;
        public LogicFrame NowLogicFrame = new LogicFrame();
        public List<LogicFrame> HistoryFrame = new List<LogicFrame>();
        public List<EndPoint> UDP_ClientList { get; set; } = new List<EndPoint>();
        public float Last_FPS_Time = 0;
        public LockStepMGR()
        {
            Console.WriteLine("Start Room FPSMgr");
            Console.WriteLine("Start RecvFPS");
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
            LogicFrameStr.SetProtocol(Fursion_Protocol.LockStep);
            lock (NowLogicFrame.instructFrames)
            {
                Console.WriteLine(JsonConvert.SerializeObject(NowLogicFrame));
                LogicFrameStr.AddData(JsonConvert.SerializeObject(NowLogicFrame));
            }
            foreach (EndPoint endPoint in UDP_ClientList)
            {
                Console.WriteLine(LogicFrameStr.bytes.Length);
                UDP.instance.SocketSend(LogicFrameStr, endPoint);
            }
            NowLogicFrame = new LogicFrame();
            S_Farmid++;
            NowLogicFrame.LogicFrameID = S_Farmid;
        }
        public void PackageLogicFrame(InstructFrame instructFrame)
        {
            lock (NowLogicFrame.instructFrames)
            {
                NowLogicFrame.instructFrames.Add(instructFrame);
            }
        }
    }
}
