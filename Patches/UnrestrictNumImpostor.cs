using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;
using TownOfHost;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using Il2CppSystem;
using System.Threading;
using System.Threading.Tasks;

namespace TownOfHost
{//参考:https://github.com/NuclearPowered/Reactor/blob/master/Reactor.Debugger/Patches.cs
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManagerUpdatePatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    public static class GameSettingMenuPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            // Unlocks map/impostor amount changing in online (for testing on your custom servers)
            // オンラインモードで部屋を立て直さなくてもマップを変更できるように変更
            __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    [HarmonyPriority(Priority.First)]
    public static class GameOptionsMenuPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            foreach (var ob in __instance.Children)
            {
                if (ob.Title == StringNames.GameShortTasks ||
                ob.Title == StringNames.GameLongTasks ||
                ob.Title == StringNames.GameCommonTasks)
                {
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 99);
                }
                if (ob.Title == StringNames.GameKillCooldown)
                {
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 180);
                }
                if(ob.Title == StringNames.GameRecommendedSettings) {
                    ob.enabled = false;
                    ob.gameObject.SetActive(false);
                }
            }
        }
    }
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.GetAdjustedNumImpostors))]
    class UnrestrictNumImpostorsPatch
    {
        public static bool Prefix(ref int __result)
        {
            __result = PlayerControl.GameOptions.NumImpostors;
            return false;
        }
    }
    //タイマーとコード隠し
    public class GameStartManagerPatch
    {
        private static float timer = 600f;
        private static string lobbyCodehide = "";
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                // Reset lobby countdown timer
                timer = 600f;
                lobbyCodehide = $"<color={main.modColor}>Town Of Host</color>";
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";
            public static void Prefix(GameStartManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }
            public static void Postfix(GameStartManager __instance)
            {
                // Lobby code
                if (main.HideCodes.Value) lobbyCodehide = $"<color={main.modColor}>Town Of Host</color>";
                else lobbyCodehide = $"{DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId)}";
                __instance.GameRoomName.text = lobbyCodehide;
                // Lobby timer
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return;

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";
                if(timer <= 60) suffix = "<color=#ff0000>" + suffix + "</color>";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;
            }
        }
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
        public static class HiddenTextPatch
        {
            private static void Postfix(TextBoxTMP __instance)
            {
                if(__instance.name == "GameIdText") __instance.outputText.text = new string('*', __instance.text.Length);
            }
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public class GameStartRandomMap
    {
        public static bool Prefix(GameStartRandomMap __instance)
        {
            bool continueStart = true;
            if (main.RandomMapsMode == true)
            {
                var rand = new System.Random();
                System.Collections.Generic.List<byte> RandomMaps = new System.Collections.Generic.List<byte>();
                /*TheSkeld   = 0
                  MIRAHQ     = 1
                  Polus      = 2
                  Dleks      = 3
                  TheAirShip = 4*/
                if (main.AddedTheSkeld == true) RandomMaps.Add(0);
                if (main.AddedMIRAHQ == true) RandomMaps.Add(1);
                if (main.AddedPolus == true) RandomMaps.Add(2);
                if (main.AddedDleks == true) RandomMaps.Add(3);
                if (main.AddedTheAirShip == true) RandomMaps.Add(4);
                var MapsId = RandomMaps[rand.Next(RandomMaps.Count)];
                PlayerControl.GameOptions.MapId = MapsId;
            
            }
            return continueStart;
        }
    } 
}
