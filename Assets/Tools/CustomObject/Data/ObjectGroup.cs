using System;
using Framework.Common.Util;
using Tools.CustomObject.Data;
using Tools.CustomObject.Data.Extension;

namespace Tools.CustomObject.Data
{
    public class ObjectGroup
    {
        public string GroupId; // 组Id，组创建时生成，唯一Id
        public int Number; // 物品数量
        public bool New; // 物品组是否为新
        public long GetTimestamp; // 物品组创建时间
        public ObjectData Data; // 物品静态数据

        public string Type => Data.GetObjectType().ToString();

        public static ObjectGroup CreateNew(ObjectData data, int number)
        {
            return new ObjectGroup
            {
                GroupId = Guid.NewGuid().ToString(),
                Number = number,
                New = true,
                GetTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Data = data,
            };
        }
    }
}