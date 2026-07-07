using System;
using System.Collections.Generic;
using Tools.Damage;
using UnityEngine;
using XLua;

namespace Tools.Buff.Data
{
    /// <summary>
    /// 创建和层数改变的时候调用
    /// </summary>
    [CSharpCallLua]
    public delegate void BuffOnOccur(BuffModel buff);
    /// <summary>
    /// 移除buff时调用
    /// </summary>
    [CSharpCallLua]
    public delegate void BuffRemoveOccur(BuffModel buff);
    /// <summary>
    /// 时间更新的时候调用
    /// </summary>
    [CSharpCallLua]
    public delegate bool BuffOnTick(BuffModel buff,float deltaTime);
    //public delegate TimelineObj BuffOnCast(BuffModel buff, SkillObj skill, TimelineObj skillEffect);
    [CSharpCallLua]
    public delegate void BuffOnHit(BuffModel buff, ref DamageInfo damageInfo, string targetId);
    [CSharpCallLua]
    public delegate void BuffOnBeHurt(BuffModel buff, ref DamageInfo damageInfo, string attackerId);
    [CSharpCallLua]
    public delegate void BuffOnKill(BuffModel buff, DamageInfo damageInfo, string targetId);
    //TIPS:这里新增了OnDie事件，用于在目标死亡时调用，还未测试是否生效
    [CSharpCallLua]
    public delegate void BuffOnDie(BuffModel buff, DamageInfo damageInfo);
    /// <summary>
    /// 检查是否可以移除
    /// </summary>
    [CSharpCallLua]
    public delegate bool CanRemoveBuff(BuffModel buff,float deltaTime,bool force);
    public enum AddDurationType
    {
        Add,
        Replace
    }
    public enum RemoveStrategy
    {
        Remove,
        TriggerThenRemove,
    }
    /// <summary>
    ///  buff模型信息(二进制的转换）
    /// </summary>
    public class BuffModelInfo
    {
        public int Id;
        public string Name;
        public string Description;
        public string Tags;
        public string Thumbnail;
        public int Priority;
        public int MaxStack;
        public float Duration;
        public float TriggerInterval;
        public string BuffAddDurationType;
        public string BuffRemoveStrategy;
    }

    /// <summary>
    /// 运行时存储的buff模型数据（配合lua使用）
    /// </summary>
    [System.Serializable]
    public class BuffModel : IDisposable
    {
        public int Id;
        public string Name;
        public string Description;
        public List<string> Tags;
        public string Thumbnail;
        public int Priority;
        
        public int MaxStack;
        public int CurrentStack;
        
        public float Duration;
        public float DurationTimer;
        public float TriggerInterval;
        public float TriggerTimer;
        public AddDurationType BuffAddDurationType;
        public RemoveStrategy BuffRemoveStrategy;
        /// <summary>
        /// Lua callbacks can set this to true after changing stack or internal state.
        /// BuffManager will remove the buff through the normal remove pipeline.
        /// </summary>
        public bool RequestRemove;
        
        // 绑定委托
        public BuffOnOccur OnOccur;
        public BuffRemoveOccur OnRemoveOccur;
        
        public BuffOnTick OnTick;
        //public BuffOnCast OnCast;
        public BuffOnHit OnHit;
        
        public BuffOnBeHurt OnBeHurt;
        public BuffOnKill OnKill;
        
        public CanRemoveBuff OnCanRemoveBuff;
        //TIPS:这里新增了OnDie事件，用于在目标死亡时调用，还未测试是否生效
        public BuffOnDie OnDie;
        
        // 施加者和目标对应id
        public string AttackerId;
        public string TargetId;
        
        
        public LuaTable LuaInstance; 

        // 提供一个绑定方法，自动映射委托
        public void BindLua(LuaTable table)
        {
            this.LuaInstance = table;
            // xLua 的 Get 能够自动将 table 内的 function 包装成标记了 [CSharpCallLua] 的委托
            OnOccur = table.Get<BuffOnOccur>("BuffOnOccur");
            OnRemoveOccur = table.Get<BuffRemoveOccur>("BuffRemoveOccur");
            OnTick = table.Get<BuffOnTick>("BuffOnTick");
            OnHit = table.Get<BuffOnHit>("BuffOnHit");
            OnBeHurt = table.Get<BuffOnBeHurt>("BuffOnBeHurt");
            OnKill = table.Get<BuffOnKill>("BuffOnKill");
            OnCanRemoveBuff = table.Get<CanRemoveBuff>("BuffCanRemove");
            // OnDie is optional. If XLua generated code does not include BuffOnDie yet, keep other callbacks usable.
            try
            {
                OnDie = table.Get<BuffOnDie>("BuffOnDie");
            }
            catch (InvalidCastException e)
            {
                Debug.LogWarning($"BuffOnDie bind skipped: {e.Message}");
                OnDie = null;
            }
        }
        public void Dispose()
        {
            // 1. 断开委托引用，防止闭包导致 C# 对象无法释放
            OnOccur = null;
            OnRemoveOccur = null;
            OnTick = null;
            OnHit = null;
            OnBeHurt = null;
            OnKill = null;
            OnCanRemoveBuff = null;
            OnDie = null;
            RequestRemove = false;
            // 2. 释放 Lua Table 引用
            if (LuaInstance != null)
            {
                LuaInstance.Dispose();
                LuaInstance = null;
            }
        }
    }
}
