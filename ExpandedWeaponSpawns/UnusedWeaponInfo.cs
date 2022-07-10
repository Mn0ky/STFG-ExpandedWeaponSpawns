using System;
using SimpleJSON;

namespace ExpandedWeaponSpawns
{
    public class UnusedWeaponInfo
    {
        public UnusedWeaponInfo(byte index, int rarity, string name)
        {
            _index = index;
            Rarity = _defaultRarity = rarity;
            _name = name;
        }

        private readonly byte _index;
        private readonly int _defaultRarity;
        private readonly string _name;

        // Immutable properties
        public byte Index { get => _index; }
        public string Name { get => _name; }
        public int DefaultRarity { get => _defaultRarity; }

        // Mutable properties
        public int Rarity { get; set; }
        public bool IsActive { get; set; } = false;

        public JSONObject ToJSON()
        {
            JSONObject json = new();
            json.Add("Index", (int)Index);
            json.Add("Rarity", Rarity);
            json.Add("IsActive", IsActive);

            return json;
        }
    }
}
    
