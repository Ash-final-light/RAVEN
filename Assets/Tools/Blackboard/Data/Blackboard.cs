using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.Blackboard.Data
{
    [CreateAssetMenu(fileName = "Blackboard", menuName = "Action/Blackboard")]
    public class Blackboard:ScriptableObject
    {
        public List<BlackboardValue> sourceDatas = new List<BlackboardValue>();
        public Blackboard Clone()
        {
            var clone = ScriptableObject.CreateInstance<Blackboard>();
            clone.sourceDatas = new List<BlackboardValue>(sourceDatas.Select(x => x.Clone()));
            return clone;
        }
        public bool ContainsKey(string key)
        {
            return sourceDatas.Exists(x => x.key == key);
        }
        public int GetIntParameter(string key)
        {
            return sourceDatas.Find(x => x.key == key).intValue;
        }
        public float GetFloatParameter(string key)
        {
            return sourceDatas.Find(x => x.key == key).floatValue;
        }
        public bool GetBoolParameter(string key)
        {
            return sourceDatas.Find(x => x.key == key).boolValue;
        }
        public string GetStringParameter(string key)
        {
            return sourceDatas.Find(x => x.key == key).stringValue;
        }
        public BlackboardValue GetParameter(string key)
        {
            return sourceDatas.Find(x => x.key == key);
        }

        /// <summary>
        /// 忽略数据类型，直接设置参数值
        /// </summary>
        /// <param name="value"></param>
        public void SetParameter(BlackboardValue value)
        {
            var data = sourceDatas.Find(x => x.key == value.key);
            if (data != null)
            {
                data.intValue = value.intValue;
                data.floatValue = value.floatValue;
                data.boolValue = value.boolValue;
                data.stringValue = value.stringValue;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning($"Blackboard:SetParameter:Key {value.key} not found");
            }
        }
        public void SetIntParameter(string key, int value)
        {
            var data = sourceDatas.Find(x => x.key == key);
            if (data != null)
            {
                data.intValue = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning($"Blackboard:SetIntParameter:Key {key} not found");
            }
        }
        public void SetFloatParameter(string key, float value)
        {
            var data = sourceDatas.Find(x => x.key == key);
            if (data != null)
            {
                data.floatValue = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning($"Blackboard:SetFloatParameter:Key {key} not found");
            }
        }
        public void SetBoolParameter(string key, bool value)
        {
            var data = sourceDatas.Find(x => x.key == key);
            if (data != null)
            {
                data.boolValue = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning($"Blackboard:SetBoolParameter:Key {key} not found");   
            }
        }
        public void SetStringParameter(string key, string value)
        {
            var data = sourceDatas.Find(x => x.key == key);
            if (data != null)
            {
                data.stringValue = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning($"Blackboard:SetStringParameter:Key {key} not found");
            }
        }
        
        
        
        #if UNITY_EDITOR
        public void AddParameter(BlackboardValue value)
        {
            sourceDatas.Add(value);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        public void RemoveParameter(string key)
        {
            sourceDatas.RemoveAll(x => x.key == key);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        #endif
    }
}