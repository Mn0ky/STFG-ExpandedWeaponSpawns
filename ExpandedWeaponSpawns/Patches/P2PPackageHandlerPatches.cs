using System;
using System.IO;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace ExpandedWeaponSpawns.Patches
{
    class P2PPackageHandlerPatches
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var readMessageBufferMethod = AccessTools.Method(typeof(P2PPackageHandler), "ReadMessageBuffer");
            var readMessageBufferMethodPrefix =
	            new HarmonyMethod(typeof(P2PPackageHandlerPatches).GetMethod(nameof(ReadMessageBufferMethodPrefix)));  
            harmonyInstance.Patch(readMessageBufferMethod, prefix: readMessageBufferMethodPrefix);
        }

        public static bool ReadMessageBufferMethodPrefix(ref byte[] rawData, ref CSteamID SteamIdRemote)
        {
			using (var memoryStream = new MemoryStream(rawData))
			{
				using (var binaryReader = new BinaryReader(memoryStream))
				{
					uint lastTimeStamp = MultiplayerManager.LastTimeStamp;
					uint num = binaryReader.ReadUInt32();
					var msgType = (NetworkHelper.MsgTypeExtended) binaryReader.ReadByte();

					if (!GameManager.Instance.mMultiplayerManager.HasBeenInitializedFromServer &&
					    msgType != NetworkHelper.MsgTypeExtended.ClientInit &&
					    msgType != NetworkHelper.MsgTypeExtended.ClientAccepted)
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

        private static bool CheckMessageType(byte[] data, NetworkHelper.MsgTypeExtended type, CSteamID steamIdRemote)
        {
	        switch (type)
	        {
		        case NetworkHelper.MsgTypeExtended.WeaponChargeSync:
			        OnWeaponChargeSyncPacketRecieved(data, steamIdRemote);
			        return true;
		        case NetworkHelper.MsgTypeExtended.WeaponShootChargeSync:
			        OnWeaponShootChargeSyncPacketRecieved(data, steamIdRemote);
			        return true;
		        default:
			        return false;
	        }
        }

        private static void OnWeaponChargeSyncPacketRecieved(byte[] data, CSteamID steamIdRemote)
        {
			foreach (var client in GameManager.Instance.mMultiplayerManager.ConnectedClients)
			{
				if (client.ClientID == steamIdRemote)
				{
					float currentCharge = BitConverter.ToSingle(data, 0);
					client.PlayerObject.GetComponentInChildren<Weapon>().currentCharge = currentCharge;
					return;
				}
			}
        }

        private static void OnWeaponShootChargeSyncPacketRecieved(byte[] data, CSteamID steamIdRemote)
		{
			foreach (var client in GameManager.Instance.mMultiplayerManager.ConnectedClients)
			{
				if (client.ClientID == steamIdRemote)
				{
					float num = BitConverter.ToSingle(data, 0);
					var bowInfo = client.PlayerObject.GetComponentInChildren<BowData>();
					Debug.Log("SHOOT charge: " + num);
					bowInfo.ShootCharge = num;
					return;
				}
			}
		}
	}
}
