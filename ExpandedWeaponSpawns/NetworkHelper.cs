using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace ExpandedWeaponSpawns
{
    class NetworkHelper
    {
		public static void SyncCharge(float curCharge)
		{
            SendMessageToAllClients
			(
				BitConverter.GetBytes(curCharge), 
				MsgTypeExtended.WeaponChargeSync, 
				false, 
				SteamUser.GetSteamID().m_SteamID,
				EP2PSend.k_EP2PSendReliable,
				0
			);
		}

        public static void SyncShootCharge(float shootCharge)
        {
			SendMessageToAllClients
			(
                BitConverter.GetBytes(shootCharge),
                MsgTypeExtended.WeaponShootChargeSync,
                false,
                SteamUser.GetSteamID().m_SteamID,
                EP2PSend.k_EP2PSendReliable,
                0
			);
        }

        private static void SendMessageToAllClients(byte[] data, MsgTypeExtended type, bool ignoreServer = false,
	        ulong ignoreUserID = 0UL, EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable, int channel = 0)
		{
			List<CSteamID> list = new();
			ushort num = 0;
			byte b = 0;

			ConnectedClientData[] connectedClients = GameManager.Instance.mMultiplayerManager.ConnectedClients;
			CSteamID unassginedID = GameManager.Instance.mMultiplayerManager.mUnassignedID;

			while (b < connectedClients.Length)
			{
				CSteamID csteamID = ((connectedClients[b] != null) ? connectedClients[b].ClientID : unassginedID);
				if (csteamID != unassginedID)
				{
					if (!ignoreServer || !(csteamID == MatchmakingHandler.Instance.LobbyOwner))
					{
						if (ignoreUserID == 0UL || csteamID.m_SteamID != ignoreUserID)
						{
							if (!list.Contains(csteamID))
							{
								num += 1;
								SendP2PPacketToUser(csteamID, data, type, sendMethod, channel);
								list.Add(csteamID);
							}
						}
					}
				}
				b += 1;
			}
		}

        private static void SendP2PPacketToUser(CSteamID clientID, byte[] data, MsgTypeExtended messageType, 
	        EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable, int channel = 0)
		{
			byte[] array = WriteMessageBuffer(data, messageType);
			uint num = (uint)array.Length;

			if (!SteamNetworking.SendP2PPacket(clientID, array, num, sendMethod, channel)) 
				UnityEngine.Debug.Log("FAILED to send package to User: " + clientID.m_SteamID);
		}

        private static byte[] WriteMessageBuffer(byte[] data, MsgTypeExtended messageType)
		{
			uint serverRealTime = SteamUtils.GetServerRealTime();
			byte[] array = new byte[data.Length + 4 + 1];

			using (MemoryStream memoryStream = new(array))
			{
				using (BinaryWriter binaryWriter = new(memoryStream))
				{
					binaryWriter.Write(serverRealTime);
					binaryWriter.Write((byte)messageType);
					binaryWriter.Write(data);
				}
			}

			return array;
		}
		
		public static float GetShootCharge(Weapon instance) 
			=> instance.gameObject.GetComponentInChildren<BowData>().ShootCharge;

		public enum MsgTypeExtended : byte
        {
			Ping,
			PingResponse,
			ClientJoined,
			ClientRequestingAccepting,
			ClientAccepted,
			ClientInit,
			ClientRequestingIndex,
			ClientRequestingToSpawn,
			ClientSpawned,
			ClientReadyUp,
			PlayerUpdate,
			PlayerTookDamage,
			PlayerTalked,
			PlayerForceAdded,
			PlayerForceAddedAndBlock,
			PlayerLavaForceAdded,
			PlayerFallOut,
			PlayerWonWithRicochet,
			MapChange,
			WeaponSpawned,
			WeaponThrown,
			RequestingWeaponThrow,
			ClientRequestWeaponDrop,
			WeaponDropped,
			WeaponWasPickedUp,
			ClientRequestingWeaponPickUp,
			ObjectUpdate,
			ObjectSpawned,
			ObjectSimpleDestruction,
			ObjectInvokeDestructionEvent,
			ObjectDestructionCollision,
			GroundWeaponsInit,
			MapInfo,
			MapInfoSync,
			WorkshopMapsLoaded,
			StartMatch,
			ObjectHello,
			OptionsChanged,
			KickPlayer,
			WeaponChargeSync,
			WeaponShootChargeSync
        }
    }
}
