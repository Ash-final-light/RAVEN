using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Sirenix.OdinInspector;
using Tools.Buff.Data;
using Tools.Damage;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using XLua;

namespace Tools.Buff
{
    [CSharpCallLua]
    public delegate LuaTable GetBuffDelegate(int buffId);
    [Serializable]
    public class BuffManager:IStartable,IDisposable
    {
        public bool IsDisposing = false;
        /// <summary>
        /// 管理场景内存在的所有buff
        /// </summary>
        [ShowInInspector]private Dictionary<string, List<BuffModel>> _buffDict = new Dictionary<string, List<BuffModel>>();
        private List<BuffModel> _buffRemoveList = new List<BuffModel>();
        private HashSet<BuffModel> _buffRemoveSet = new HashSet<BuffModel>();

        private GetBuffDelegate getBuffDelegate;
        
        /// <summary>
        /// 获取全局可以出现的物品信息字典
        /// </summary>
        private BuffModelInfoContainer BuffModelInfoContainer=>
            GameApplication.Instance.ExcelBinaryManager.GetContainer<BuffModelInfoContainer>();
        
        
        /// <summary>
        /// 绑定lua中的方法
        /// </summary>
        [Inject] private LuaManager luaManager;

         void IStartable.Start()
        {
            Debug.Log("BuffManager 初始化");
            Init();
        }

        public void Dispose()
        {
            IsDisposing = true;
            foreach (var key in _buffDict.Keys)
            {
                foreach (var buff in _buffDict[key])
                {
                    buff.Dispose();
                }
                _buffDict[key].Clear();
            }
            _buffDict.Clear();
            getBuffDelegate = null;
        }

        public void Init()
        {
            //初始化buff管理器
            getBuffDelegate = luaManager.Global.GetInPath<GetBuffDelegate>("BuffManager.GetBuffInstance");
        }

