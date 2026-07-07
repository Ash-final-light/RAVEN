using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Quest;
using UnityEngine;

namespace Daily.Config
{
    public class TextDic:SerializedScriptableObject
    {
        [SerializeField]private Dictionary<DailyMapNodeEventType, string> Data;
        
        private QuestPlaceHolderFormatter<int> _placeHolderFormatter;

        public TextDic()
        {
            _placeHolderFormatter = new QuestPlaceHolderFormatter<int>(
                new Dictionary<string, (string placeHolderMeaning, Func<int, string> placeHolderOutput)>()
            {
                { "TargetValue", ("目标数值", data => data.ToString()) }
            });
        }
        public string GetDescription(DailyMapNode node)
        {
            if(!Data.TryGetValue(node.EventType, out string temp))
            {
                Debug.LogError($"[DescriptionDic]{node.EventType}]'s description not found");
                return string.Empty;
            }
            
            switch (node.EventType)
            {
                case DailyMapNodeEventType.AdjustStep:
                    return _placeHolderFormatter.Format(temp, node.Properties.AdjustStepAmount);
                    break;
                case DailyMapNodeEventType.ExtraAction:
                    return _placeHolderFormatter.Format(temp, node.Properties.ExtraActionCount);
                    break;
                default:
                    return temp;
                    break;
            }
        }
    }
}