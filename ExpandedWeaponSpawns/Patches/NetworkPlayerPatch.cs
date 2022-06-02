using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.IO;
using Steamworks;

namespace ExpandedWeaponSpawns
{
    class NetworkPlayerPatch
    {
        public static void Patch(Harmony harmonyInstance)
        {
            var sendNewClientStatePackageMethod = AccessTools.Method(typeof(NetworkPlayer), "SendNewClientStatePackage");
            var sendNewClientStatePackageMethodPrefix = new HarmonyMethod(typeof(NetworkPlayerPatch).GetMethod(nameof(SendNewClientStatePackageMethodPrefix)));  
            harmonyInstance.Patch(sendNewClientStatePackageMethod, prefix: sendNewClientStatePackageMethodPrefix);
        }

        public static bool SendNewClientStatePackageMethodPrefix(NetworkPlayer __instance)
        {
			Traverse mNetworkPositionPackageObj = Traverse.Create(__instance).Field("mNetworkPositionPackage");
			Traverse mNetworkWeaponPackageObj = Traverse.Create(__instance).Field("mNetworkWeaponPackage");

			//mNetworkPositionPackageObj.SetValue(Helper.createNetworkPositionPackage.Invoke(__instance, null));
			//mNetworkWeaponPackageObj.SetValue(Helper.createNetworkWeaponPackage.Invoke(__instance, null));

			NetworkPlayer.NetworkPositionPackage mNetworkPositionPackage = mNetworkPositionPackageObj.GetValue<NetworkPlayer.NetworkPositionPackage>();
			NetworkPlayer.NetworkWeaponPackage mNetworkWeaponPackage = mNetworkWeaponPackageObj.GetValue<NetworkPlayer.NetworkWeaponPackage>();

			uint serverRealTime = SteamUtils.GetServerRealTime();
			byte[] clientStatePackageArray;

			// Creates a dynamically sizing memorystream rather than based on the size of the byte[], this should prevent any memorystream size exceptions the original method has
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					binaryWriter.Write(mNetworkPositionPackage.Position.X);
					binaryWriter.Write(mNetworkPositionPackage.Position.Y);
					binaryWriter.Write(mNetworkPositionPackage.Rotation.X);
					binaryWriter.Write(mNetworkPositionPackage.Rotation.Y);
					binaryWriter.Write(mNetworkPositionPackage.YValue);
					binaryWriter.Write(mNetworkPositionPackage.MovementType);
					binaryWriter.Write(mNetworkWeaponPackage.FightState);

					ProjectilePackageStruct[] projectilePackages = mNetworkWeaponPackage.ProjectilePackages;
					ushort num = (ushort)projectilePackages.Length;
					binaryWriter.Write(num);

					if (num > 0)
					{
						for (int i = 0; i < num; i++)
						{
							ProjectilePackageStruct projectilePackageStruct = projectilePackages[i];
							binaryWriter.Write(projectilePackageStruct.shootPosition.X);
							binaryWriter.Write(projectilePackageStruct.shootPosition.Y);
							binaryWriter.Write(projectilePackageStruct.shootVector.X);
							binaryWriter.Write(projectilePackageStruct.shootVector.Y);
							binaryWriter.Write(projectilePackageStruct.syncIndex);
							UnityEngine.Debug.Log("Sending: ProjectilePackage: " + projectilePackageStruct.shootPosition.ToString() + " : " + projectilePackageStruct.shootVector.ToString());
						}
					}
					binaryWriter.Write(mNetworkWeaponPackage.WeaponType);
				}
				clientStatePackageArray = memoryStream.ToArray();
			}

			int mUpdateChannel = Traverse.Create(__instance).Field("mUpdateChannel").GetValue<int>();
			ushort mNetworkSpawnID = Traverse.Create(__instance).Field("mNetworkSpawnID").GetValue<ushort>();

			GameManager.Instance.mMultiplayerManager.OnPlayerMoved(clientStatePackageArray, mUpdateChannel, mNetworkSpawnID);
			return false;
		}
    }
}
