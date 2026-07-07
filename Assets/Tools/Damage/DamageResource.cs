namespace Tools.Damage
{
    public struct DamageResource
    {
        public int hp;
        public int mp;
        public float @break;

        public static DamageResource Zero = new DamageResource
        {
            hp = 0,
            mp = 0,
            @break = 0,
        };
        public CharacterResource Cost()
        {
            return new CharacterResource
            {
                hp = -hp, // hp花费是扣除
                mp = -mp, // mp花费是扣除
                @break = @break // 破防花费是添加破防量
            };
        }
    }
    public struct CharacterResource
    {
        public int hp; // hp
        public int mp; // mp
        public float @break; // 破防量
        public float atb; // Atb量
    }
}