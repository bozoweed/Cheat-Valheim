using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkToolbox.SkModules
{
	internal class ModPlayer : SkBaseModule, IModule
	{
		private Player selectedPlayer;

		private int spawnQuantity = 1;

		private bool bTeleport;

		private Rect rectClock = new Rect(5f, 5f, 125f, 20f);

		private Rect rectCoords = new Rect(5f, 27f, 125f, 20f);

		private Rect rectEnemy = new Rect(5f, 280f, 425f, 50f);

		private List<Character> nearbyCharacters = new List<Character>();

		public ModPlayer()
		{
			ModuleName = "Player";
			Loading();
		}

		public void Start()
		{
			selectedPlayer = Player.m_localPlayer;
			BeginMenu();
			Ready();
		}

		public void BeginMenu()
		{
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("Repair All", RepairAll, "Repair all your items");
			skMenu.AddItem("Heal Self", Heal, "Heal yourself");
			skMenu.AddItem("Tame", Tame, "Tame all nearby creatures");
			skMenu.AddItem("List of Player", BeginListPlayer, "List of player!");
			skMenu.AddItemToggle("Enable Teleport to Mouse", ref bTeleport, ToggleTeleport, "Press tilde (Alt Gr) to teleport!");
			skMenu.AddItemToggle("Enable God Item", ref SkCommandProcessor.godItem, ToggleGodItem, "Item Cheated!");
			skMenu.AddItemToggle("Build Anywhere", ref SkCommandPatcher.bBuildAnywhere, ToggleAnywhere, "Remove build restrictions");
			skMenu.AddItemToggle("No Cost Building", ref SkCommandProcessor.noCostEnabled, ToggleNoCost, "Unlock all pieces and no cost");
			skMenu.AddItemToggle("Detect Nearby Enemies", ref SkCommandProcessor.bDetectEnemies, ToggleESPEnemies, "Range: 20m");
			skMenu.AddItemToggle("Display Coordinates", ref SkCommandProcessor.bCoords, ToggleCoords, "Display coords in top left corner");
			skMenu.AddItemToggle("Godmode", ref SkCommandProcessor.godEnabled, ToggleGodmode, "Godmode for yourself");
			skMenu.AddItemToggle("Flying", ref SkCommandProcessor.flyEnabled, ToggleFlying, "Flying for yourself");
			skMenu.AddItemToggle("Infinite Stamina", ref SkCommandProcessor.infStamina, ToggleInfStam, "Infinite stamina for yourself");
			skMenu.AddItem("Give Item\t\t►", BeginListItems, "Give item(s) to self");
			base.MenuOptions = skMenu;
		}

		public void BeginListPlayer()
		{
			SkMenu skMenu = new SkMenu();			
			foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
			{
				skMenu.AddItem(player.m_name, BeginListPlayerAction, "Get Action for player "+ player.m_name);
			}
			RequestMenu(skMenu);
		}

		public void BeginListPlayerAction(string player)
        {
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("Kick", (name) =>
			{
				SkCommandProcessor.ProcessCommand("/kick " + player, SkCommandProcessor.LogTo.Chat);
				BeginMenu();
			}, "Kick Player");
			skMenu.AddItem("Ban", (name) =>
			{
				SkCommandProcessor.ProcessCommand("/ban " + player, SkCommandProcessor.LogTo.Chat);
				BeginMenu();
			}, "Ban Player");
			skMenu.AddItem("Heal", (name) =>
			{
				SkCommandProcessor.ProcessCommand("/heal " + player, SkCommandProcessor.LogTo.Chat);
				BeginMenu();
			}, "Heal Player");
			skMenu.AddItem("kill", (name) =>
			{
				SkCommandProcessor.ProcessCommand("/kill " + player, SkCommandProcessor.LogTo.Chat);
				BeginMenu();
			}, "Kill Player");
			skMenu.AddItem("Tp To player", (name) =>
			{
				SkCommandProcessor.ProcessCommand("/tpto " + player, SkCommandProcessor.LogTo.Chat);
				BeginMenu();
			}, "Tp To player");
			/*skMenu.AddItem("Tp player to me", (name) =>
			{
				SkCommandProcessor.ProcessCommand("/tptome " + player, SkCommandProcessor.LogTo.Chat);
				BeginMenu();
			}, "Tp player to me");*/

			RequestMenu(skMenu);
		}

		public void BeginListItems()
		{
			SkMenu skMenu = new SkMenu();
			List<ItemDrop> list = new List<ItemDrop>();
			foreach (GameObject item in ObjectDB.instance.m_items)
			{
				ItemDrop component = item.GetComponent<ItemDrop>();
				list.Add(component);
			}
			skMenu.AddItem("Quantity\t►", BeginPromptQuantity, "Quantity: " + spawnQuantity);
			foreach (ItemDrop item2 in list)
			{
				skMenu.AddItem(item2.name, GiveItem);
			}
			RequestMenu(skMenu);
		}

		public void BeginPromptQuantity()
		{
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("1", SetSpawnQuantity);
			skMenu.AddItem("2", SetSpawnQuantity);
			skMenu.AddItem("5", SetSpawnQuantity);
			skMenu.AddItem("10", SetSpawnQuantity);
			skMenu.AddItem("25", SetSpawnQuantity);
			skMenu.AddItem("50", SetSpawnQuantity);
			skMenu.AddItem("100", SetSpawnQuantity);
			RequestMenu(skMenu);
		}

		public void SetSpawnQuantity(string ln = "")
		{
			int result = 1;
			int.TryParse(ln, out result);
			spawnQuantity = result;
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Quantity set: " + spawnQuantity);
		}

		public void GiveItem(string ln = "")
		{
			if (selectedPlayer != null)
			{
				GameObject prefab = ZNetScene.instance.GetPrefab(ln);
				if (prefab != null)
				{
					Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + ln);
					for (int i = 0; i < spawnQuantity; i++)
					{
						try
						{
							UnityEngine.Object.Instantiate(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up, Quaternion.identity).GetComponent<Character>().SetLevel(1);
						}
						catch (Exception)
						{
						}
					}
				}
				else
				{
					Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Object " + ln + " not found.");
				}
			}
			else
			{
				Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Player not found. Set to local player.");
				selectedPlayer = Player.m_localPlayer;
				GiveItem(ln);
			}
			BeginMenu();
		}

		public void ToggleTeleport()
		{
			bTeleport = !bTeleport;
			if (bTeleport)
			{
				Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Teleport enabled. Press tilde (Alt Gr)!");
			}
			else
			{
				Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Teleport disabled.");
			}
			BeginMenu();
		}

		public void Heal()
		{
			SkCommandProcessor.ProcessCommand("/heal", SkCommandProcessor.LogTo.Chat);
		}

		public void Tame()
		{
			SkCommandProcessor.ProcessCommand("/tame", SkCommandProcessor.LogTo.Chat);
		}

		public void ToggleNoCost()
		{
			SkCommandProcessor.ProcessCommand("/nocost", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleESPEnemies()
		{
			SkCommandProcessor.ProcessCommand("/detect 20", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleCoords()
		{
			SkCommandProcessor.ProcessCommand("/coords", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleAnywhere()
		{
			SkCommandProcessor.ProcessCommand("/nores", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleGodItem()
		{
			SkCommandProcessor.ProcessCommand("/goditem", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleGodmode()
		{
			SkCommandProcessor.ProcessCommand("/god", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleFlying()
		{
			SkCommandProcessor.ProcessCommand("/fly", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void ToggleInfStam()
		{
			SkCommandProcessor.ProcessCommand("/infstam", SkCommandProcessor.LogTo.Chat);
			BeginMenu();
		}

		public void RepairAll()
		{
			SkCommandProcessor.ProcessCommand("/repair", SkCommandProcessor.LogTo.Chat);
		}

		private void Update()
		{
			if (bTeleport && Input.GetKeyDown(KeyCode.AltGr) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
			{
				Vector3 point = hitInfo.point;
				Debug.DrawRay(Player.m_localPlayer.transform.position, point, Color.white);
				Player.m_localPlayer.transform.position = point;
				Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Warp!");
			}
		}

		private void OnGUI()
		{
		}
	}
}
