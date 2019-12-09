using LockStepServer1._0.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LockStepServer1._0.Protocol;
/**
 * @fursion 2019-11-12
 * E-Mail fursion@fursion.cn
 * Copyright (c) 2019 fursion.All rights reserved.
 */
namespace LockStepServer1._0.ROOM
{
    class FriendMC
    {
        public static FriendMC A;
        public Dictionary<string, Player> OnlinePlayerList = new Dictionary<string, Player>();
        public Dictionary<string, Friend> OnlinePlayerInfoList = new Dictionary<string, Friend>();
        public FriendMC()
        {
            A = this;
            Console.WriteLine("FriendMC 启动成功");
        }
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vs"></param>
        public void AddFriend(Player player, object[] vs)
        {
            string TarID = vs[FriendVar.TargetID].ToString();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.AddApply);
            if (DataMgr.instance.AddFriend(player, TarID))
            {
                bytes.AddData(0);
                player.Send(bytes);
                Console.WriteLine("添加成功");
            }
            else
            {
                bytes.AddData(1);
                player.Send(bytes);
                Console.WriteLine("添加失败");
            }
        }
        public void AddOnlinePlayer(string Openid, Player player, Friend friend)
        {
            if (OnlinePlayerList.ContainsKey(Openid))
            {
                if (OnlinePlayerInfoList.ContainsKey(Openid))
                    return;
                else
                {
                    OnlinePlayerInfoList.Add(Openid, friend);
                    return;
                }
            }
            else
            {
                OnlinePlayerList.Add(Openid, player);
                if (OnlinePlayerInfoList.ContainsKey(Openid))
                    return;
                else
                {
                    OnlinePlayerInfoList.Add(Openid, friend);
                    return;
                }
            }
        }
        public void DelOnlinePlayer(string Openid)
        {
            if (!OnlinePlayerList.ContainsKey(Openid))
            {
                if (!OnlinePlayerInfoList.ContainsKey(Openid))
                    return;
                else
                {
                    OnlinePlayerInfoList.Remove(Openid);
                }
            }
            else
            {
                OnlinePlayerList.Remove(Openid);
                if (OnlinePlayerInfoList.ContainsKey(Openid))
                    OnlinePlayerInfoList.Remove(Openid);
                return;
            }

        }
        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vs"></param>
        public void DelFriend(Player player, object[] vs)
        {
            string TarID = vs[FriendVar.TargetID].ToString();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.AddApply);
            if (DataMgr.instance.DelFriend(player.Openid, TarID))
            {
                bytes.AddData(0);
                player.Send(bytes);
            }
            else
            {
                bytes.AddData(1);
                player.Send(bytes);
            }
        }
        public void ApplyAddFriend(Player player, object[] vs)
        {
            string TarID = vs[FriendVar.TargetID].ToString();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.AddApply);
            if (DataMgr.instance.AddApplyFriend(player, TarID))
            {
                bytes.AddData(0);
                player.Send(bytes);
                if (OnlinePlayerList.ContainsKey(TarID))
                {
                    ProtocolBytes byt = new ProtocolBytes();
                    byt.AddData(FriendVar.UpdateList);
                    OnlinePlayerList[TarID].Send(byt);
                }
                Console.WriteLine("添加申请成功");
                if (OnlinePlayerList.ContainsKey(TarID))
                {
                    ProtocolBytes OnlneApply = new ProtocolBytes();
                    OnlneApply.AddData(FriendVar.OnlineApply);
                    OnlneApply.AddData(player.Openid);
                    UserData UD = DataMgr.instance.GetUserData(player.Openid);
                    string UDStr = JsonConvert.SerializeObject(UD);
                    OnlneApply.AddData(UDStr);
                    OnlinePlayerList[TarID].Send(OnlneApply);
                    bytes.AddData(0);
                    player.Send(bytes);
                    return;
                }
            }
            else
            {
                bytes.AddData(1);
                player.Send(bytes);
                Console.WriteLine("添加申请失败");
            }

        }
        public void delApply(Player player, object[] vs)
        {
            string TarID = vs[FriendVar.TargetID].ToString();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.DelApply);
            if (DataMgr.instance.DelApplyFriend(player.Openid, TarID))
            {
                bytes.AddData(0);
                player.Send(bytes);
                Console.WriteLine("成功");
            }
            else
            {
                bytes.AddData(1);
                player.Send(bytes);
                Console.WriteLine("失败");
            }
        }
        public void AddBlackList(Player player, object[] vs)
        {
            string TarID = vs[FriendVar.TargetID].ToString();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.AddApply);
            if (DataMgr.instance.AddBlackFriend(player, TarID))
            {
                bytes.AddData(0);
                player.Send(bytes);
            }
            else
            {
                bytes.AddData(1);
                player.Send(bytes);
            }
        }
        public void DelBlackList(Player player, object[] vs)
        {
            string TarID = vs[FriendVar.TargetID].ToString();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.AddApply);
            if (DataMgr.instance.DelBlackFriend(player.Openid, TarID))
            {
                bytes.AddData(0);
                player.Send(bytes);
            }
            else
            {
                bytes.AddData(1);
                player.Send(bytes);
            }
        }
        public void GetFriendListInfo(Player player)
        {
            DataMgr.instance.GetFriend(player);
        }
        public void RetFriendList(Friend friend, Player player)
        {
            AddOnlinePlayer(player.Openid, player, friend);
            if (friend.GoodList.Keys.Count != 0)
            {
                foreach (string id in friend.GoodList.Keys)
                {
                    friend.GoodList[id].data = DataMgr.instance.GetUserData(id);
                    if (OnlinePlayerList.ContainsKey(id))
                    {
                        friend.GoodList[id].OnlineState = true;
                    }
                }
            }
            if (friend.ApplyList.Keys.Count != 0)
            {
                foreach (string id in friend.ApplyList.Keys)
                {
                    friend.ApplyList[id].data = DataMgr.instance.GetUserData(id);
                }
            }
            if (friend.BlackList.Keys.Count != 0)
            {
                foreach (string id in friend.BlackList.Keys)
                {
                    friend.BlackList[id].data = DataMgr.instance.GetUserData(id);
                }
            }
            string FriendListStr = JsonConvert.SerializeObject(friend);
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.GetFriendListInfo);
            bytes.AddData(FriendListStr);
            player.Send(bytes);
        }
        public Friend InitFriendListInfo(Player player)
        {
            try
            {
                Friend friend = DataMgr.instance.GetFriend(player.Openid);
                AddOnlinePlayer(player.Openid, player, friend);
                if (friend.GoodList.Keys.Count != 0)
                {
                    foreach (string id in friend.GoodList.Keys)
                    {
                        friend.GoodList[id].data = DataMgr.instance.GetUserData(id);
                    }
                }
                if (friend.ApplyList.Keys.Count != 0)
                {
                    foreach (string id in friend.ApplyList.Keys)
                    {
                        friend.ApplyList[id].data = DataMgr.instance.GetUserData(id);
                    }
                }
                if (friend.BlackList.Keys.Count != 0)
                {
                    foreach (string id in friend.BlackList.Keys)
                    {
                        friend.BlackList[id].data = DataMgr.instance.GetUserData(id);
                    }
                }
                return friend;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "  200");
                return null;
            }
        }
        public void OnlineNotic(Player player, object[] vs)
        {

        }
        public void test(string s)
        {
            string NackName = s;
            Dictionary<string, UserData> d = new Dictionary<string, UserData>();
            d = DataMgr.instance.FindUser(NackName);
            Console.WriteLine("count    " + d.Count);
        }
        public void FindUser(Player player, object[] vs)
        {
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.AddData(FriendVar.FindUser);
            string NackName = vs[2].ToString();
            FindUserList FUL = new FindUserList
            {
                UserList = DataMgr.instance.FindUser(NackName)
            };
            if (FUL.UserList.Count == 0)
            {
                bytes.AddData(1);
                player.Send(bytes);
                return;
            }
            string FULStr = JsonConvert.SerializeObject(FUL);
            bytes.AddData(0);
            bytes.AddData(FULStr);
            player.Send(bytes);
        }
    }
}
