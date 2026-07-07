using System.Collections.Generic;
using UnityEngine;

namespace Tools.ReddotTree
{
    //未来通过VContainer来管理红点树
    public class ReddotTree
    {
        [SerializeField]
        public Dictionary<string, ReddotNode> m_ReddotNodeDic = new();
        
        /// <summary>
        /// 子节点key 到 父节点key的映射
        /// </summary>
        [SerializeField]
        public Dictionary<string,List<string>> m_ChildParentDic = new();


        

        public ReddotNode GetReddotNode(string id)
        {
            if (!m_ReddotNodeDic.ContainsKey(id))
                return null;
            return m_ReddotNodeDic[id];
        }
        
        public void RegisterReddotNode(string id, bool showCount, int primeNum)
        {
            if (m_ReddotNodeDic.ContainsKey(id))
                return;
            ReddotNode reddotNode = new ReddotNode(id,showCount,primeNum);
            m_ReddotNodeDic.Add(id, reddotNode);
            m_ChildParentDic.Add(id, new List<string>());
        }
        
        public void BindChildParent(string childId, string parentId)
        {
            if (!m_ChildParentDic.ContainsKey(childId))
                return;
            m_ChildParentDic[childId].Add(parentId);
        }
        
        /// <summary>
        /// 消除注册和查询方法
        /// </summary>
        /// <param name="id"></param>
        public void RemoveReddotNode(string id)
        {
            if (!m_ReddotNodeDic.ContainsKey(id))
                return;
            m_ReddotNodeDic.Remove(id);
            m_ChildParentDic.Remove(id);
        }

        public void InvokeReddotChangeAction(string id, int addCount)
        {
            if (!m_ReddotNodeDic.ContainsKey(id))
                return;
            int ans = m_ReddotNodeDic[id].AddReddotCount(addCount);//实际变化的数值
            
            // 递归调用父节点
            foreach (var parentId in m_ChildParentDic[id])
            {
                InvokeReddotChangeAction(parentId, ans);
            }
        }
    }
}