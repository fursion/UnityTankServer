using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.Core;
using LockStepServer1._0.Protocol;
using LockStepServer1._0.NetWorking;

namespace LockStepServer1._0.Logic
{
    partial class HandlePlayerMsg
    {
        public void MsgGetScore(Player player, ProtocolBase protocoBase)
        {
            ProtocolBytes protocoBytes = new ProtocolBytes();
            protocoBytes.Addstring("GetScore");
            protocoBytes.AddInt(player.data.score);
            player.Send(protocoBytes);
            Console.WriteLine("MsgGetScore" + player.Name, player.data.score);
        }
        public void MsgAddScore(Player player, ProtocolBase protocoBase)
        {
            int start = 0;
            ProtocolBytes protocoBytes = (ProtocolBytes)protocoBase;
            string protoName = protocoBytes.GetString(start, ref start);
            player.data.score += 1;
            Console.WriteLine(protoName + "[MsgAddScore] " + player.Name + " " + player.data.score.ToString());
        }
        public void MsgGetList(Player player, ProtocolBase protocoBase)
        {
            Scene.instance.SendPlayerList(player);
        }
        public void MsgUpdateInfo(Player player, ProtocolBase protocoBase)
        {
            int start = 0;
            ProtocolBytes protoc = (ProtocolBytes)protocoBase;
            string protoName = protoc.GetString(start, ref start);
            float x = protoc.GetFloat(start, ref start);
            float y = protoc.GetFloat(start, ref start);
            float z = protoc.GetFloat(start, ref start);
            int score = player.data.score;
            Scene.instance.UpdateInfo(player.Name, x, y, z, score);
            ProtocolBytes protoRet = new ProtocolBytes();
            protoRet.Addstring("UpdateInfo");
            protoRet.Addstring(player.Name);
            protoRet.AddFloat(x);
            protoRet.AddFloat(y);
            protoRet.AddFloat(z);
            protoRet.AddInt(score);
            NMC.instance.Broadcast(protoRet);
        }
        //获取玩家信息
        public void MsgGetAchieve(Player player, ProtocolBase protocoBase)
        {
            ProtocolBytes protoRet = new ProtocolBytes();
            protoRet.Addstring("GetAchieve");
            protoRet.Addstring(player.Name);
            player.Send(protoRet);
            Console.WriteLine("MsgGetAchieve" + player.Name);
        }
        public void MsgGetPing(Player player, ProtocolBase protocoBase)
        {
            ProtocolBytes protoc = new ProtocolBytes();
            protoc.Addstring("GetPing");
            player.Send(protoc);
            Console.WriteLine("MsgGetPing" + player.Name);
        }
        public void MsgMSG(ProtocolBytes bytes)
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
