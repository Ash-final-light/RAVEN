namespace Tools.CustomObject.Data
{
    public enum ObjectType
    {
        Weapon,
        Gear,
        Item,
        Material,
    }

    public enum PackageQuantityRestriction
    {
        OnlyOneGroup,
        NoRestriction,
    }

    public abstract class ObjectData
    {
        public int Id; // 物品Id，同一种物品预设Id相同
        public string Name;
        public string Introduction;
        public int Price;
        public string ThumbnailAtlas;
        public string ThumbnailName;
        public PackageQuantityRestriction QuantitativeRestriction;
        public int GroupMaximum;
        
    }
}