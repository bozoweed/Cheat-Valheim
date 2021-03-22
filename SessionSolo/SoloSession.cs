using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace SoloSession
{
    [BepInPlugin("virtuacode.valheim.SoloSession", "Fix Solo Session", "0.0.1")]

    public class SoloSession : BaseUnityPlugin
    {
        private readonly Harmony harmo = new Harmony("virtuacode.valheim.SoloSession");
        private static int lastCount = 0;
        private void Awake()
        { 
            Logger.LogInfo("Fix Solo Session!");
            harmo.PatchAll();
        }

        public static void Log(string msg)
        {
            Debug.Log(" [" + typeof(SoloSession).Name + "] " + msg);
        }

        public static void LogErr(string msg)
        {
            Debug.LogError(" [" + typeof(SoloSession).Name + "] " + msg);
        }

        public static void checkConnection()
        {
            try
            {
                if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
                {
                    ZNet.instance.Shutdown();


                }

            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }
        [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
        class UseStamina_Patch
        {
            static void Prefix(ref float ___m_stamina)
            {
                checkConnection();
            }
        }
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.OnStopMoving))]
        class OnStopMoving_Patch
        {
            static void PostFix()
            {
                checkConnection();
            }
        }

    }
}
