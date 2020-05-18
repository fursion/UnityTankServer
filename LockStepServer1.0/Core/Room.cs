﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using LockStepServer1._0.LockStep;
using LockStepServer1._0.NetWorking;
using System.Timers;
using LockStepServer1._0.ROOM;
using Newtonsoft.Json;
using Fursion.Protocol;

namespace LockStepServer1._0.Core
{
    class Room
    {
        public enum Status
        {
            Prepare = 1,
            Fight = 2,
        }
        public Status status { get; set; } = Status.Prepare;
        public int RoomID { get; set; } = 0;
        public int MaxPlayer { get; set; } = 10;
        /// <summary>
        /// 返回值单位为ms,传入值单位为秒
        /// </summary>
        private int MaxWaitLoadingTime { get; set; } = 90000;//ms
        private int LoadingBroadcastTime { get; set; } = 1000;//ms
        private int WaitRightTime { get; set; } = 30000;//ms
        private int WaitSelectTime { get; set; } = 30000;
        public LockStepMGR LSM { get; set; }
        public Dictionary<string, Player> MemberList { get; set; } = new Dictionary<string, Player>();
        public List<EndPoint> UDP_ClientList { get; set; } = new List<EndPoint>();
        public Dictionary<EndPoint, int> Rep_Send_List = new Dictionary<EndPoint, int>();
        public Timer MaxWaitLoadingTimer;//最大等待加载时间定时器
        public Timer LoadTimer;//loading进度广播间隔倒计时定时器
        public Timer RightTimer;//Right倒计时定时器
        public Timer SelectTime;
        private Timer StartTimer;
        /// <summary>
        /// Room初始化
        /// </summary>
        /// <param name="players"></param>
        public void Init(Player[] players)
        {
            MaxWaitLoadingTime = 90000;
            LoadingBroadcastTime = 1000;
            WaitRightTime = 30000;
            TimeInit();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                    continue;
                players[i].room = this;
                MemberList.Add(players[i].Openid, players[i]);
                RoomPlayer roomPlayer = new RoomPlayer
                {
                    Openid = players[i].Openid,
                    Nickname = players[i].UserData.NickNeam,
                    HeadImageUrl = players[i].UserData.NickWebPath,
                    Right = false
                };
                players[i].TempData.PlayerGameInfo = roomPlayer;
            }
            GoToConfirmStage();
        }
        private void TimeInit()
        {
            RightTimer = new Timer(WaitRightTime)
            {
                AutoReset = false,
                Enabled = false
            };
            RightTimer.Elapsed += new ElapsedEventHandler(RightMathod);
            MaxWaitLoadingTimer = new Timer(MaxWaitLoadingTime)
            {
                AutoReset = false,
                Enabled = false
            };
            MaxWaitLoadingTimer.Elapsed += new ElapsedEventHandler(LoadTimeExceededStart);
            LoadTimer = new Timer(LoadingBroadcastTime)
            {
                AutoReset = true,
                Enabled = false
            };
            LoadTimer.Elapsed += new ElapsedEventHandler(SendLoading);
            SelectTime = new Timer(WaitSelectTime)
            {
                AutoReset = false,
                Enabled = false,
            };
            SelectTime.Elapsed += new ElapsedEventHandler(SelectToLoading);
            StartTimer = new Timer(5000)
            {
                AutoReset = false,
                Enabled = false
            };
            StartTimer.Elapsed += new ElapsedEventHandler(StartGame);
        }
        private void StartGame(object sender, ElapsedEventArgs e)
        {
            Start();
        }

        /// <summary>
        /// 选择阶段转入加载阶段
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectToLoading(object sender, ElapsedEventArgs e)
        {
            LockSelect();
        }

