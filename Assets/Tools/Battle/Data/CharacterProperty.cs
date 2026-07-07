using System;
using Tools.Damage;
using UnityEngine;

namespace Tools.Battle.Data
{
    /// <summary>
    /// 角色配置基础属性，这是基类，后续如果针对某个属性具有防御力，需要继承这个类
    /// </summary>
    [Serializable]
    public class CharacterInfoData
    {
        public int Id;
        public string Name;
        public int Health;
        public int MaxMp;
        public float CritRate;
        public int Damage;
        public int Defense;
        public int Speed;

        public CharacterInfoData(CharacterInfoData baseInfo)
        {
            Id = baseInfo.Id;
            Name = baseInfo.Name;
            Health = baseInfo.Health;
            MaxMp = baseInfo.MaxMp;
            CritRate = baseInfo.CritRate;
            Damage = baseInfo.Damage;
            Defense = baseInfo.Defense;
            Speed = baseInfo.Speed;
        }
        public CharacterInfoData()
        {
        }
        
    }
    
    /// <summary>
    /// 角色当前属性（运行时）
    /// </summary>
    [Serializable]
    public class CharacterProperty
    {
        public CharacterInfoData Info;
        public string gameObjId;
        
        public int CurrentHealth;
        public int CurrentDamage;
        public float CurrentCritRate;
        public int CurrentDefense;
        public int CurrentSpeed;

        /// <summary>
        /// 血量归零事件
        /// </summary>
        public Action<string> OnCharacterDie;
        /// <summary>
        /// 破防事件
        /// </summary>
        public Action<string> OnCharacterBreak;

        
        //TODO:还没有在Excel表格中进行配置，当前实际流水线中无用
        public DamageValueType weakness; // 角色弱点，用于伤害计算，注意，治疗不受影响
        public DamageValueType immunity; // 角色免疫，用于伤害计算，注意，治疗不受影响
        
        public bool isPlayer; // 角色是否为玩家
        
        public bool dead; // 角色是否死亡
        public bool isActive; // 角色是否处于活跃状态，活跃状态下才会参与优先级排序
        
        //TODO：未来添加其他机制时使用
        public bool broken; // 角色是否破防
        public bool inDefence; // 角色是否处于防御
        public bool inDialogue; // 角色是否处于对话
        
        public Transform targetTransform; // 目标角色的Transform，用于计算距离
        
        //TIPS:例用于大招，不播放受伤动画，有此标记时伤害归零（闪避？）
        public bool immune; // 是否处于无敌，是则攻击不再对该角色判定，无敌状态下自带霸体和不可破防效果
        //TIPS：霸体帧，不播放受伤动画，但参与伤害结算
        public bool endure; // 是否处于霸体，是则无法播放受击动画
        public bool unbreakable; // 是否处于不可破防状态，是则无法减少和增加破防值
        
        /// <summary>
        /// 当前护盾值
        /// </summary>
        public int CurrentShield;
        
        /// <summary>
        /// 当前魔法值
        /// </summary>
        public int CurrentMp;

        /// <summary>
        /// Runtime MP cap. It is initialized from Excel and can be changed by buffs or character switching later.
        /// </summary>
        public int CurrentMaxMp;
        
        /// <summary>
        /// TODO：当前破防值，默认所有角色达到100触发破防效果（在这里体现为眩晕动画？），后续在Excel中修改
        /// </summary>
        public float CurrentBreak;
    }
}
