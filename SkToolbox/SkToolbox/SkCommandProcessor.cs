using System;
using System.Timers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkToolbox.Configuration;
using SkToolbox.SkModules;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox
{
	internal static class SkCommandProcessor
	{
		[Flags]
		public enum LogTo
		{
			Console = 0x0,
			Chat = 0x1,
			DebugConsole = 0x2
		}

		internal static class TerrainModification
		{
			private static void CreateTerrain(GameObject prefab, Vector3 position, ZNetView component)
			{
				float levelOffset = prefab.GetComponent<TerrainModifier>().m_levelOffset;
				UnityEngine.Object.Instantiate(prefab, position - Vector3.up * levelOffset, Quaternion.identity).GetComponent<ZNetView>().GetZDO()
					.SetPGWVersion(component.GetZDO().GetPGWVersion());
			}

			public static void ModifyTerrain(int operation, Vector3 centerLocation, GameObject prefab, float radius)
			{
				if (radius > 30f)
				{
					PrintOut("Radius clamped to 30 max!", LogTo.Console);
				}
				radius = Mathf.Clamp(radius, 0f, 30f);
				Vector3 v = centerLocation;
				ZNetView component = Player.m_localPlayer.gameObject.GetComponent<ZNetView>();
				int num = Mathf.CeilToInt(radius / 3f) * 3;
				for (int i = -num; i <= num; i += 3)
				{
					for (int j = -num; j <= num; j += 3)
					{
						Vector3 vector = new Vector3(v.x + (float)i, v.y, v.z + (float)j);
						if (Utils.DistanceXZ(v, vector) <= radius)
						{
							CreateTerrain(prefab, vector, component);
						}
					}
				}
				int num2 = int.MaxValue;
				int num3 = int.MaxValue;
				for (int k = -num; k <= 0; k++)
				{
					for (int l = -num; l <= -num / 2; l++)
					{
						if (!(Utils.DistanceXZ(v1: new Vector3(v.x + (float)k, v.y, v.z + (float)l), v0: v) <= radius))
						{
							continue;
						}
						if (k < l || (l >= num3 && k <= num2 + 2 && k != 0))
						{
							break;
						}
						num2 = k;
						num3 = l;
						if (k / 3 * 3 == k && l / 3 * 3 == l)
						{
							break;
						}
						CreateTerrain(prefab, new Vector3(v.x + (float)k, v.y, v.z + (float)l), component);
						CreateTerrain(prefab, new Vector3(v.x + (float)k, v.y, v.z - (float)l), component);
						if (k != 0)
						{
							CreateTerrain(prefab, new Vector3(v.x - (float)k, v.y, v.z + (float)l), component);
							CreateTerrain(prefab, new Vector3(v.x - (float)k, v.y, v.z - (float)l), component);
						}
						if (k != l)
						{
							CreateTerrain(prefab, new Vector3(v.x + (float)l, v.y, v.z + (float)k), component);
							CreateTerrain(prefab, new Vector3(v.x - (float)l, v.y, v.z + (float)k), component);
							if (k != 0)
							{
								CreateTerrain(prefab, new Vector3(v.x + (float)l, v.y, v.z - (float)k), component);
								CreateTerrain(prefab, new Vector3(v.x - (float)l, v.y, v.z - (float)k), component);
							}
						}
						break;
					}
				}
			}

			public static void ResetTerrain(Vector3 centerLocation, float radius)
			{
				if (radius > 50f)
				{
					PrintOut("Radius clamped to 50 max!", LogTo.Console);
				}
				radius = Mathf.Clamp(radius, 2f, 50f);
				try
				{
					foreach (TerrainModifier allInstance in TerrainModifier.GetAllInstances())
					{
						if (allInstance != null && Utils.DistanceXZ(Player.m_localPlayer.transform.position, allInstance.transform.position) < radius)
						{
							ZNetView component = allInstance.GetComponent<ZNetView>();
							if (component != null && component.IsValid())
							{
								component.ClaimOwnership();
								component.Destroy();
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}
		public static System.Timers.Timer GodItem = new System.Timers.Timer(500);

		public static bool flyEnabled = false;

		public static bool godEnabled = false;

		public static bool farInteract = false;

		public static bool infStamina = false;

		public static bool godItem = false;

		public static bool noCostEnabled = false;

		public static bool bTeleport = false;

		public static bool bDebugTime = false;

		public static bool bDetectEnemies = false;

		public static bool btDetectEnemiesSwitch = true;

		public static int bDetectRange = 20;

		public static bool altOnScreenControls = false;

		public static bool bCoords = false;

		public static int pageSize = 11;

		private static Vector3 chatPos = new Vector3(0f, -99f);

		private static ModConsoleOpt consoleOpt = null;

		private static List<string> weatherList = new List<string>
		{
			"Twilight_Clear", "Clear", "Misty", "Darklands_dark", "Heath clear", "DeepForest Mist", "GDKing", "Rain", "LightRain", "ThunderStorm",
			"Eikthyr", "GoblinKing", "nofogts", "SwampRain", "Bonemass", "Snow", "Twilight_Snow", "Twilight_SnowStorm", "SnowStorm", "Moder",
			"Ashrain", "Crypt", "SunkenCrypt"
		};

		public static Dictionary<string, string> commandList = new Dictionary<string, string>
		{
			{
				"/alt",
				"- Use alternate on-screen controls. Press '" + ((SkConfigEntry.OAltToggle == null) ? "Home" : SkConfigEntry.OAltToggle.Value) + "' to toggle if active."
			},
			{ "/coords", "- Show coords in corner of the screen" },
			{ "/clear", "- Clear the current output shown in the console" },
			{ "/clearinventory", "- Removes all items from your inventory. There is no confirmation, be careful." },
			{ "/detect", "[Range=20] - Toggle enemy detection" },
			{ "/farinteract", "[Distance=50] - Toggles far interactions (building as well). To change distance, toggle this off then back on with new distance" },
			{ "/env", "[Weather] - Change the weather. No parameter provided will list all weather. -1 will allow the game to control the weather again." },
			{ "/event", "[Event] - Begin an event" },
			{ "/findtomb", "- Pin nearby dead player tombstones on the map if any currently exist" },
			{ "/fly", "- Toggle flying" },
			{ "/freecam", "- Toggle freecam" },
			{ "/goditem", "- Toggle GodItem (Unlimited stack , max quality, unlimited durability)" },
			{ "/ghost", "- Toggle Ghostmode (enemy creatures cannot see you)" },
			{ "/give", "[Item] [Qty=1], OR /give [Item] [Qty=1] [Player] [Level=1] - Gives item to player. If player has a space in name, only provide name before the space. Capital letters matter in item / player name!" },
			{ "/god", "- Toggle Godmode" },
			{ "/heal", "[Player=local] - Heal Player" },
			{ "/imacheater", "- Use the toolbox to force enable standard cheats on any server" },
			{ "/infstam", "- Toggles infinite stamina" },
			{ "/killall", "- Kills all nearby creatures" },
			{ "/listitems", "[Name Contains] - List all items. Optionally include name starts with. Ex. /listitems Woo returns any item that contains the letters 'Woo'" },
			{ "/listskills", "- Lists all skills" },
			{ "/nocost", "- Toggle no requirement building" },
			{ "/nores", "- Toggle no restrictions to where you can build (except ward zones)" },
			{ "/nosup", "- Toggle no supports required for buildings - WARNING! - IF YOU REJOIN AND THIS IS DISABLED, YOUR STRUCTURES MAY FALL APART - USE WITH CARE. Maybe use the AutoRun functionality?" },
			{ "/portals", "- List all portal tags" },
			{ "/randomevent", "- Begins a random event" },
			{ "/removedrops", "- Removes items from the ground" },
			{ "/resetwind", "- If wind has been set, this will allow the game to take control of the wind again" },
			{ "/repair", "- Repair your inventory" },
			{ "/resetmap", "- Reset the map exploration" },
			{ "/revealmap", "- Reveals the entire minimap" },
			{ "/q", "- Quickly exit the game. Commands are sometimes just more convenient." },
			{ "/seed", "- Reveals the map seed" },
			{ "/set cw", "[Weight] - Set your weight limit (default 300)" },
			{ "/set difficulty", "[Player Count] - Set the difficulty (default is number of connected players)" },
			{ "/set exploreradius", "[Radius=100] - Set the explore radius" },
			{ "/set jumpforce", "[Force] - Set jump force (default 10). Careful if you fall too far!" },
			{ "/set pickup", "[Radius] - Set your auto pickup radius (default 2)" },
			{ "/set skill", "[Skill] [Level] - Set your skill level" },
			{ "/set speed", "[Speed Type] [Speed] - Speed Types: crouch (def: 2), run (def: 120), swim (def: 2)" },
			{ "/tpto", "[PlayerName] - Tp to player" },
			//{ "/tptome", "[PlayerName] - Tp player to me" },
			{ "/td", "[Radius=5] [Height=1] - Dig nearby terrain. Radius 30 max." },
			{ "/tl", "[Radius=5] - Level nearby terrain. Radius 30 max." },
			{ "/tr", "[Radius=5] [Height=1] - Raise nearby terrain. Radius 30 max." },
			{ "/tu", "[Radius=5] - Undo terrain modifications around you. Radius 50 max." },
			{ "/spawn", "[Creature Name] [Level=1] - Spawns a creature or prefab in front of you. Capitals in name matter! Ex. /spawn Boar 3 (use /give for items!)" },
			{ "/stopevent", "- Stops a current event" },
			{ "/tame", "- Tame all nearby creatures" },
			{ "/tod", "[0-1] - Set (and lock) time of day (-1 to unlock time) - Ex. /tod 0.5" },
			{ "/tp", "[X,Y] - Teleport you to the coords provided" },
			{ "/wind [Angle] [Intensity]", "Set the wind direction and intensity" },
			{ "/whois", "- List all players" }
		};

		internal static ModConsoleOpt ConsoleOpt
		{
			get
			{
				return consoleOpt;
			}
			set
			{
				consoleOpt = value;
			}
		}

		public static void Announce()
		{
			if (SkVersionChecker.VersionCurrent())
			{
				PrintOut(string.Concat("Toolbox (", SkVersionChecker.currentVersion, ") by Skrip (DS) is enabled. Custom commands are working."), LogTo.Console);
			}
			else
			{
				PrintOut("Toolbox by Skrip (DS) is enabled. Custom commands are working.", LogTo.Console);
				PrintOut(string.Concat("►\tNew Version Available on NexusMods! Current: ", SkVersionChecker.currentVersion, " Latest: ", SkVersionChecker.latestVersion), LogTo.DebugConsole);
			}
			PrintOut("====  Press numpad 0 to open on-screen menu or type /? 1  ====", LogTo.Console);
			try
			{
				commandList = Enumerable.ToDictionary<KeyValuePair<string, string>, string, string>((IEnumerable<KeyValuePair<string, string>>)Enumerable.OrderBy<KeyValuePair<string, string>, string>((IEnumerable<KeyValuePair<string, string>>)commandList, (Func<KeyValuePair<string, string>, string>)((KeyValuePair<string, string> obj) => obj.Key)), (Func<KeyValuePair<string, string>, string>)((KeyValuePair<string, string> obj) => obj.Key), (Func<KeyValuePair<string, string>, string>)((KeyValuePair<string, string> obj) => obj.Value));
				weatherList.Sort();
				SkCommandPatcher.InitPatch();
			}
			catch (Exception)
			{
			}
		}

		public static void ProcessCommands(string inCommand, LogTo source, GameObject go = null)
		{
			if (string.IsNullOrEmpty(inCommand))
			{
				return;
			}
			if (Console.instance != null)
			{
				Console.instance.SetPrivateField("m_lastEntry", inCommand);
			}
			string[] array = inCommand.Split(';');
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i].Trim();
				if (!ProcessCommand(text, source, go))
				{
					if (array.Length > 1)
					{
						Console.instance.m_input.text = text;
						Console.instance.InvokePrivateMethod("InputText", null);
					}
					Console.instance.m_input.text = string.Empty;
				}
			}
		}

		public static bool TpTo(string[] array, LogTo source)
		{
			if (array.Length != 2)
				PrintOut("Missing player name", source);
            else
            {
				Player.m_localPlayer.TeleportTo(GetPlayerInfo(array[1]).m_position, new Quaternion(), false);

				PrintOut("Tp to " + array[1] + "!", source);

			}
			return true;



		}

		public static bool TpToMe(string[] array, LogTo source)
		{

			if (array.Length != 2)
				PrintOut("Missing player name", source);
			else
			{
				try
				{
					ZNet.PlayerInfo me = GetPlayerInfo(Player.m_localPlayer.m_name);

					Player targetplayer = GetPlayer(array[1], source);
					targetplayer.TeleportTo(me.m_position, new Quaternion(), false);

					PrintOut("Tp " + targetplayer.m_name + " to me!", source);
					return true;
				}
				catch
				{
					
				}		
			}



			PrintOut("no player found!", source);
			return true;
		}

		public static Player GetPlayer(string name, LogTo source)
		{
			try
			{
				if (Player.m_localPlayer.name.ToLower() == name.ToLower())
					return Player.m_localPlayer;
				ZNet.PlayerInfo target = GetPlayerInfo(name);
				return Player.GetPlayer(target.m_characterID.userID);

			}

			catch (Exception e)
			{
				PrintOut(e.ToString(), source);
				return Player.m_localPlayer;
			}
		}


		private static ZNet.PlayerInfo GetPlayerInfo(string name)
		{

			return ZNet.instance.GetPlayerList().FirstOrDefault(x => x.m_name.ToLower() == name.ToLower());
			
		}
		


		private static void ItemCheat(object sender , object a)
        {
			foreach (ItemDrop.ItemData item in Player.m_localPlayer.GetInventory().GetAllItems())
			{
				item.m_durability = item.GetMaxDurability();

				item.m_crafterName = "TedShirock le hacker";
				item.m_quality = item.m_shared.m_maxQuality;
				item.m_stack = item.m_shared.m_maxStackSize;
				item.m_shared.m_weight = 0f;
				item.m_shared.m_equipDuration = 0f;
				if(!item.m_shared.m_description.Contains("TedShirock"))
				item.m_shared.m_description = item.m_shared.m_description + " Cheated by TedShirock";
				item.m_shared.m_armor = 9999f;
				item.m_shared.m_attackForce = 9999f;
				item.m_shared.m_deflectionForce = 9999f;
				item.m_shared.m_blockPower = 9999f;
				item.m_shared.m_timedBlockBonus = 9999f;
				item.m_shared.m_dodgeable = false;
				item.m_shared.m_backstabBonus = 9999f;
				item.m_shared.m_damages.m_blunt = 9999f;
				item.m_shared.m_damages.m_chop = 9999f;
				item.m_shared.m_damages.m_damage = 9999f;
				item.m_shared.m_damages.m_fire = 9999f;
				item.m_shared.m_damages.m_frost = 9999f;
				item.m_shared.m_damages.m_lightning = 9999f;
				item.m_shared.m_damages.m_pickaxe = 9999f;
				item.m_shared.m_damages.m_pierce = 9999f;
				item.m_shared.m_damages.m_poison = 9999f;
				item.m_shared.m_damages.m_slash = 9999f;
				item.m_shared.m_damages.m_spirit = 9999f;
			}
		}

		public static bool ProcessCommand(string inCommand, LogTo source, GameObject go = null)
		{
			if (string.IsNullOrEmpty(inCommand) || string.IsNullOrWhiteSpace(inCommand))
			{
				return true;
			}
			inCommand = inCommand.Trim();
			string[] array = inCommand.Split(' ');
			if (inCommand.StartsWith("help") && source.HasFlag(LogTo.Console))
			{
				Console.instance.Print("imacheater - Enable in-game cheats");
				Console.instance.Print("/? [Page] - SkToolbox Commands - Ex /? 1");
				return false;
			}
			if (array[0].Equals("/?"))
			{
				int result = 1;
				if (array.Length > 1 && int.TryParse(array[1], out result))
				{
					if ((float)result > Mathf.Ceil(commandList.Count / pageSize) + (float)((commandList.Count % pageSize != 0) ? 1 : 0))
					{
						result = Mathf.RoundToInt(Mathf.Ceil(commandList.Count / pageSize) + (float)((commandList.Count % pageSize != 0) ? 1 : 0));
					}
					List<string> list = new List<string>(commandList.Keys);
					List<string> list2 = new List<string>(commandList.Values);
					PrintOut("Command List Page " + result + " / " + (Mathf.Ceil(commandList.Count / pageSize) + (float)((commandList.Count % pageSize != 0) ? 1 : 0)), source);
					for (int i = pageSize * result - pageSize; i < ((commandList.Count > pageSize * (result + 1) - pageSize) ? (pageSize * (result + 1) - pageSize) : commandList.Count); i++)
					{
						PrintOut(list[i] + " " + list2[i], source);
					}
				}
				else
				{
					PrintOut("Type /? # to see the help for that page number. Ex. /? 1", source);
				}
				return true;
			}
			if (commandList.ContainsKey(array[0]) && Player.m_localPlayer == null && !array[0].Equals("/q") && !array[0].Equals("/clear"))
			{
				PrintOut("You must be in-game to run commands!", source);
				return true;
			}
			if (array[0].Equals("/repair"))
			{
				List<ItemDrop.ItemData> list3 = new List<ItemDrop.ItemData>();
				Player.m_localPlayer.GetInventory().GetWornItems(list3);
				foreach (ItemDrop.ItemData item in list3)
				{
					try
					{
						item.m_durability = item.GetMaxDurability();
					}
					catch (Exception)
					{
					}
				}
				PrintOut("All items repaired!", source);
				return true;
			}
			if (array[0].Equals("/tpto"))
			{
				return TpTo(array, source);
			}
			/*if (array[0].Equals("/tptome"))
			{
				return TpToMe(array, source);
			}*/
			if (array[0].Equals("/portals"))
			{
				PrintOut(ListPortals(), source, playerSay: true);
				return true;
			}
			if (array[0].Equals("/tl"))
			{
				GameObject prefab = ZNetScene.instance.GetPrefab("digg_v2");
				if (prefab == null)
				{
					PrintOut("Terrain level failed. Report to mod author - terrain level error 1", source);
					return true;
				}
				float radius = 5f;
				if (array.Length > 1)
				{
					try
					{
						radius = int.Parse(array[1]);
					}
					catch (Exception)
					{
					}
				}
				TerrainModification.ModifyTerrain(0, Player.m_localPlayer.transform.position, prefab, radius);
				PrintOut("Terrain levelled!", source);
				return true;
			}
			if (array[0].Equals("/tu"))
			{
				float radius2 = 5f;
				if (array.Length > 1)
				{
					try
					{
						radius2 = int.Parse(array[1]);
					}
					catch (Exception)
					{
					}
				}
				TerrainModification.ResetTerrain(Player.m_localPlayer.transform.position, radius2);
				PrintOut("Terrain reset!", source);
				return true;
			}
			if (array[0].Equals("/tr"))
			{
				GameObject prefab2 = ZNetScene.instance.GetPrefab("raise");
				if (prefab2 == null)
				{
					PrintOut("Terrain raise failed. Report to mod author - terrain raise error 1", source);
					return true;
				}
				float radius3 = 5f;
				float num = 2f;
				if (array.Length > 1)
				{
					try
					{
						radius3 = int.Parse(array[1]);
					}
					catch (Exception)
					{
					}
				}
				if (array.Length > 2)
				{
					try
					{
						num = int.Parse(array[2]);
					}
					catch (Exception)
					{
					}
				}
				TerrainModification.ModifyTerrain(1, Player.m_localPlayer.transform.position + Vector3.up * num, prefab2, radius3);
				PrintOut("Terrain raised!", source);
				return true;
			}
			if (array[0].Equals("/td"))
			{
				GameObject prefab3 = ZNetScene.instance.GetPrefab("digg_v2");
				if (prefab3 == null)
				{
					PrintOut("Terrain dig failed. Report to mod author - terrain dig error 1", source);
					return true;
				}
				float radius4 = 5f;
				float num2 = 1f;
				if (array.Length > 1)
				{
					try
					{
						radius4 = int.Parse(array[1]);
					}
					catch (Exception)
					{
					}
				}
				if (array.Length > 2)
				{
					try
					{
						num2 = int.Parse(array[2]);
					}
					catch (Exception)
					{
					}
				}
				TerrainModification.ModifyTerrain(-1, Player.m_localPlayer.transform.position - Vector3.up * num2, prefab3, radius4);
				PrintOut("Terrain dug!", source);
				return true;
			}
			if (array[0].Equals("/resetwind"))
			{
				EnvMan.instance.ResetDebugWind();
				PrintOut("Wind unlocked and under game control.", source);
			}
			if (array[0].Equals("/wind"))
			{
				string[] array2 = inCommand.Split(' ');
				if (array2.Length == 3)
				{
					float angle = float.Parse(array2[1]);
					float intensity = float.Parse(array2[2]);
					EnvMan.instance.SetDebugWind(angle, intensity);
				}
				else
				{
					PrintOut("Failed to set wind. Check parameters! Ex. /wind 240 5", source);
				}
			}
			if (array[0].Equals("/env"))
			{
				if (array.Length == 1)
				{
					foreach (string item2 in Enumerable.OrderBy<string, string>((IEnumerable<string>)weatherList, (Func<string, string>)((string q) => q)).ToList())
					{
						PrintOut(item2, source);
					}
				}
				else if (array.Length >= 2)
				{
					if (array[1].Equals("-1"))
					{
						if (EnvMan.instance != null)
						{
							EnvMan.instance.m_debugEnv = "";
							PrintOut("Weather unlocked and under game control.", source);
						}
					}
					else
					{
						string text = string.Empty;
						if (array.Length > 2)
						{
							string[] array3 = array;
							foreach (string text2 in array3)
							{
								if (!text2.Equals(array[0]))
								{
									text = text + " " + text2;
								}
							}
							text = text.Trim();
						}
						else
						{
							text = array[1];
						}
						if (weatherList.Contains(text))
						{
							if (EnvMan.instance != null)
							{
								EnvMan.instance.m_debugEnv = text;
								PrintOut("Weather set to: " + EnvMan.instance.m_debugEnv, source);
							}
							else
							{
								PrintOut("1Failed to set weather to '" + text + "'. Can't find environment manager.", source);
							}
						}
						else
						{
							PrintOut("2Failed to set weatherto '" + text + "'. Check parameters! Ex. /env, /env -1, /env Misty", source);
						}
					}
				}
				else
				{
					PrintOut("Failed to set weather. Check parameters! Ex. /env, /env -1, /env Misty", source);
				}
				return true;
			}
			if (array[0].Equals("/fly"))
			{
				flyEnabled = !flyEnabled;
				Player.m_debugMode = flyEnabled;
				Player.m_localPlayer.SetPrivateField("m_debugFly", flyEnabled);
				PrintOut("Fly toggled! (" + flyEnabled + ")", source);
				return true;
			}
			if (array[0].Equals("/alt"))
			{
				altOnScreenControls = !altOnScreenControls;
				PrintOut("Alt controls toggled! (" + altOnScreenControls + ")", source);
				return true;
			}
			if (array[0].Equals("/stopevent"))
			{
				RandEventSystem.instance.ResetRandomEvent();
				PrintOut("Event stopped!", source);
				return true;
			}
			if (array[0].Equals("/revealmap"))
			{
				Minimap.instance.ExploreAll();
				PrintOut("Map revealed!", source);
			}
			if (array[0].Equals("/whois"))
			{
				string text3 = string.Empty;
				foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
				{
					
					text3 = text3 + ", " + player.m_name + "(" + player.m_characterID + ")";
					
				}
				if (text3.Length > 2)
				{
					text3 = text3.Remove(0, 2);
				}
				PrintOut("Active Players (" + ZNet.instance.GetPlayerList().Count + ") - " + text3, source, playerSay: true);
			}
			if (array[0].Equals("/give"))
			{
				inCommand = inCommand.Remove(0, 6);
				string[] array4 = inCommand.Split(' ');
				string text4 = Player.m_localPlayer.GetPlayerName();
				_ = string.Empty;
				int num3 = 1;
				int num4 = 1;
				if (array4.Length == 0)
				{
					PrintOut("Failed. No item provided. /give [Item] [Qty=1] [Player] [Level=1]", source);
				}
				if (array4.Length >= 1)
				{
					_ = array4[0];
				}
				if (array4.Length == 2)
				{
					try
					{
						num3 = int.Parse(array4[1]);
					}
					catch (Exception)
					{
					}
				}
				if (array4.Length > 2)
				{
					try
					{
						num3 = int.Parse(array4[1]);
					}
					catch (Exception)
					{
						num3 = 1;
					}
				}
				if (array4.Length >= 3)
				{
					try
					{
						text4 = array4[2];
					}
					catch (Exception)
					{
					}
				}
				if (array4.Length >= 4)
				{
					try
					{
						num4 = int.Parse(array4[3]);
					}
					catch (Exception)
					{
						num4 = 1;
					}
				}
				bool flag = false;
				foreach (GameObject item3 in ObjectDB.instance.m_items)
				{
					if (item3.GetComponent<ItemDrop>().name.StartsWith(array4[0]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					PrintOut("Failed. Item does not exist. /give [Item] [Qty=1] [Player] [Level=1]. Check for items with /listitems. Capital letters matter on this command!", source);
					return true;
				}
				ZNet.PlayerInfo player = GetPlayerInfo(text4);				
				GameObject prefab4 = ZNetScene.instance.GetPrefab(array4[0]);
				if ((bool)prefab4)
				{
					PrintOut("Spawning " + num3 + " of item " + array4[0] + "(" + num4 + ") on " + text4, source, playerSay: true);
					try
					{
						ItemDrop itemDrop = (ItemDrop)UnityEngine.Object.Instantiate(prefab4, new Vector3(player.m_position.x, player.m_position.y, player.m_position.z + 1.5f), Quaternion.identity).GetComponent(typeof(ItemDrop));
						if (itemDrop != null && itemDrop.m_itemData != null)
						{
							itemDrop.m_itemData.m_quality = num4;
							itemDrop.m_itemData.m_stack = num3;
							itemDrop.m_itemData.m_durability = itemDrop.m_itemData.GetMaxDurability();
						}
					}
					catch (Exception ex12)
					{
						PrintOut("Something unexpected failed.", source);
						SkUtilities.Logz(new string[2] { "ERR", "/give" }, new string[2] { ex12.Message, ex12.Source }, LogType.Warning);
					}
				}
				else
				{
					PrintOut("Failed. Check parameters. /give [Item] [Qty=1] [Player] [Level=1]", source);
				}
				return true;
			}
			if (array[0].Equals("/god"))
			{
				godEnabled = !godEnabled;
				Player.m_localPlayer.SetGodMode(godEnabled);
				PrintOut("God toggled! (" + godEnabled + ")", source);
				return true;
			}
			if (array[0].Equals("/clearinventory"))
			{
				Player.m_localPlayer.GetInventory().RemoveAll();
				PrintOut("All items removed from inventory.", source);
				return true;
			}
			if (array[0].Equals("/findtomb"))
			{
				TombStone[] array5 = UnityEngine.Object.FindObjectsOfType<TombStone>();
				if (array5.Length != 0)
				{
					TombStone[] array6 = array5;
					foreach (TombStone tombStone in array6)
					{
						if (tombStone != null && tombStone.enabled)
						{
							Minimap.instance.AddPin(tombStone.transform.position, Minimap.PinType.Ping, "TS", save: true, isChecked: true);
						}
					}
				}
				PrintOut("Tombstone sought out! Potentially " + array5.Length + " found.", source);
				return true;
			}
			if (array[0].Equals("/seed"))
			{
				World privateField = WorldGenerator.instance.GetPrivateField<World>("m_world");
				PrintOut("Map seed: " + privateField.m_seedName, source, playerSay: true);
				return true;
			}
			if (array[0].Equals("/freecam"))
			{
				GameCamera.instance.ToggleFreeFly();
				PrintOut("Free cam toggled " + GameCamera.InFreeFly(), source, playerSay: true);
				return true;
			}
			if (array[0].Equals("/heal"))
			{
				if (array.Length > 1)
				{
					foreach (Player allPlayer2 in Player.GetAllPlayers())
					{
						if (allPlayer2 != null && allPlayer2.GetPlayerName().ToLower().Equals(array[1].ToLower()))
						{
							allPlayer2.Heal(allPlayer2.GetMaxHealth());
							PrintOut("Player healed: " + allPlayer2.GetPlayerName(), source, playerSay: true);
						}
					}
				}
				else
				{
					Player.m_localPlayer.Heal(Player.m_localPlayer.GetMaxHealth());
					PrintOut("Self healed.", source, playerSay: true);
				}
				return true;
			}
			if (array[0].Equals("/nores"))
			{
				SkCommandPatcher.InitPatch();
				SkCommandPatcher.bBuildAnywhere = !SkCommandPatcher.bBuildAnywhere;
				PrintOut("No build restrictions toggled! (" + SkCommandPatcher.bBuildAnywhere + ")", source);
				return true;
			}
			if (array[0].Equals("/nocost"))
			{
				noCostEnabled = !noCostEnabled;
				Player.m_debugMode = noCostEnabled;
				Player.m_localPlayer.SetPrivateField("m_noPlacementCost", noCostEnabled);
				PrintOut("No build cost/requirements toggled! (" + noCostEnabled + ")", source);
				return true;
			}
			if (array[0].Equals("/event"))
			{
				if (array.Length > 1)
				{
					if (RandEventSystem.instance.HaveEvent(array[1]))
					{
						RandEventSystem.instance.SetRandomEventByName(array[1], Player.m_localPlayer.transform.position);
						PrintOut("Event started!", source);
					}
					else
					{
						PrintOut("Event does not exist, please try again.", source);
					}
				}
				else
				{
					PrintOut("Please provide an event name. Ex. /event NAME", source);
				}
				return true;
			}
			if (array[0].Equals("/randomevent"))
			{
				RandEventSystem.instance.StartRandomEvent();
				PrintOut("Random event started!", source);
				return true;
			}
			if (array[0].Equals("/tp"))
			{
				if (array.Length != 2 || !array[1].Contains(","))
				{
					PrintOut("Syntax /tp X,Z", source);
					return true;
				}
				try
				{
					string[] array7 = array[1].Split(',');
					float x = float.Parse(array7[0]);
					float z = float.Parse(array7[1]);
					float groundHeight = ZoneSystem.instance.GetGroundHeight(new Vector3(x, 750f, z));
					groundHeight = Mathf.Clamp(groundHeight, 0f, 100f);
					if (groundHeight > 99f)
					{
						groundHeight = Player.m_localPlayer.transform.position.y;
					}
					Player localPlayer = Player.m_localPlayer;
					if ((bool)localPlayer)
					{
						Vector3 pos = new Vector3(x, groundHeight, z);
						localPlayer.TeleportTo(pos, localPlayer.transform.rotation, distantTeleport: false);
						PrintOut("Teleporting...", source, playerSay: true);
					}
				}
				catch (Exception)
				{
					PrintOut("Syntax /tp X,Z", source);
				}
				return true;
			}
			if (array[0].Equals("/detect"))
			{
				bDetectEnemies = !bDetectEnemies;
				if (array.Length != 0)
				{
					try
					{
						bDetectRange = int.Parse(array[1]);
						bDetectRange = ((bDetectRange < 5) ? 5 : bDetectRange);
					}
					catch (Exception)
					{
						bDetectRange = 20;
					}
				}
				PrintOut("Detect enemies toggled! (" + bDetectEnemies.ToString() + ", range: " + bDetectRange + ")", source);
				return true;
			}
			if (array[0].Equals("/imacheater"))
			{
				SkCommandPatcher.InitPatch();
				SkCommandPatcher.BCheat = !SkCommandPatcher.BCheat;
				if (Player.m_localPlayer != null)
				{
					try
					{
						Player.m_localPlayer.SetPrivateField("m_debugMode", SkCommandPatcher.BCheat);
						Console.instance.SetPrivateField("m_cheat", SkCommandPatcher.BCheat);
					}
					catch (Exception)
					{
					}
				}
				PrintOut("Cheats toggled! (" + SkCommandPatcher.BCheat + ")", source);
				return true;
			}
			if (array[0].Equals("/nosup"))
			{
				SkCommandPatcher.InitPatch();
				SkCommandPatcher.BFreeSupport = !SkCommandPatcher.BFreeSupport;
				PrintOut("No build support requirements toggled! (" + SkCommandPatcher.BFreeSupport + ")", source);
				return true;
			}
			if (array[0].Equals("/coords"))
			{
				bCoords = !bCoords;
				PrintOut("Show coords toggled! (" + bCoords + ")", source);
				return true;
			}
			if (array[0].Equals("/resetmap"))
			{
				Minimap.instance.Reset();
				return true;
			}
			if (array[0].Equals("/infstam"))
			{
				infStamina = !infStamina;
				if (infStamina)
				{
					Player.m_localPlayer.m_staminaRegenDelay = 0.1f;
					Player.m_localPlayer.m_staminaRegen = 99f;
					Player.m_localPlayer.m_runStaminaDrain = 0f;
					Player.m_localPlayer.SetMaxStamina(999f, flashBar: true);
				}
				else
				{
					Player.m_localPlayer.m_staminaRegenDelay = 1f;
					Player.m_localPlayer.m_staminaRegen = 5f;
					Player.m_localPlayer.m_runStaminaDrain = 10f;
					Player.m_localPlayer.SetMaxStamina(100f, flashBar: true);
				}
				PrintOut("Infinite stamina toggled! (" + infStamina + ")", source);
				return true;
			}
			if (array[0].Equals("/goditem"))
			{
				godItem = !godItem;
				if (godItem)
				{
                    GodItem.Elapsed += ItemCheat;
					GodItem.Start();
				}
				else
				{

					GodItem.Elapsed -= ItemCheat;
					GodItem.Stop();
				}
				PrintOut("God Item toggled! (" + godItem + ")", source);
				return true;
			}
			if (array[0].Equals("/tame"))
			{
				Tameable.TameAllInArea(Player.m_localPlayer.transform.position, 20f);
				PrintOut("Creatures tamed!", source);
				return true;
			}
			if (array[0].Equals("/farinteract"))
			{
				farInteract = !farInteract;
				if (farInteract)
				{
					if (array.Length > 1)
					{
						try
						{
							int num5 = ((int.Parse(array[1]) < 20) ? 20 : int.Parse(array[1]));
							Player.m_localPlayer.m_maxInteractDistance = num5;
							Player.m_localPlayer.m_maxPlaceDistance = num5;
						}
						catch (Exception)
						{
							PrintOut("Failed to set far interaction distance. Check params. /farinteract 50", source);
						}
					}
					else
					{
						Player.m_localPlayer.m_maxInteractDistance = 50f;
						Player.m_localPlayer.m_maxPlaceDistance = 50f;
					}
					PrintOut("Far interactions toggled! (" + farInteract.ToString() + " Distance: " + Player.m_localPlayer.m_maxInteractDistance + ")", source);
				}
				else
				{
					Player.m_localPlayer.m_maxInteractDistance = 5f;
					Player.m_localPlayer.m_maxPlaceDistance = 5f;
					PrintOut("Far interactions toggled! (" + farInteract + ")", source);
				}
				return true;
			}
			if (array[0].Equals("/ghost"))
			{
				Player.m_localPlayer.SetGhostMode(!Player.m_localPlayer.InGhostMode());
				PrintOut("Ghost mode toggled! (" + Player.m_localPlayer.InGhostMode() + ")", source);
				return true;
			}
			if (array[0].Equals("/tod"))
			{
				if (array.Length > 1)
				{
					if (!float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
					{
						return true;
					}
					if (result2 < 0f)
					{
						EnvMan.instance.m_debugTimeOfDay = false;
						PrintOut("Time unlocked and under game control.", source);
					}
					else
					{
						EnvMan.instance.m_debugTimeOfDay = true;
						EnvMan.instance.m_debugTime = Mathf.Clamp01(result2);
						PrintOut("Setting time of day:" + result2, source);
					}
				}
				else
				{
					PrintOut("Failed. Syntax /tod [0-1] Ex. /tod 0.5", source);
				}
				return true;
			}
			if (array[0].Equals("/set"))
			{
				if (array.Length > 1)
				{
					if (array[1].Equals("cw"))
					{
						try
						{
							int num6 = int.Parse(array[2]);
							Player.m_localPlayer.m_maxCarryWeight = num6;
							PrintOut("New carry weight set to: " + num6, source);
						}
						catch (Exception)
						{
							PrintOut("Failed to set new carry weight. Check params.", source);
						}
						return true;
					}
					if (array[1].Equals("skill"))
					{
						if (array.Length == 4 && !array[2].Contains("None") && !array[2].Contains("All") && !array[2].Contains("FireMagic") && !array[2].Contains("FrostMagic"))
						{
							try
							{
								string text5 = array[2];
								int num7 = int.Parse(array[3]);
								Player.m_localPlayer.GetSkills().CheatResetSkill(text5.ToLower());
								Player.m_localPlayer.GetSkills().CheatRaiseSkill(text5.ToLower(), num7);
								return true;
							}
							catch (Exception)
							{
								PrintOut("Failed to set skill. Check params / skill name. See /listskills. /set skill [skill] [level]", source);
								return true;
							}
						}
						PrintOut("Failed to set skill. Check params / skill name. See /listskills.  /set skill [skill] [level]", source);
						return true;
					}
					if (array[1].Equals("pickup"))
					{
						if (array.Length >= 3)
						{
							try
							{
								int num8 = int.Parse(array[2]);
								Player.m_localPlayer.m_autoPickupRange = num8;
								PrintOut("New range set to: " + num8, source);
								return true;
							}
							catch (Exception)
							{
								PrintOut("Failed to set pickup range. Check params. /set pickup 2", source);
								return true;
							}
						}
						PrintOut("Failed to set pickup range. Check params.  /set pickup 2", source);
						return true;
					}
					if (array[1].Equals("jumpforce"))
					{
						if (array.Length >= 3)
						{
							try
							{
								int num9 = int.Parse(array[2]);
								Player.m_localPlayer.m_jumpForce = num9;
								PrintOut("New range set to: " + num9, source);
								return true;
							}
							catch (Exception)
							{
								PrintOut("Failed to set jump force. Check params. /set jumpforce 10", source);
								return true;
							}
						}
						PrintOut("Failed to set jump force. Check params.  /set jumpforce 10", source);
						return true;
					}
					if (array[1].Equals("exploreradius"))
					{
						if (array.Length >= 3)
						{
							try
							{
								int num10 = int.Parse(array[2]);
								Minimap.instance.m_exploreRadius = num10;
								PrintOut("New range set to: " + num10, source);
								return true;
							}
							catch (Exception)
							{
								PrintOut("Failed to set explore radius. Check params. /set exploreradius 100", source);
								return true;
							}
						}
						PrintOut("Failed to set explore radius. Check params.  /set exploreradius 100", source);
						return true;
					}
					if (array[1].Equals("speed"))
					{
						if (array.Length >= 4)
						{
							string text6 = array[2];
							if (new string[3] { "crouch", "run", "swim" }.Contains(text6))
							{
								try
								{
									int num11 = int.Parse(array[3]);
									switch (text6)
									{
									case "crouch":
										Player.m_localPlayer.m_crouchSpeed = num11;
										break;
									case "run":
										Player.m_localPlayer.m_runSpeed = num11;
										break;
									case "swim":
										Player.m_localPlayer.m_swimSpeed = num11;
										break;
									}
									PrintOut("New " + text6 + " speed set to: " + num11, source);
								}
								catch (Exception)
								{
									PrintOut("Failed to set speed. Check params name. Ex. /set speed crouch 2", source);
									return true;
								}
							}
							else
							{
								PrintOut("Failed to set speed. Check params name. Ex.  /set speed crouch 2", source);
							}
							return true;
						}
						PrintOut("Failed to set speed. Check params name. Ex.  /set speed crouch 2", source);
						return true;
					}
					if (array[1].Equals("difficulty"))
					{
						if (array.Length >= 3)
						{
							try
							{
								int forcePlayerDifficulty = int.Parse(array[2]);
								Game.instance.SetForcePlayerDifficulty(forcePlayerDifficulty);
								PrintOut("Difficulty set to " + forcePlayerDifficulty, source);
								return true;
							}
							catch (Exception)
							{
								PrintOut("Failed to set difficulty. Check params. /set difficulty 5", source);
								return true;
							}
						}
						PrintOut("Failed to set difficulty. Check params.  /set difficulty 5", source);
						return true;
					}
				}
				return true;
			}
			if (array[0].Equals("/removedrops"))
			{
				ItemDrop[] array8 = UnityEngine.Object.FindObjectsOfType<ItemDrop>();
				for (int k = 0; k < array8.Length; k++)
				{
					ZNetView component = array8[k].GetComponent<ZNetView>();
					if ((bool)component)
					{
						component.Destroy();
					}
				}
				PrintOut("Items cleared.", source, playerSay: true);
				return true;
			}
			if (array[0].Equals("/spawn"))
			{
				ZNetView[] array9 = UnityEngine.Object.FindObjectsOfType<ZNetView>();
				if (array9.Length == 0)
				{
					PrintOut("Couldn't find zdo...", source);
				}
				if (array.Length > 1)
				{
					GameObject prefab5 = ZNetScene.instance.GetPrefab(array[1]);
					if (prefab5 == null)
					{
						PrintOut("Creature not found.", source);
						return true;
					}
					_ = Player.m_localPlayer.transform.position;
					GameObject gameObject = UnityEngine.Object.Instantiate(prefab5, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 1.5f, Quaternion.identity);
					ZNetView component2 = gameObject.GetComponent<ZNetView>();
					gameObject.GetComponent<BaseAI>();
					if (array.Length > 2)
					{
						Character component3 = gameObject.GetComponent<Character>();
						if ((bool)component3)
						{
							int num12 = int.Parse(array[2]);
							if (num12 > 10)
							{
								num12 = 10;
							}
							component3.SetLevel(num12);
						}
					}
					if (array9.Length != 0)
					{
						component2.GetZDO().SetPGWVersion(array9[0].GetZDO().GetPGWVersion());
						array9[0].GetZDO().Set("spawn_id", component2.GetZDO().m_uid);
						array9[0].GetZDO().Set("alive_time", ZNet.instance.GetTime().Ticks);
					}
					PrintOut("Creature spawned - " + array[1], source);
				}
				return true;
			}
			if (array[0].Equals("/killall"))
			{
				List<Character> list4 = new List<Character>();
				Character.GetCharactersInRange(Player.m_localPlayer.transform.position, 50f, list4);
				foreach (Character item4 in list4)
				{
					if (!item4.IsPlayer())
					{
						HitData hitData = new HitData();
						hitData.m_damage.m_damage = 1E+10f;
						item4.Damage(hitData);
					}
				}
				PrintOut("Nearby creatures killed! (50m)", source);
				return true;
			}
			if (array[0].Equals("/listitems"))
			{
				if (array.Length > 1)
				{
					foreach (GameObject item5 in ObjectDB.instance.m_items)
					{
						ItemDrop component4 = item5.GetComponent<ItemDrop>();
						if (component4.name.ToLower().Contains(array[1].ToLower()))
						{
							PrintOut("Item: '" + component4.name + "'", source);
						}
					}
				}
				else
				{
					foreach (GameObject item6 in ObjectDB.instance.m_items)
					{
						ItemDrop component5 = item6.GetComponent<ItemDrop>();
						PrintOut("Item: '" + component5.name + "'", source);
					}
				}
				return true;
			}
			if (array[0].Equals("/listskills"))
			{
				string text7 = "Skills found: ";
				foreach (object value in Enum.GetValues(typeof(Skills.SkillType)))
				{
					if (!value.ToString().Contains("None") && !value.ToString().Contains("All") && !value.ToString().Contains("FireMagic") && !value.ToString().Contains("FrostMagic"))
					{
						text7 = text7 + value.ToString() + ", ";
					}
				}
				text7 = text7.Remove(text7.Length - 2);
				PrintOut(text7, source);
				return true;
			}
			if (array[0].Equals("/q"))
			{
				PrintOut("Quitting game...", source);
				Application.Quit();
				return true;
			}
			if (array[0].Equals("/clear"))
			{
				if (Console.instance != null)
				{
					Console.instance.m_output.text = string.Empty;
					try
					{
						Console.instance.SetPrivateField("m_chatBuffer", new List<string>());
					}
					catch (Exception)
					{
					}
				}
				return true;
			}
			return false;
		}

		public static void PrintOut(string text, LogTo source, bool playerSay = false)
		{
			if (text.Equals(string.Empty) || text.Equals(" "))
			{
				return;
			}
			if (source.HasFlag(LogTo.Console) && Console.instance != null)
			{
				Console.instance.Print("(SkToolbox) " + text);
				if (ConsoleOpt != null && ConsoleOpt.conWriteToFile)
				{
					SkUtilities.Logz(new string[2] { "DUMP", "ITEM" }, new string[1] { text });
				}
			}
			if (source.HasFlag(LogTo.Chat) && Chat.instance != null)
			{
				if (playerSay && Player.m_localPlayer != null && SkConfigEntry.CAllowPublicChatOutput != null && SkConfigEntry.CAllowPublicChatOutput.Value)
				{
					Player.m_localPlayer.GetComponent<Talker>().Say(Talker.Type.Normal, text);
				}
				else
				{
					ChatPrint(text);
				}
			}
			if (source.HasFlag(LogTo.DebugConsole))
			{
				SkUtilities.Logz(new string[1] { "TOOLBOX" }, new string[1] { text });
			}
		}

		public static void ChatPrint(string ln, string source = "(SkToolbox) ")
		{
			if (!(Chat.instance != null))
			{
				return;
			}
			Chat.instance.OnNewChatMessage(null, 999L, chatPos, Talker.Type.Normal, source, ln);
			Chat.instance.SetPrivateField("m_hideTimer", 0f);
			Chat.instance.m_chatWindow.gameObject.SetActive(value: true);
			Chat.instance.m_input.gameObject.SetActive(value: true);
			foreach (Chat.WorldTextInstance item in Chat.instance.GetPrivateField<List<Chat.WorldTextInstance>>("m_worldTexts"))
			{
				if (item.m_talkerID == 999)
				{
					item.m_timer = 999f;
				}
			}
		}

		public static string ListPortals(bool printToChat = false)
		{
			List<ZDO> list = new List<ZDO>();
			int index = 0;
			bool flag = false;
			while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(Game.instance.m_portalPrefab.name, list, ref index))
			{
			}
			string text = string.Empty;
			foreach (ZDO item in list)
			{
				item.GetZDOID("target");
				string @string = item.GetString("tag");
				if (!@string.Equals(string.Empty) && !@string.Equals(" "))
				{
					text = text + "'" + @string + "', ";
				}
			}
			if (!text.Equals(string.Empty))
			{
				text = text.Substring(0, text.Length - 2);
				return "Portals found: " + text;
			}
			return "No portals found.";
		}
	}
}
