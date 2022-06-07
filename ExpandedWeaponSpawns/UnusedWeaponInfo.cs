using System;

namespace ExpandedWeaponSpawns
{
    public class UnusedWeaponInfo
    {
        public UnusedWeaponInfo(byte index, int rarity, string name)
        {
            Index = index;
            DefaultRarity = rarity;
            Rarity = rarity;
            Name = name;
        }

        public byte Index { get; set; }
        public int Rarity { get; set; }
        public string Name { get; set; } = "";

        public bool IsActive { get; set; } = false;
        public int DefaultRarity { get; private set; }
    }
}
    
