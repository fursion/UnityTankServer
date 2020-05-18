using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
public enum Direction
{
    Up, Down, Left, Right, Core
}
public enum ModeNumber
{
    None, Mode1, Mode2,
}

namespace LockStepServer1._0.LockStep
{
    [Serializable]
    public class SelectModel
    {
        public string Openid { get; set; }
        public ModeNumber SelectedModel { get; set; }
    }

    [Serializable]
    class LogicFrame
    {
        public int LogicFrameID;
        public List<InstructFrame> instructFrames = new List<InstructFrame>();
    }

    [Serializable]
    public enum MoveDirection
    {
        Front,
        Behind,
        Left,
        Right
    }

    [Serializable]
    class InstructFrame
    {
        public string Openid;
        //public TSVector2 DirectionRockerPosition;//摇杆位置
        public Synchro Synchro = new Synchro();
        public Move m_Move { get; set; }
        public Skill m_Skill = new Skill();
        public Equip m_Equip = new Equip();
    }

    [Serializable]
    /// <summary>
    /// 状态
    /// </summary>
    public class Transfrom
    {
        //public TSVector Logic_Position;
    }

    [Serializable]
    /// <summary>
    /// 移动
    /// </summary>
    public class Move
    {
        public MoveDirection m_MoveDirection;
    }

    [Serializable]
    /// <summary>
    /// 技能
    /// </summary>
    public class Skill
    {

    }

    [Serializable]
    /// <summary>
    /// 装备
    /// </summary>
    public class Equip
    {
        public List<Equip> NowEquip = new List<Equip>();
        public List<Equip> SellList = new List<Equip>();
        public List<Equip> PurchaseList = new List<Equip>();
    }

    [Serializable]
    public enum EquipList
    {
        xiaodao,
        pojun,
    }

    /// <summary>
    /// 摇杆信息类
    /// </summary>
    [Serializable]
    public class Synchro
    {
        public Vector2Ser TouchPosition = new Vector2Ser();
        public Direction TouchDirection { get; set; }
    }
    [Serializable]
    public struct Vector2Ser
    {
        public float x;
        public float y;
        public void Fill(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }
        public Vector2 GetVector2
        {
            get
            {
                return new Vector2(x, y);
            }
        }
    }
    public struct LogicTransfrom
    {
        public Vector3 LogicPosition { get; set; }
        public Quaternion LogicRoation { get; set; }
        public Vector3 Logicforward
        {
            get
            {
                return LogicRoation * Vector3.forward;
            }
        }
        public Vector3 LogicEulerAngle { get { return LogicRoation.eulerAngles; } set { LogicRoation = Quaternion.Euler(value); } }
    }
}
