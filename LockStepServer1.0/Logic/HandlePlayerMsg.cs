using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankServerTest.Core;

namespace TankServerTest.Logic
{
    partial class HandlePlayerMsg
    {
        public void MsgGetScore(Player player,ProtocoBase protocoBase)
        {
            ProtocoBytes protocoBytes = new ProtocoBytes();
            protocoBytes.Addstring("GetScore");
            protocoBytes.AddInt(player.data.score);
            player.Send(protocoBytes);
            Console.WriteLine("MsgGetScore"+player.id,player.data.score);
        }
        public void MsgAddScore(Player player,ProtocoBase protocoBase)
        {
            int start = 0;
            ProtocoBytes protocoBytes = (ProtocoBytes)protocoBase;
            string protoName = protocoBytes.GetString(start, ref start);
            player.data.score +=1;
            Console.WriteLine(protoName+"[MsgAddScore] "+player.id+" "+player.data.score.ToString());
        }
        public void MsgGetList(Player player,ProtocoBase protocoBase)
        {
            Scene.instance.SendPlayerList(player);
        } 
        public void MsgUpdateInfo(Player player,ProtocoBase protocoBase)
        {
            int start = 0;
            ProtocoBytes protoc = (ProtocoBytes)protocoBase;
            string protoName = protoc.GetString(start, ref start);
            float x = protoc.GetFloat(start, ref start);
            float y = protoc.GetFloat(start, ref start);
            float z = protoc.GetFloat(start, ref start);
            int score = player.data.score;
            Scene.instance.UpdateInfo(player.id, x, y, z, score);
            ProtocoBytes protoRet = new ProtocoBytes();
            protoRet.Addstring("UpdateInfo");
            protoRet.Addstring(player.id);
            protoRet.AddFloat(x);
            protoRet.AddFloat(y);
            protoRet.AddFloat(z);
            protoRet.AddInt(score);
            ServerNet.instance.Broadcast(protoRet);
        }
        //获取玩家信息
        public void MsgGetAchieve(Player player,ProtocoBase protocoBase)
        {
            ProtocoBytes protoRet = new ProtocoBytes();
            protoRet.Addstring("GetAchieve");
            protoRet.Addstring(player.id);
            player.Send(protoRet);
            Console.WriteLine("MsgGetAchieve" + player.id);
        }
        public void MsgGetPing(Player player,ProtocoBase protocoBase)
        {
            ProtocoBytes protoc = new ProtocoBytes();
            protoc.Addstring("GetPing");
            player.Send(protoc);
            Console.WriteLine("MsgGetPing" + player.id);
        }
    }
}