        /// <summary>
        /// 下发开始协议，进入确认面板
        /// </summary>
        public void GoToConfirmStage()
        {
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.SetProtocol(Fursion_Protocol.StartGame);
            RoomInitInfo roomInitInfo = new RoomInitInfo();
            foreach (Player player in MemberList.Values)
            {
                RoomPlayer roomPlayer = player.TempData.PlayerGameInfo;
                roomInitInfo.RoomPlayers.Add(roomPlayer);
            }
            bytes.AddData(JsonConvert.SerializeObject(roomInitInfo));
            Console.WriteLine(JsonConvert.SerializeObject(roomInitInfo));
            foreach (Player player in MemberList.Values)
            {
                if (player != null)
                    player.Send(bytes);
            }
            RightTimer.Enabled = true;
        }
        private void RightMathod(object sender, ElapsedEventArgs e)
        {
            End();
        }
        /// <summary>
        /// 广播加载进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendLoading(object sender, ElapsedEventArgs e)//广播进度并判断
        {
            bool Temp = true;
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.SetProtocol(Fursion_Protocol.Loading);
            RoomInitInfo roomplayerInfo = new RoomInitInfo();
            foreach (Player player in MemberList.Values)
            {
                RoomPlayer roomPlayer = player.TempData.PlayerGameInfo;
                roomplayerInfo.RoomPlayers.Add(roomPlayer);
            }
            bytes.AddData(JsonConvert.SerializeObject(roomplayerInfo));
            foreach (Player player in MemberList.Values)
            {
                if (player != null && player.UDPClient != null)
                    UDP.instance.SocketSend(bytes, player.UDPClient);
            }
            foreach (Player player in MemberList.Values)
            {
                Temp &= player.TempData.PlayerGameInfo.LoadingProgressBool;
            }
            if (Temp)
            {
                ToStart();
            }
        }
        private void ToStart()
        {
            ProtocolBytes StartProtocol = new ProtocolBytes();
            StartProtocol.SetProtocol(Fursion_Protocol.LockStartGame);
            foreach (Player player in MemberList.Values)
            {
                UDP.instance.SocketSend(StartProtocol, player.UDPClient);
            }
            LoadTimer.Dispose();
            MaxWaitLoadingTimer.Dispose();
            StartTimer.Enabled = true;
        }
        /// <summary>
        /// 超过加载时间开始游戏
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void LoadTimeExceededStart(object o, ElapsedEventArgs e)//超时开始
        {
            LSM = new LockStepMGR
            {
                UDP_ClientList = UDP_ClientList
            };
            LoadTimer.Dispose();
            LSM.Start();
        }
        /// <summary>
        /// 加载完成，正式开始
        /// </summary>
        public void Start()
        {
            StartTimer.Dispose();
            LSM = new LockStepMGR
            {
                UDP_ClientList = UDP_ClientList
            };
            //满足开始条件后广播给客户端正式开始信息
            LSM.Start();
            MaxWaitLoadingTimer.Dispose();
        }
        /// <summary>
        /// 客户端加载进度处理函数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vs"></param>
        public void LoadingMethod(Player player, object[] vs)
        {
            player.TempData.PlayerGameInfo.LoadingProgress = (float)vs[1];
        }
        /// <summary>
        /// 客户端确认进入游戏处理函数
        /// </summary>
        /// <param name="player"></param>
        public void RightMethod(Player player)//游戏确认
        {
            player.TempData.PlayerGameInfo.Right = true;
            RoomInitInfo roomplayerInfo = new RoomInitInfo();
            ProtocolBytes bytes = new ProtocolBytes();
            bytes.SetProtocol(Fursion_Protocol.RightUpDate);
            bool Temp = true;
            foreach (Player player1 in MemberList.Values)
            {
                roomplayerInfo.RoomPlayers.Add(player1.TempData.PlayerGameInfo);
                Temp &= player1.TempData.PlayerGameInfo.Right;
            }
            bytes.AddData(JsonConvert.SerializeObject(roomplayerInfo));
            foreach (Player player1 in MemberList.Values)
            {
                UDP.instance.SocketSend(bytes, player1.UDPClient);
            }
            if (Temp)
            {
                ToSelectStage();
            }
        }
        private void ToSelectStage()
        {
            RightTimer.Dispose();
            ProtocolBytes StartBytes = new ProtocolBytes();
            StartBytes.SetProtocol(Fursion_Protocol.GoToSelect);
            foreach (Player player1 in MemberList.Values)
            {
                UDP.instance.SocketSend(StartBytes, player1.UDPClient);
            }
            SelectTime.Enabled = true;
        }
        /// <summary>
        /// 客户端模型选择锁定函数，并判断是否全部锁定
        /// </summary>
        /// <param name="select"></param>
        public void LockSelect()
        {
            RoomInitInfo roomplayerInfo = new RoomInitInfo();
            bool AllLockSelect = true;
            foreach (Player player in MemberList.Values)
            {
                roomplayerInfo.RoomPlayers.Add(player.TempData.PlayerGameInfo);
                AllLockSelect &= player.TempData.PlayerGameInfo.LockSelect;
            }
            if (AllLockSelect)
            {
                ToLoadingStage(roomplayerInfo);
            }
        }
        /// <summary>
        /// 进入加载阶段
        /// </summary>
        /// <param name="roomInitInfo"></param>
        public void ToLoadingStage(RoomInitInfo roomInitInfo)
        {
            SelectTime.Dispose();
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.SetProtocol(Fursion_Protocol.Loading);
            protocol.AddData(JsonConvert.SerializeObject(roomInitInfo));
            foreach (Player player1 in MemberList.Values)
            {
                UDP.instance.SocketSend(protocol, player1.UDPClient);
            }
            LoadTimer.Enabled = true;
            MaxWaitLoadingTimer.Enabled = true;
        }

        public void End()
        {
            for (int i = 0; i < MemberList.Values.ToList().Count; i++)
            {
                MemberList.Values.ToList()[i].room = null;
            }
        }
    }
}
