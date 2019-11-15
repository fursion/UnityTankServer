using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.NetWorking;

namespace LockStepServer1._0.Logic
{
    public class World
    {
        public static World instance;
        public World()
        {
            instance = this;
        }
        public List<object[]> WorldMSG = new List<object[]>();
        public void NewMSG(ProtocolBytes bytes)
        {
            WorldMSG.Add(bytes.GetDecode());
            if (WorldMSG.Count > 100)
                WorldMSG.RemoveAt(0);
            NMC.instance.Broadcast(bytes);
        }
    }
}