        /// <summary>
        /// 获得角色身上所有的buff信息
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="buffModel"></param>
        /// <returns></returns>
        public bool TryGetBuff_All(string targetId, out List<BuffModel> buffModel)
        {
            if(_buffDict.TryGetValue(targetId,out buffModel))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获得角色身上指定Tag的buff信息
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="tag"></param>
        /// <param name="buffModel"></param>
        /// <returns></returns>
        public bool TryGetBuff_Tag(string targetId, string tag, out BuffModel buffModel)
        {
            if(_buffDict.TryGetValue(targetId,out List<BuffModel> buffList))
            {
                buffModel = buffList.FirstOrDefault(buff => buff.Tags.Contains(tag));
                return buffModel != null;
            }
            buffModel = null;
            return false;
        }
        
        [Button("触发击中buff")]
        public void OnHitTriggerBuff(ref DamageInfo damageInfo)
        {
            string attackerId = damageInfo.AttackerId;
    
            if (_buffDict.TryGetValue(attackerId, out var buffs))
            {
                // 注意：这里遍历的是攻击者的 Buff
                foreach (var buff in buffs)
                {
                    buff.OnHit?.Invoke(buff, ref damageInfo, damageInfo.TargetId);
                    MarkRemoveIfRequested(buff);
                }
            }
        }
        [Button("触发被伤害buff")]
        public void OnBeHurtTriggerBuff(ref DamageInfo damageInfo)
        {
            // 被打时，应该触发【受击者】身上的效果（如：反伤、减伤、护盾）
            string targetId = damageInfo.TargetId;
    
            if (_buffDict.TryGetValue(targetId, out var buffs))
            {
                // 这里遍历的是受击者的 Buff
                foreach (var buff in buffs)
                {
                    buff.OnBeHurt?.Invoke(buff, ref damageInfo, damageInfo.AttackerId);
                    MarkRemoveIfRequested(buff);
                }
            }
        }
        [Button("触发击杀buff")]
        public void OnKillTriggerBuff(ref DamageInfo damageInfo)
        {
            // 击杀时，应该触发【击杀者】身上的效果（如：击杀奖励）
            string attackerId = damageInfo.AttackerId;
    
            if (_buffDict.TryGetValue(attackerId, out var buffs))
            {
                // 这里遍历的是击杀者的 Buff
                foreach (var buff in buffs)
                {
                    buff.OnKill?.Invoke(buff, damageInfo, damageInfo.TargetId);
                    MarkRemoveIfRequested(buff);
                }
            }
        }
        [Button("触发死亡buff")]
        public void OnDieTriggerBuff(ref DamageInfo damageInfo)
        {
            // 死亡时，应该触发【死亡者】身上的效果（如：死亡爆炸）
            string targetId = damageInfo.TargetId;
    
            if (_buffDict.TryGetValue(targetId, out var buffs))
            {
                // 这里遍历的是死亡者的 Buff
                foreach (var buff in buffs)
                {
                    buff.OnDie?.Invoke(buff, damageInfo);
                    MarkRemoveIfRequested(buff);
                }
            }
        }
        /// <summary>
        /// 更新所有buff的时间
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateBuff(float deltaTime)
        {
            if (IsDisposing)
            {
                return;
            }

            // Use snapshots so Lua callbacks can add/remove buffs without breaking iteration.
            foreach (var buffList in _buffDict.Values.ToArray())
            {
                foreach (var buff in buffList.ToArray())
                {
                    if (buff == null || _buffRemoveSet.Contains(buff))
                    {
                        continue;
                    }

                    if (buff.Duration > 0)
                    {
                        buff.DurationTimer -= deltaTime;
                        if (buff.DurationTimer <= 0)
                        {
                            MarkRemove(buff);
                            continue;
                        }
                    }

                    if (buff.TriggerInterval > 0)
                    {
                        buff.TriggerTimer -= deltaTime;
                        if (buff.TriggerTimer <= 0)
                        {
                            buff.TriggerTimer += buff.TriggerInterval;
                            buff.OnTick?.Invoke(buff, deltaTime);
                            if (MarkRemoveIfRequested(buff))
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        buff.OnTick?.Invoke(buff, deltaTime);
                        if (MarkRemoveIfRequested(buff))
                        {
                            continue;
                        }
                    }

                    if(buff.OnCanRemoveBuff?.Invoke(buff,deltaTime,false) == true)
                    {
                        MarkRemove(buff);
                    }
                }
            }
            RemoveMarkedBuff();
        }

        [Button("创建buff")]
        public bool CreateBuff(string attackerId, string targetId, int buffId,int stack)
        {
            if (getBuffDelegate == null)
            {
                Debug.LogError("getBuffDelegate 为空");
                return false;
            }

            if(!BuffModelInfoContainer.Data.TryGetValue(buffId, out BuffModelInfo buffModelInfo))
            {
                Debug.LogError($"buffId {buffId} 不存在");
                return false;
            }
            
            
            // 查看buff是否已经存在
            if(_buffDict.TryGetValue(targetId, out var value))
            {
                var findBuff = value.Find(buff => buff.Id == buffId);
                //说明此时为相同buff
                if(findBuff != null && findBuff.AttackerId == attackerId)
                {
                    ApplyStack(findBuff, stack);
                    ApplyDuration(findBuff, buffModelInfo.Duration, buffModelInfo.BuffAddDurationType);
                    findBuff.OnOccur?.Invoke(findBuff);
                    return true;
                }
            }
            
            //不同buff，创建新的buff模型
            BuffModel buffModel = buffModelInfo.ToBuffModel(attackerId, targetId);
            buffModel.CurrentStack = Mathf.Clamp(stack <= 0 ? 1 : stack, 1, Mathf.Max(1, buffModel.MaxStack));
            buffModel.DurationTimer = buffModel.Duration;
            buffModel.TriggerTimer = buffModel.TriggerInterval;
    
            // 1. 加载并获取 Lua 返回的 table
            LuaTable luaTable = getBuffDelegate(buffId);

            if (luaTable != null)
            {
                // 2. 将 Lua 表映射到 BuffModel 的委托上
                buffModel.BindLua(luaTable);
            }
            else
            {
                Debug.LogWarning($"Buff {buffId} 的 Lua 脚本未返回有效的 Table 对象");
            }
            
            BindBuff(targetId);
            buffModel.OnOccur?.Invoke(buffModel);
            return true;

            void BindBuff(string id)
            {
                if (_buffDict.ContainsKey(id))
                {
                    _buffDict[id].Add(buffModel);
                }
                else
                {
                    _buffDict.Add(id, new List<BuffModel>() { buffModel });
                }
            }
        }

        private void ApplyStack(BuffModel buff, int stack)
        {
            if (buff == null)
            {
                return;
            }

            int addStack = stack <= 0 ? 1 : stack;
            int maxStack = Mathf.Max(1, buff.MaxStack);
            buff.CurrentStack = Mathf.Clamp(buff.CurrentStack + addStack, 1, maxStack);
        }

        private void ApplyDuration(BuffModel buff, float duration, string addDurationType)
        {
            if (buff == null || duration <= 0)
            {
                return;
            }

            if (!Enum.TryParse(addDurationType, out AddDurationType durationType))
            {
                durationType = AddDurationType.Replace;
            }

            switch (durationType)
            {
                case AddDurationType.Add:
                    buff.DurationTimer = buff.DurationTimer > 0 ? buff.DurationTimer + duration : duration;
                    break;
                case AddDurationType.Replace:
                    buff.DurationTimer = duration;
                    break;
            }
        }

        private void MarkRemove(BuffModel buff)
        {
            if (buff == null || _buffRemoveSet.Contains(buff))
            {
                return;
            }

            buff.RequestRemove = false;
            _buffRemoveSet.Add(buff);
            _buffRemoveList.Add(buff);
        }

        private bool MarkRemoveIfRequested(BuffModel buff)
        {
            if (buff == null || !buff.RequestRemove)
            {
                return false;
            }

            MarkRemove(buff);
            return true;
        }

        private void RemoveMarkedBuff()
        {
            foreach (var buff in _buffRemoveList)
            {
                if (buff.BuffRemoveStrategy == RemoveStrategy.TriggerThenRemove)
                {
                    buff.OnRemoveOccur?.Invoke(buff);
                }

                buff.Dispose();
                if (!string.IsNullOrEmpty(buff.TargetId) && _buffDict.TryGetValue(buff.TargetId, out var buffs))
                {
                    buffs.Remove(buff);
                    if (buffs.Count == 0)
                    {
                        _buffDict.Remove(buff.TargetId);
                    }
                }
            }
            _buffRemoveList.Clear();
            _buffRemoveSet.Clear();
        }
    }
}
