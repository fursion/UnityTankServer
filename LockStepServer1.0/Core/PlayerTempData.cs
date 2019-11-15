using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Status status;
        public Room room;
        public int team = 1;
        public bool isOwner = false;
        public float posX;
        public float posY;
        public float posZ;
        public float Hp;
        public long lastUpdateTime;
        public long lastShootTime;
        public PlayerTempData()
        {
            status = Status.None;
        }
    }
}
