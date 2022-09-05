using System;
using SimpleJSON;

namespace ExpandedWeaponSpawns
{
    public class UnusedWeaponInfo
    {
        public UnusedWeaponInfo(byte index, int rarity, string name)
        {
            Index = index;
            Rarity = DefaultRarity = rarity;
            Name = name;
        }

        // Immutable properties
        public byte Index { get; }
        public string Name { get; }
        public int DefaultRarity { get; }

        // Mutable properties
        public int Rarity { get; set; }
        public bool IsActive { get; set; }

        public JSONObject ToJson()
        {
            JSONObject json = new();
            json.Add("Index", (int)Index);
            json.Add("Rarity", Rarity);
            json.Add("IsActive", IsActive);

            return json;
        }
    }
}
    
