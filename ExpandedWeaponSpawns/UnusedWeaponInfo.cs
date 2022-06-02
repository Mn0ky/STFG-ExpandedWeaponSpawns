using System;

namespace ExpandedWeaponSpawns
{
    public class UnusedWeaponInfo
    {
        public byte Index { get; set; }
        public int Rarity { get; set; }
        public string Name { get; set; } = "";

        public bool IsActive { get; set; } = false;
    }
}
    
