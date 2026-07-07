using System;
using System.Collections.Generic;

namespace Tools.Buff.Data
{
    public static class ExtendMethod
    {
        public static BuffModel ToBuffModel(this BuffModelInfo buffModelInfo,string attackerId,string targetId)
        {
            return new BuffModel
            {
                Id = buffModelInfo.Id,
                Name = buffModelInfo.Name,
                Description = buffModelInfo.Description,
                Tags = ToList(buffModelInfo.Tags),
                Thumbnail = buffModelInfo.Thumbnail,
                Priority = buffModelInfo.Priority,
                MaxStack = buffModelInfo.MaxStack,
                Duration = buffModelInfo.Duration,
                DurationTimer = buffModelInfo.Duration,
                TriggerInterval = buffModelInfo.TriggerInterval,
                TriggerTimer = buffModelInfo.TriggerInterval,
                BuffAddDurationType = Enum.Parse<AddDurationType>(buffModelInfo.BuffAddDurationType),
                BuffRemoveStrategy = Enum.Parse<RemoveStrategy>(buffModelInfo.BuffRemoveStrategy),
                AttackerId = attackerId,
                TargetId = targetId,
            };

            List<string> ToList(string str)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return new List<string>();
                }

                // 按逗号分割字符串，并去除每个元素前后的空白字符
                string[] array = str.Split(',');
                List<string> result = new List<string>();
            
                foreach (string item in array)
                {
                    result.Add(item.Trim()); // 去除前后空白字符
                }
            
                return result;
            }
        }
    }
}
