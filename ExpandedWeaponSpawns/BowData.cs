using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExpandedWeaponSpawns
{
    class BowData : MonoBehaviour
    {
        public int PlayerID { get; private set; }
        public Weapon? WeaponObj { get; private set; }
        public float ShootCharge;

        void Start()
        {
            PlayerID = transform.root.gameObject.GetComponentInChildren<NetworkPlayer>().NetworkSpawnID;
            WeaponObj = gameObject.GetComponentInParent<Weapon>();
        }
    }
}
