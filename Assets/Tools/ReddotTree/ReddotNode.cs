using System;
using System.Collections.Generic;

namespace Tools.ReddotTree
{
    /// <summary>
    /// 目前暂定为有数值则显示红点，没有则隐藏
    /// </summary>
    public class ReddotNode
    {
        private int m_reddotCount;
        private bool m_showCount;
        private Action<int> reddotAction;
        
        public int ReddotCount { get { return m_reddotCount; } }
        public bool ShowCount { get { return m_showCount; } }
        public string Key { get; private set; }

        /// <summary>
        /// 同时标注child节点,parent节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="key"></param>
        public ReddotNode(string key, bool showCount, int primeNum)
        {
            this.Key = key;
            this.m_showCount = showCount;
            this.m_reddotCount = primeNum;
        }
        
        public void RegisterReddot(Action<int> fun)
        {
            reddotAction += fun;
        }

        /// <summary>
        /// 返回实际操作数量
        /// </summary>
        /// <param name="count"></param>
        public int AddReddotCount(int count = int.MinValue)
        {
            int ans = 0;
            
            if (m_reddotCount < -count)
            {
                ans = m_reddotCount;
                m_reddotCount = 0;
            }
            else
            {
                ans = count;
                m_reddotCount += count;
            }

            reddotAction?.Invoke(m_reddotCount);
            return ans;
        }
        
        public void UnRegisterReddot(Action<int> fun)
        {
            if (reddotAction != null)
            {
                reddotAction -= fun; //可能这个事件就注册过
            }
        }

        /// <summary>
        /// 可能用来告知当前堆积事件数量？
        /// </summary>
        private void ReddotAction()
        {
            if (reddotAction != null)
            {
                reddotAction?.Invoke(m_reddotCount);
            }
        }
    }
}