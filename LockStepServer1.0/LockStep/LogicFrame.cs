using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.LockStep
{
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
        public Move m_Move = new Move();
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
}
