using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.ROOM
{
    [Serializable]
    public class RoomPlayer
    {
        public string Openid { get; set; }
        public string Nickname { get; set; }
        public string HeadImageUrl { get; set; }
        public bool Right { get; set; }
        public bool LoadingProgressBool { get; set; } = false;
        private float loadingprogress = 0f;
        public float LoadingProgress { get { return loadingprogress; } set { loadingprogress = value; if (value >= 100) LoadingProgressBool = true; } }
        public bool LockSelect { get; set; } = false;
        public ModeNumber SelectModelNumber { get; set; } = ModeNumber.None;
    }
    [Serializable]
    public class RoomInitInfo
    {
        public string RoomID;
        public List<RoomPlayer> RoomPlayers = new List<RoomPlayer>();
    }
}
