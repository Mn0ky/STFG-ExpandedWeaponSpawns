using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace ExpandedWeaponSpawns
{
    class P2PPackageHandlerPatches
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var readMessageBufferMethod = AccessTools.Method(typeof(P2PPackageHandler), "ReadMessageBuffer");
            var readMessageBufferMethodPrefix = new HarmonyMethod(typeof(P2PPackageHandlerPatches).GetMethod(nameof(ReadMessageBufferMethodPrefix)));  
            harmonyInstance.Patch(readMessageBufferMethod, prefix: readMessageBufferMethodPrefix);
        }

        public static bool ReadMessageBufferMethodPrefix(ref byte[] rawData, ref CSteamID SteamIdRemote, P2PPackageHandler __instance)
        {
			using (MemoryStream memoryStream = new MemoryStream(rawData))
			{
				using (BinaryReader binaryReader = new BinaryReader(memoryStream))
				{
					uint lastTimeStamp = MultiplayerManager.LastTimeStamp;
					uint num = binaryReader.ReadUInt32();
					Helper.MsgTypeExtended msgType = (Helper.MsgTypeExtended) binaryReader.ReadByte();

					if (!GameManager.Instance.mMultiplayerManager.HasBeenInitializedFromServer && msgType != Helper.MsgTypeExtended.ClientInit && msgType != Helper.MsgTypeExtended.ClientAccepted)
					{
						Debug.Log("Stopping packet: " + msgType + " Has not been inited by server yet!");
						return false;
					}
					else
					{
						if (num < lastTimeStamp) Debug.LogWarning("Packet is obsolete!");

						byte[] data = binaryReader.ReadBytes(rawData.Length - 1);
						bool isBowPacket = CheckMessageType(data, msgType, SteamIdRemote);

						// Run normal packet checker if packet is not a bow packet, otherwise skip
						return !isBowPacket;
					}
				}
			}
		}

		public static bool CheckMessageType(byte[] data, Helper.MsgTypeExtended type, CSteamID steamIdRemote)
        {
			switch (type)
            {
				case Helper.MsgTypeExtended.WeaponChargeSync:
					OnWeaponChargeSyncPacketRecieved(data, steamIdRemote);
					return true;
				case Helper.MsgTypeExtended.WeaponShootChargeSync:
					OnWeaponShootChargeSyncPacketRecieved(data, steamIdRemote);
					return true;
				default:
					return false;
			}
        }

		public static void OnWeaponChargeSyncPacketRecieved(byte[] data, CSteamID steamIdRemote)
        {
			foreach (ConnectedClientData client in GameManager.Instance.mMultiplayerManager.ConnectedClients)
			{
				if (client.ClientID == steamIdRemote)
				{
					float currentCharge = BitConverter.ToSingle(data, 0);
					client.PlayerObject.GetComponentInChildren<Weapon>().currentCharge = currentCharge;
					return;
				}
			}
        }

		public static void OnWeaponShootChargeSyncPacketRecieved(byte[] data, CSteamID steamIdRemote)
		{
			foreach (ConnectedClientData client in GameManager.Instance.mMultiplayerManager.ConnectedClients)
			{
				if (client.ClientID == steamIdRemote)
				{
					float num = BitConverter.ToSingle(data, 0);
					BowData bowInfo = client.PlayerObject.GetComponentInChildren<BowData>();
					Debug.Log("SHOOT charge: " + num);
					bowInfo.ShootCharge = num;
					return;
				}
			}
		}
	}
}
