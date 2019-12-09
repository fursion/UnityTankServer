using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ProtocolConst
{
    public static readonly int True = 0;
    public static readonly int False = 1;
    public static readonly int def = -1;
    public static readonly string CheckOpenid = "CheckOpenid";
    public static readonly string HearBeat = "HearBeat";
    public static readonly string Logout = "Logout";
    public static readonly string Login = "Login";
    public static readonly string MSG = "MSG";
    public static readonly string LockStep = "LockStep";
    public static readonly string FPS = "FPS";
}
public static class TeamVar
{
    public static readonly string IntoTeam = "IntoTeam";
    public static readonly string ExitTeam = "ExitTeam";
    public static readonly string CreateTeam = "CreateTeam";
    public static readonly int Team_Max_player = 1;
    public static readonly string Kick_Out = "Kick_Out";
    public static readonly string Team_Invitation = "TeamInvitation";
    public static readonly string OneIntoTeam = "OneIntoTeam";
    public static readonly string RetInit = "RetInit";
}
public static class FriendVar
{
    public static readonly int TargetID = 2;
    public static readonly string Friend = "Friend";
    public static readonly string GetFriendListInfo = "GetFriendListInfo";
    public static readonly string AddFriend = "AddFriend";
    public static readonly string DelFriend = "DelFriend";
    public static readonly string AddBlackList = "AddBlackList";
    public static readonly string DelBlackList = "DelBlackList";
    public static readonly string AddApply = "AddApply";
    public static readonly string DelApply = "DelApply";
    public static readonly string UpdateList = "UpdateList";
    public static readonly string FindUser = "FindUser";
    public static readonly string OnlineApply = "OnlineApply";
    public static readonly string OnlineNotice = "OnlineNotice";
}
public static class ServerMCVar
{
    public static readonly string ServerInfo = "ServerInfo";
}