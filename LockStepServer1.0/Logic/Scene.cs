using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankServerTest.Core;

namespace TankServerTest.Logic
{
    class Scene
    {
        public static Scene instance;
        public Scene()
        {
            instance = this;
        }
        List<ScenePlayer> list = new List<ScenePlayer>();
        //根据名字获取
        private ScenePlayer GetScenePlayer(string id)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i].id == id)
                    return list[i];
            }
            return null;
        }
        public void AddPlayer(string id)
        {
            lock (list)
            {
                ScenePlayer p = new ScenePlayer();
                p.id = id;
                list.Add(p);
            }
        }
        public void DelPlayer(string id)
        {
            lock (list)
            {
                ScenePlayer p = GetScenePlayer(id);
                list.Remove(p);
            }
            ProtocoBytes protoco = new ProtocoBytes();
            protoco.Addstring("PlayerLeave");
            protoco.Addstring(id);
            ServerNet.instance.Broadcast(protoco);
        }
        //发送list
        public void SendPlayerList(Player player)
        {
            int count = list.Count;
            ProtocoBytes protoco = new ProtocoBytes();
            protoco.Addstring("GetList");
            protoco.AddInt(count);
            for(int i = 0; i < count; i++)
            {
                ScenePlayer p = list[i];
                protoco.Addstring(p.id);
                protoco.AddFloat(p.x);
                protoco.AddFloat(p.y);
                protoco.AddFloat(p.z);
                protoco.AddInt(p.score);
            }
            player.Send(protoco);
        }
        public void UpdateInfo(string id,float x,float y,float z,int score)
        {
            int count = list.Count;
            ProtocoBytes protoco = new ProtocoBytes();
            ScenePlayer p = GetScenePlayer(id);
            if (p == null)
                return;
            p.x = x;
            p.y = y;
            p.z = z;
            p.score = score;
        }
    }
}
