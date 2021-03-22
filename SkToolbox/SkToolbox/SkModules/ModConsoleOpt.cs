using System;
using System.Collections.Generic;
using SkToolbox.Configuration;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox.SkModules
{
	internal class ModConsoleOpt : SkBaseModule, IModule
	{
		private class History
		{
			private List<string> history = new List<string>();

			private int index;

			private string current;

			public void Add(string item)
			{
				history.Add(item);
				index = 0;
			}

			public string Fetch(string current, bool next)
			{
				if (index == 0)
				{
					this.current = current;
				}
				if (history.Count == 0)
				{
					return current;
				}
				index += ((!next) ? 1 : (-1));
				if (history.Count + index < 0 || history.Count + index > history.Count - 1)
				{
					index = 0;
					return this.current;
				}
				return history[history.Count + index];
			}
		}

		internal bool conWriteToFile;

		private string consoleLastMessage = string.Empty;

		private string chatInLastMessage = string.Empty;

		private Rect EnemyWindow;

		private History consoleHistory = new History();

		private Rect rectCoords = new Rect(5f, 20f, 125f, 20f);

		private Rect rectEnemy = new Rect(5f, 415f, 375f, 50f);

		private bool anncounced1;

		private bool anncounced2;

		private List<Character> nearbyCharacters = new List<Character>();

		public ModConsoleOpt()
		{
			ModuleName = "CC Controller";
			Loading();
		}

		public void Start()
		{
			SkCommandProcessor.ConsoleOpt = this;
			BeginMenu();
			Ready();
		}

		public void BeginMenu()
		{
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("Reload Toolbox", ReloadMenu);
			skMenu.AddItem("Unload Toolbox", UnloadMenu);
			skMenu.AddItem("Open Log Folder", OpenLogFolder);
			base.MenuOptions = skMenu;
		}

		public void OpenLogFolder()
		{
			SkUtilities.Logz(new string[2] { "CMD", "REQ" }, new string[1] { "Opening Log Directory" });
			Application.OpenURL(Application.persistentDataPath);
		}

		public void ReloadMenu()
		{
			SkUtilities.Logz(new string[3] { "BASE", "CMD", "REQ" }, new string[2] { "UNLOADING CONTROLLERS AND MODULES...", "SKTOOLBOX RELOAD REQUESTED." });
			SkLoader.Reload();
		}

		public void UnloadMenu()
		{
			SkUtilities.Logz(new string[3] { "BASE", "CMD", "REQ" }, new string[2] { "SKTOOLBOX UNLOAD REQUESTED.", "UNLOADING CONTROLLERS AND MODULES..." });
			SkLoader.Unload();
		}

		public void HandleConsole()
		{
			if (!(Console.instance != null))
			{
				return;
			}
			if (Console.instance.m_chatWindow.gameObject.activeInHierarchy)
			{
				string text = Console.instance.m_input.text;
				if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
				{
					consoleLastMessage = string.Empty;
				}
				if (!text.Equals(string.Empty) && !text.Equals(consoleLastMessage))
				{
					consoleLastMessage = text;
				}
				if (Input.GetKeyDown(KeyCode.Return) && text.Equals(string.Empty) && !consoleLastMessage.Equals(string.Empty))
				{
					consoleHistory.Add(consoleLastMessage);
					SkCommandProcessor.ProcessCommands(consoleLastMessage, SkCommandProcessor.LogTo.Console);
					consoleLastMessage = string.Empty;
				}
				if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					Console.instance.m_input.text = consoleHistory.Fetch(text, next: true);
					Console.instance.m_input.caretPosition = Console.instance.m_input.text.Length;
				}
				if (Input.GetKeyDown(KeyCode.DownArrow))
				{
					Console.instance.m_input.text = consoleHistory.Fetch(text, next: false);
				}
			}
			if (Input.GetKeyDown(KeyCode.Slash) && SkConfigEntry.COpenConsoleWithSlash != null && SkConfigEntry.COpenConsoleWithSlash.Value && !Console.IsVisible() && !Chat.instance.IsChatDialogWindowVisible() && !TextInput.IsVisible())
			{
				Console.instance.m_chatWindow.gameObject.SetActive(value: true);
				Console.instance.m_input.caretPosition = Console.instance.m_input.text.Length;
			}
		}

		public void HandleChat()
		{
			if (!(Chat.instance != null))
			{
				return;
			}
			if (Chat.instance.m_chatWindow.gameObject.activeInHierarchy)
			{
				string text = Chat.instance.m_input.text;
				_ = Chat.instance.m_output.text;
				if ((Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape)) && ((SkConfigEntry.CAllowExecuteOnClear != null && !SkConfigEntry.CAllowExecuteOnClear.Value) || SkConfigEntry.CAllowExecuteOnClear == null))
				{
					chatInLastMessage = string.Empty;
				}
				if (!text.Equals(string.Empty) && !text.Equals(consoleLastMessage))
				{
					chatInLastMessage = text;
				}
				if (text.Equals(string.Empty) && !chatInLastMessage.Equals(string.Empty))
				{
					SkCommandProcessor.ProcessCommands(chatInLastMessage, SkCommandProcessor.LogTo.Chat);
					chatInLastMessage = string.Empty;
				}
			}
			if (Input.GetKeyDown(KeyCode.Slash) && Player.m_localPlayer != null && SkConfigEntry.COpenConsoleWithSlash != null && !SkConfigEntry.COpenConsoleWithSlash.Value && SkConfigEntry.COpenChatWithSlash != null && SkConfigEntry.COpenChatWithSlash.Value && !Console.IsVisible() && !Chat.instance.IsChatDialogWindowVisible() && !TextInput.IsVisible() && !Minimap.InTextInput() && !Menu.IsVisible())
			{
				Chat.instance.SetPrivateField("m_hideTimer", 0f);
				Chat.instance.m_chatWindow.gameObject.SetActive(value: true);
				Chat.instance.m_input.gameObject.SetActive(value: true);
				Chat.instance.m_input.ActivateInputField();
			}
		}

		public void LoadConsoleCustomizations()
		{
			if (SkConfigEntry.CAllowLookCustomizations == null || !SkConfigEntry.CAllowLookCustomizations.Value)
			{
				return;
			}
			try
			{
				int num = Console.instance.m_output.fontSize;
				string fontname = "Consolas";
				Color color = Console.instance.m_output.color;
				Color color2 = Console.instance.m_input.textComponent.color;
				Color color3 = Console.instance.m_input.selectionColor;
				Color color4 = Color.white;
				try
				{
					num = SkConfigEntry.CConsoleFontSize.Value;
					fontname = SkConfigEntry.CConsoleFont.Value;
					ColorUtility.TryParseHtmlString(SkConfigEntry.CConsoleOutputTextColor.Value, out color);
					ColorUtility.TryParseHtmlString(SkConfigEntry.CConsoleInputTextColor.Value, out color2);
					ColorUtility.TryParseHtmlString(SkConfigEntry.CConsoleSelectionColor.Value, out color3);
					ColorUtility.TryParseHtmlString(SkConfigEntry.CConsoleCaretColor.Value, out color4);
				}
				catch (Exception ex)
				{
					SkUtilities.Logz(new string[1] { "ERR" }, new string[2] { "Failed to load something from the config.", ex.Message }, LogType.Warning);
				}
				Font font = Font.CreateDynamicFontFromOSFont(fontname, num);
				Console.instance.m_output.font = font;
				Console.instance.m_output.fontSize = num;
				Console.instance.m_output.color = color;
				Console.instance.m_input.textComponent.color = color2;
				Console.instance.m_input.textComponent.font = font;
				Console.instance.m_input.selectionColor = color3;
				Console.instance.m_input.caretColor = color4;
				Console.instance.m_input.customCaretColor = true;
			}
			catch (Exception ex2)
			{
				SkUtilities.Logz(new string[1] { "ERR" }, new string[2] { "Failed to set when customizing console style.", ex2.Message }, LogType.Warning);
			}
		}

		private void OnGUI()
		{
			if (Player.m_localPlayer != null)
			{
				if (SkCommandProcessor.bDetectEnemies && nearbyCharacters.Count > 0)
				{
					EnemyWindow = GUILayout.Window(39999, rectEnemy, ProcessEnemies, "Enemy Information");
				}
				if (SkCommandProcessor.bCoords)
				{
					Vector3 position = Player.m_localPlayer.transform.position;
					GUI.Label(rectCoords, "Coords: " + Mathf.RoundToInt(position.x) + "/" + Mathf.RoundToInt(position.z));
				}
			}
			if (FejdStartup.instance != null && FejdStartup.instance.m_creditsPanel != null && FejdStartup.instance.m_creditsPanel.activeInHierarchy)
			{
				GUILayout.BeginArea(new Rect(0f, Screen.height / 4, Screen.width, Screen.height));
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUIStyle gUIStyle = new GUIStyle();
				gUIStyle.fontStyle = FontStyle.Bold;
				gUIStyle.fontSize = 18;
				GUILayout.Label("<color=yellow>Skrip from NexusMods</color>", gUIStyle);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndArea();
			}
		}

		private void Update()
		{
			HandleConsole();
			if ((SkConfigEntry.CAllowChatCommandInput != null && SkConfigEntry.CAllowChatCommandInput.Value) || SkConfigEntry.CAllowChatCommandInput == null)
			{
				HandleChat();
			}
			if (!anncounced1)
			{
				if (Console.instance != null && Player.m_localPlayer == null)
				{
					SkCommandProcessor.Announce();
					LoadConsoleCustomizations();
					anncounced1 = true;
				}
			}
			else if (Player.m_localPlayer != null)
			{
				anncounced1 = false;
			}
			if (!anncounced2)
			{
				if (Chat.instance != null)
				{
					SkCommandProcessor.Announce();
					LoadConsoleCustomizations();
					anncounced2 = true;
				}
			}
			else if (Chat.instance == null)
			{
				anncounced2 = false;
			}
			if (!SkConfigEntry.CAutoRunComplete)
			{
				if (SkConfigEntry.CAutoRun != null && SkConfigEntry.CAutoRun.Value)
				{
					if (Player.m_localPlayer != null && Chat.instance != null && Console.instance != null)
					{
						try
						{
							SkUtilities.Logz(new string[2] { "CMD", "AUTORUN" }, new string[1] { "Command List:" + SkConfigEntry.CAutoRunCommand.Value });
							SkCommandProcessor.PrintOut("==> AutoRun enabled! Command line: " + SkConfigEntry.CAutoRunCommand.Value, SkCommandProcessor.LogTo.Console);
							SkCommandProcessor.ProcessCommands(SkConfigEntry.CAutoRunCommand.Value, SkCommandProcessor.LogTo.Console);
						}
						catch (Exception)
						{
							SkUtilities.Logz(new string[1] { "Console" }, new string[1] { "AutoRun Failed. Something went wrong. Command line: " + SkConfigEntry.CAutoRunCommand.Value }, LogType.Warning);
							SkCommandProcessor.PrintOut("==> AutoRun Failed. Something went wrong. Command line: " + SkConfigEntry.CAutoRunCommand.Value, SkCommandProcessor.LogTo.Console);
						}
						finally
						{
							SkConfigEntry.CAutoRunComplete = true;
						}
					}
				}
				else
				{
					SkConfigEntry.CAutoRunComplete = true;
				}
			}
			if (SkCommandProcessor.bTeleport && Input.GetKeyDown(KeyCode.BackQuote) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
			{
				Vector3 point = hitInfo.point;
				Player.m_localPlayer.transform.position = point;
				Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Warp!");
			}
			if (!SkCommandProcessor.bDetectEnemies)
			{
				return;
			}
			try
			{
				List<Character> allCharacters = Character.GetAllCharacters();
				if (allCharacters.Count > 0)
				{
					foreach (Character item in allCharacters)
					{
						if (item != null && !item.IsDead() && !item.IsPlayer())
						{
							if (Vector3.Distance(item.transform.position, Player.m_localPlayer.transform.position) < (float)SkCommandProcessor.bDetectRange)
							{
								if (!nearbyCharacters.Contains(item))
								{
									nearbyCharacters.Add(item);
								}
							}
							else if (nearbyCharacters.Contains(item))
							{
								nearbyCharacters.Remove(item);
							}
						}
						else if (nearbyCharacters.Contains(item))
						{
							nearbyCharacters.Remove(item);
						}
					}
					if (nearbyCharacters.Count > 0)
					{
						foreach (Character item2 in new List<Character>(nearbyCharacters))
						{
							if (!allCharacters.Contains(item2))
							{
								nearbyCharacters.Remove(item2);
							}
						}
					}
				}
				if (nearbyCharacters.Count > 0 && SkCommandProcessor.btDetectEnemiesSwitch)
				{
					Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Enemy nearby!");
					SkCommandProcessor.btDetectEnemiesSwitch = false;
				}
				else if (nearbyCharacters.Count == 0)
				{
					SkCommandProcessor.btDetectEnemiesSwitch = true;
				}
			}
			catch (Exception)
			{
			}
		}

		private void OnDestroy()
		{
			if (SkCommandPatcher.Harmony != null)
			{
				SkCommandPatcher.Harmony.UnpatchSelf();
			}
		}

		private void ProcessEnemies(int WindowID)
		{
			if (!(Player.m_localPlayer != null))
			{
				return;
			}
			GUILayout.BeginVertical();
			List<Character> list = nearbyCharacters;
			if (list != null && list.Count > 0)
			{
				Vector3 position = Player.m_localPlayer.transform.position;
				foreach (Character nearbyCharacter in nearbyCharacters)
				{
					float num = Vector3.Distance(position, nearbyCharacter.transform.position);
					if (num > 15f)
					{
						GUI.color = Color.green;
					}
					else if (num > 10f && num < 15f)
					{
						GUI.color = Color.yellow;
					}
					else if (num > 5f && num < 10f)
					{
						GUI.color = Color.yellow + Color.red;
					}
					else if (num > 0f && num < 5f)
					{
						GUI.color = Color.red;
					}
					GUILayout.BeginHorizontal();
					GUILayout.Label("Name: " + nearbyCharacter.GetHoverName());
					GUILayout.Label("HP: " + Mathf.RoundToInt(nearbyCharacter.GetHealth()) + "/" + nearbyCharacter.GetMaxHealth() + " | Level: " + nearbyCharacter.GetLevel() + " | Dist: " + Mathf.RoundToInt(num));
					GUILayout.EndHorizontal();
					GUI.color = Color.white;
				}
			}
			GUILayout.EndVertical();
			GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
		}
	}
}
