using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LockStepServer1._0.LockStep;
using LockStepServer1._0.ROOM;

namespace LockStepServer1._0.Core
{
    class PlayerTempData
    {
        public enum Status
        {
            None,
            Room,
            Fight
        }
        public Status status { get; set; }
        public Room Room { get; set; }
        public RoomPlayer PlayerGameInfo { get; set; }
        public string TeamOpenid { get; set; }
        public string RoomID { get; set; }
        public bool isOwner = false;
        public float Hp;
        public long lastUpdateTime;
        public long lastShootTime;
        public PlayerTempData()
        {
            status = Status.None;
        }
    }
}
