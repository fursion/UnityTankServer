using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.Core;
using Fursion.Protocol;
using LockStepServer1._0.NetWorking;

namespace LockStepServer1._0.Logic
{
    partial class HandlePlayerMsg
    {
        public void UpdateInfo(Player player, ProtocolBase protocoBase)
        {

        }
        public void MSG(ProtocolBytes bytes)
        {
            Console.WriteLine(bytes.GetDecode()[1]);
            switch (bytes.GetDecode()[1])
            {
                case "TeamMSG": break;
                case "WrodMSG": World.instance.NewMSG(bytes); break;
            }
        }
    }
}
