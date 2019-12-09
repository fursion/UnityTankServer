using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;

public enum State
{
    Friend,
    Wrold,
    Team
}
namespace LockStepServer1._0.Core
{
    [Serializable]
    public class PlayerData
    {
        public string Openid;
        public int score = 0;
        public int win = 0;
        public int fail = 0;
        public PlayerData()
        {
            score = 100;
        }
        public Dictionary<string, int> Contacts;//ID NickName
    }
    [Serializable]
    class Friend
    {
        public Dictionary<string, FriendInfo> GoodList = new Dictionary<string, FriendInfo>();
        public Dictionary<string, FriendInfo> BlackList = new Dictionary<string, FriendInfo>();
        public Dictionary<string, FriendInfo> ApplyList = new Dictionary<string, FriendInfo>();
    }

    [Serializable]
    public class UserData
    {
        public string NickNeam;
        public string NickWebPath;
        public string Openid;
    }

    [Serializable]
    public class FriendInfo
    {
        public bool OnlineState = false;
        public UserData data;
    }
    [Serializable]
    public class FindUserList
    {
        public Dictionary<string, UserData> UserList = new Dictionary<string, UserData>();
    }
    [Serializable]
    public class TeamMemberList
    {
        public UserData[] MembetList;
    }
    [Serializable]
    public class ServerInfo
    {
        public string ServerVrsion;
        public string IP;
        public int Port;
    }
    [Serializable]
    public class NewMSG
    {
        public string Source;
        public string Destination;
        public string MSGText;
        public State MSGState;
    }
}
