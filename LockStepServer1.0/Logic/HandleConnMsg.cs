using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankServerTest.Core;

namespace TankServerTest.Logic
{
    class HandleConnMsg
    {
        public void MsgHearBeat(Conn conn, ProtocoBase proto)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间]" + conn.GetAddress());
        }
        //注册  
        //参数 str 用户名 PW 密码
        //返回协议 -1失败 0 成功
        public void MsgRegister(Conn conn, ProtocoBase protocoBase)
        {
            int start = 0;
            ProtocoBytes protoco = (ProtocoBytes)protocoBase;
            string protoName = protoco.GetString(start, ref start);
            string id = protoco.GetString(start, ref start);
            string pw = protoco.GetString(start, ref start);
            string strFromat = "[收到注册协议]" + conn.GetAddress();
            Console.WriteLine(strFromat + "用户名" + id + "密码" + pw);
            protoco = new ProtocoBytes();
            protoco.Addstring("Register");
            if (DataMgr.instance.Register(id, pw))
                protoco.AddInt(0);
            else
                protoco.AddInt(-1);
            DataMgr.instance.CreatPlayer(id);
            conn.Send(protoco);
            Console.WriteLine(protoco.GetName()+"***"+protoco.GetInt(start,ref start));
        }
        //登录    
        public void MsgLogin(Conn conn, ProtocoBase protocoBase)
        {
            int start = 0;
            ProtocoBytes protoco = (ProtocoBytes)protocoBase;
            string protoName = protoco.GetString(start, ref start);
            string id = protoco.GetString(start, ref start);
            string pw = protoco.GetString(start, ref start);
            string strFromat = "[收到登录协议]" + conn.GetAddress();
            Console.WriteLine(strFromat + "用户名" + id + "密码" + pw);

            ProtocoBytes protocoRet = new ProtocoBytes();
            protocoRet.Addstring("Login");
            if (!DataMgr.instance.CheckPassWordAndId(id, pw))
            {
                protocoRet.AddInt(-1);
                conn.Send(protocoRet);
                Console.WriteLine("登录失败*密码错误");
                return;
            }
            //是否已经登录
            ProtocoBytes protocoLogout = new ProtocoBytes();
            protocoLogout.Addstring("Logout");
            if (!Player.Kickoff(id, protocoLogout))
            {
                protocoRet.AddInt(-1);
                Console.WriteLine("重复登录");
                conn.Send(protocoRet);
            }
            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {
                protocoRet.AddInt(-1);
                conn.Send(protocoRet);
                Console.WriteLine("没有玩家数据");
                return;
            }
            conn.Player = new Player(id, conn)
            {
                data = playerData
            };
            ServerNet.instance.handlePlayerEvent.OnLogin(conn.Player);
            protocoRet.AddInt(0);
            conn.Send(protocoRet);
            Console.WriteLine("登录成功******发送"+protocoRet.GetInt(start,ref start));
            return;
        }
        public void MsgLogout(Conn conn, ProtocoBase protocoBase)
        {
            ProtocoBytes protocoBytes = new ProtocoBytes();
            protocoBytes.Addstring("Logout");
            protocoBytes.AddInt(0);
            if (conn.Player == null)
            {
                conn.Send(protocoBytes);
                conn.Close();
            }
            else
            {
                conn.Send(protocoBytes);
                conn.Player.Logout();
            }
        }
    }
}
