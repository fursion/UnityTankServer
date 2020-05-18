using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace LockStepServer1._0.LockStep
{
    class SynTest
    {
        public static SynTest The;
        public LockStepMGR mGR = new LockStepMGR();
        public List<EndPoint> P_UDP_IP = new List<EndPoint>();
        public SynTest()
        {
            The = this;
            InstructFrame instructFrame = new InstructFrame
            {
                Openid = "DJ"
            };
            instructFrame.Synchro.TouchDirection = Direction.Down;
            instructFrame.Synchro.TouchPosition.Fill(new UnityEngine.Vector2(1, 1));
            Console.WriteLine(JsonConvert.SerializeObject(instructFrame));
        }
        public void Start()
        {
            mGR.UDP_ClientList = P_UDP_IP;
            mGR.Start();
        }
    }
}
