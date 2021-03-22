using System;
using HarmonyLib;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox
{
	internal static class SkCommandPatcher
	{
		[HarmonyPatch(typeof(Console), "IsCheatsEnabled")]
		public static class PatchIsCheatsEnabled
		{
			public static void Postfix(bool __result)
			{
				__result = BCheat;
			}
		}

		[HarmonyPatch(typeof(WearNTear), "UpdateSupport")]
		private static class PatchUpdateSupport
		{
			private static bool Prefix(ref float ___m_support, ref ZNetView ___m_nview)
			{
				if (BFreeSupport)
				{
					___m_support += ___m_support;
					___m_nview.GetZDO().Set("support", ___m_support);
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
		private static class PatchUpdatePlacementGhost
		{
			private static void Postfix(bool flashGuardStone)
			{
				if (!bBuildAnywhere)
				{
					return;
				}
				try
				{
					if (Player.m_localPlayer != null && Player.m_localPlayer.GetPrivateField<int>("m_placementStatus") != 4)
					{
						Player.m_localPlayer.SetPrivateField("m_placementStatus", 0);
					}
				}
				catch (Exception)
				{
				}
			}
		}

		[HarmonyPatch(typeof(Location), "IsInsideNoBuildLocation")]
		private static class PatchIsInsideNoBuildLocation
		{
			private static void Postfix(ref bool __result)
			{
				if (bBuildAnywhere)
				{
					__result = false;
				}
			}
		}

		private static Harmony harmony;

		private static bool initComplete;

		private static bool bCheat;

		private static bool bFreeSupport;

		public static bool bBuildAnywhere;

		public static Harmony Harmony
		{
			get
			{
				return harmony;
			}
			set
			{
				harmony = value;
			}
		}

		public static bool BCheat
		{
			get
			{
				return bCheat;
			}
			set
			{
				bCheat = value;
			}
		}

		public static bool BFreeSupport
		{
			get
			{
				return bFreeSupport;
			}
			set
			{
				bFreeSupport = value;
			}
		}

		public static bool InitComplete
		{
			get
			{
				return initComplete;
			}
			set
			{
				initComplete = value;
			}
		}

		public static void InitPatch()
		{
			if (InitComplete)
			{
				return;
			}
			try
			{
				harmony = Harmony.CreateAndPatchAll(typeof(SkCommandPatcher).Assembly);
			}
			catch (Exception ex)
			{
				SkCommandProcessor.PrintOut("Something failed, there is a strong possibility another mod blocked this operation.", SkCommandProcessor.LogTo.Console);
				SkUtilities.Logz(new string[2] { "SkCommandPatcher", "PATCH" }, new string[3] { "PATCH => FAILED. CHECK FOR OTHER MODS BLOCKING PATCHES.\n", ex.Message, ex.StackTrace }, LogType.Error);
			}
			finally
			{
				InitComplete = true;
			}
		}
	}
}
