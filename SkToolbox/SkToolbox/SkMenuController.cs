using System;
using System.Collections;
using System.Collections.Generic;
using SkToolbox.Configuration;
using SkToolbox.SkModules;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox
{
	internal class SkMenuController : MonoBehaviour
	{
		private string contextTipInfo1 = "NumPad Arrows";

		private string contextTipInfo2 = "NumPad 5 to Select";

		private string contextTipInfo3 = "NumPad . to Back";

		internal static System.Version SkMenuControllerVersion = new System.Version(1, 1, 4);

		internal static SkUtilities.Status SkMenuControllerStatus = SkUtilities.Status.Initialized;

		private readonly string appName = "SkToolbox";

		private readonly string welcomeMsg = "[SkToolbox Loaded]\nPress NumPad 0\nto Toggle Menu.";

		private readonly string welcomeMotd = "";

		private bool firstRun;

		private bool InitialCheck = true;

		internal bool logResponse;

		private bool MenuOpen;

		private bool SubMenuOpen;

		private bool menuProcessInitialOptSize = true;

		private bool subMenuProcessInitialOptSize = true;

		private int MenuSelection = 1;

		private int SubMenuSelection = 1;

		private int maxSelectionOption;

		private int subMaxSelectionOption = 1;

		private int subMenuMaxItemsPerPage = 12;

		private int subMenuCurrentPage = 1;

		public List<SkBaseModule> MenuOptions;

		public List<SkMenuItem> SubMenuOptions;

		public List<SkMenuItem> SubMenuOptionsDisplay;

		private SkModuleController SkModuleController;

		private int ypos_initial;

		private int ypos_offset = 22;

		private int mWidth;

		private int subMenu_xpos_offset = 35;

		private int sWidth;

		private Color menuOptionHighlight = Color.cyan;

		private Color menuOptionSelected = Color.yellow;

		internal Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>
		{
			{
				"selToggle",
				KeyCode.Keypad0
			},
			{
				"selUp",
				KeyCode.Keypad8
			},
			{
				"selDown",
				KeyCode.Keypad2
			},
			{
				"selChoose",
				KeyCode.Keypad5
			},
			{
				"selBack",
				KeyCode.KeypadPeriod
			}
		};

		private void Start()
		{
			SkMenuControllerStatus = SkUtilities.Status.Loading;
			SkUtilities.Logz(new string[2] { "CONTROLLER", "NOTIFY" }, new string[2] { "LOADING...", "WAITING FOR TOOLBOX." });
			SkModuleController = base.gameObject.AddComponent<SkModuleController>();
		}

		private void Update()
		{
			if (SkCommandProcessor.altOnScreenControls)
			{
				if (SkConfigEntry.OAltToggle != null && SkConfigEntry.OAltUp != null && SkConfigEntry.OAltDown != null && SkConfigEntry.OAltChoose != null && SkConfigEntry.OAltBack != null)
				{
					contextTipInfo1 = SkConfigEntry.OAltToggle.Value + "/" + SkConfigEntry.OAltUp.Value + "/" + SkConfigEntry.OAltDown.Value;
					contextTipInfo2 = SkConfigEntry.OAltChoose.Value + " Button";
					contextTipInfo3 = SkConfigEntry.OAltBack.Value + " Button";
					keyBindings["selToggle"] = (KeyCode)Enum.Parse(typeof(KeyCode), SkConfigEntry.OAltToggle.Value);
					keyBindings["selUp"] = (KeyCode)Enum.Parse(typeof(KeyCode), SkConfigEntry.OAltUp.Value);
					keyBindings["selDown"] = (KeyCode)Enum.Parse(typeof(KeyCode), SkConfigEntry.OAltDown.Value);
					keyBindings["selChoose"] = (KeyCode)Enum.Parse(typeof(KeyCode), SkConfigEntry.OAltChoose.Value);
					keyBindings["selBack"] = (KeyCode)Enum.Parse(typeof(KeyCode), SkConfigEntry.OAltBack.Value);
				}
				else
				{
					contextTipInfo1 = "Home/PgUp/PgDn";
					contextTipInfo2 = "Insert Button";
					contextTipInfo3 = "Delete Button";
					keyBindings["selToggle"] = KeyCode.Home;
					keyBindings["selUp"] = KeyCode.PageUp;
					keyBindings["selDown"] = KeyCode.PageDown;
					keyBindings["selChoose"] = KeyCode.Insert;
					keyBindings["selBack"] = KeyCode.Delete;
				}
			}
			else
			{
				contextTipInfo1 = "NumPad Arrows";
				contextTipInfo2 = "NumPad 5 to Select";
				contextTipInfo3 = "NumPad . to Back";
				keyBindings["selToggle"] = KeyCode.Keypad0;
				keyBindings["selUp"] = KeyCode.Keypad8;
				keyBindings["selDown"] = KeyCode.Keypad2;
				keyBindings["selChoose"] = KeyCode.Keypad5;
				keyBindings["selBack"] = KeyCode.KeypadPeriod;
			}
			if (InitialCheck)
			{
				List<SkBaseModule> menuOptions = MenuOptions;
				if (menuOptions != null && menuOptions.Count == 0)
				{
					UpdateMenuOptions(SkModuleController.GetOptions());
				}
				else
				{
					SkMenuControllerStatus = SkUtilities.Status.Ready;
					if (SkMenuControllerStatus == SkUtilities.Status.Ready && SkModuleController.SkMainStatus == SkUtilities.Status.Ready)
					{
						InitialCheck = false;
						SkUtilities.Logz(new string[2] { "CONTROLLER", "NOTIFY" }, new string[1] { "READY." });
					}
				}
			}
			if (Input.GetKeyDown(keyBindings["selToggle"]) && !Console.IsVisible() && !Chat.instance.m_input.isFocused)
			{
				firstRun = false;
				MenuOpen = !MenuOpen;
			}
			if (!MenuOpen)
			{
				return;
			}
			if (!SubMenuOpen)
			{
				if (Input.GetKeyDown(keyBindings["selDown"]))
				{
					SubMenuSelection = 1;
					if (MenuSelection != maxSelectionOption)
					{
						MenuSelection++;
					}
					else
					{
						MenuSelection = 1;
					}
				}
				if (Input.GetKeyDown(keyBindings["selUp"]))
				{
					SubMenuSelection = 1;
					if (MenuSelection != 1)
					{
						MenuSelection--;
					}
					else
					{
						MenuSelection = maxSelectionOption;
					}
				}
				if (Input.GetKeyDown(keyBindings["selChoose"]))
				{
					try
					{
						RunMethod(MenuOptions[MenuSelection - 1].CallerEntry.ItemClass);
					}
					catch (Exception ex)
					{
						SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { ex.Message });
					}
				}
			}
			else
			{
				if (Input.GetKeyDown(keyBindings["selDown"]))
				{
					if (SubMenuSelection != subMaxSelectionOption)
					{
						SubMenuSelection++;
					}
					else
					{
						SubMenuSelection = 1;
					}
				}
				if (Input.GetKeyDown(keyBindings["selUp"]))
				{
					if (SubMenuSelection != 1)
					{
						SubMenuSelection--;
					}
					else
					{
						SubMenuSelection = subMaxSelectionOption;
					}
				}
				if (Input.GetKeyDown(keyBindings["selChoose"]))
				{
					SubMenuOpen = false;
					try
					{
						RunMethod(SubMenuOptionsDisplay[SubMenuSelection - 1].ItemClass);
					}
					catch (Exception)
					{
						try
						{
							if (SubMenuOptionsDisplay[SubMenuSelection - 1].ItemText.Equals("Next >"))
							{
								IncreasePage();
							}
							else if (SubMenuOptionsDisplay[SubMenuSelection - 1].ItemText.Equals("< Previous"))
							{
								DecreasePage();
							}
							else
							{
								RunMethod(SubMenuOptionsDisplay[SubMenuSelection - 1].ItemClassStr, SubMenuOptionsDisplay[SubMenuSelection - 1].ItemText);
							}
						}
						catch (Exception ex2)
						{
							SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { ex2.Message });
						}
					}
				}
			}
			if (Input.GetKeyDown(keyBindings["selBack"]))
			{
				if (!SubMenuOpen)
				{
					MenuOpen = false;
				}
				SubMenuOpen = false;
			}
		}

		private void OnGUI()
		{
			GUI.color = Color.white;
			if (firstRun)
			{
				GUILayout.Window(49101, new Rect(7f, ypos_initial, 150f, 55f), ProcessWelcome, "");
			}
			if (MenuOptions == null || MenuOptions.Count == 0 || ypos_initial == 0)
			{
				UpdateMenuOptions(SkModuleController.GetOptions());
			}
			else
			{
				if (!MenuOpen)
				{
					return;
				}
				if (menuProcessInitialOptSize)
				{
					float num = 0f;
					GUIStyle box = GUI.skin.box;
					foreach (SkBaseModule menuOption in MenuOptions)
					{
						GUIContent content = new GUIContent(menuOption.CallerEntry.ItemText);
						Vector2 vector = box.CalcSize(content);
						if (vector.x > num)
						{
							num = vector.x;
						}
					}
					mWidth = ((mWidth == 0) ? Mathf.CeilToInt(num) : mWidth);
					mWidth = Mathf.Clamp(mWidth, 125, 1024);
					menuProcessInitialOptSize = false;
				}
				GUILayout.Window(49000, new Rect(7f, ypos_initial, mWidth + ypos_offset, 30 + ypos_offset * MenuOptions.Count), ProcessMainMenu, "- [" + appName + "] -");
				if (!SubMenuOpen)
				{
					return;
				}
				if (subMenuProcessInitialOptSize)
				{
					float num2 = 0f;
					GUIStyle box2 = GUI.skin.box;
					foreach (SkMenuItem subMenuOption in SubMenuOptions)
					{
						GUIContent content2 = new GUIContent(subMenuOption.ItemText);
						Vector2 vector2 = box2.CalcSize(content2);
						if (vector2.x > num2)
						{
							num2 = vector2.x;
						}
					}
					sWidth = ((sWidth == 0) ? Mathf.CeilToInt(num2) : sWidth);
					sWidth = Mathf.Clamp(sWidth, 105, 1024);
				}
				if (SubMenuOptions.Count > subMenuMaxItemsPerPage)
				{
					GUILayout.Window(49001, new Rect(mWidth + subMenu_xpos_offset, ypos_initial - ypos_offset, sWidth + subMenu_xpos_offset, 30 + ypos_offset * subMenuMaxItemsPerPage), ProcessSubMenu, "- Submenu - " + subMenuCurrentPage + "/" + (Mathf.Ceil(SubMenuOptions.Count / subMenuMaxItemsPerPage) + (float)((SubMenuOptions.Count % subMenuMaxItemsPerPage != 0) ? 1 : 0)));
				}
				else
				{
					GUILayout.Window(49001, new Rect(mWidth + subMenu_xpos_offset, ypos_initial - ypos_offset, sWidth + subMenu_xpos_offset, 30 + ypos_offset * SubMenuOptions.Count), ProcessSubMenu, "- Submenu -");
				}
			}
		}

		private void ProcessMainMenu(int windowID)
		{
			try
			{
				ProcessMainMenu();
				ProcessContextMenu(windowID);
			}
			catch (Exception ex)
			{
				SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { ex.Message });
			}
		}

		private void ProcessSubMenu(int windowID)
		{
			try
			{
				ProcessSubMenu(SubMenuOptions);
			}
			catch (Exception ex)
			{
				SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { ex.Message });
			}
		}

		private void ProcessContextMenu(int windowID)
		{
			try
			{
				ProcessMenuTip();
			}
			catch (Exception ex)
			{
				SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { ex.Message });
			}
		}

		private void ProcessWelcome(int windowID)
		{
			try
			{
				GUILayout.BeginVertical();
				GUILayout.Label(welcomeMsg);
				if (!welcomeMotd.Equals(""))
				{
					GUILayout.Label(welcomeMotd);
				}
				GUILayout.EndVertical();
			}
			catch (Exception ex)
			{
				SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { ex.Message });
			}
		}

		private void ProcessMainMenu()
		{
			GUIStyle style = new GUIStyle(GUI.skin.box);
			GUILayout.BeginVertical();
			for (int i = 0; i < MenuOptions.Count; i++)
			{
				if (i == MenuSelection - 1)
				{
					if (SubMenuOpen)
					{
						GUI.color = menuOptionSelected;
					}
					else
					{
						GUI.color = menuOptionHighlight;
					}
				}
				else
				{
					GUI.color = Color.white;
				}
				GUILayout.Label(MenuOptions[i].CallerEntry.ItemText, style);
			}
			GUI.color = Color.white;
			GUILayout.EndVertical();
		}

		public void IncreasePage()
		{
			subMenuCurrentPage++;
			if (subMenuCurrentPage == 2)
			{
				SubMenuSelection++;
			}
			SubMenuOpen = true;
		}

		public void DecreasePage()
		{
			subMenuCurrentPage--;
			SubMenuOpen = true;
		}

		public void RequestSubMenu(List<SkMenuItem> subMenuOptions, float refreshTime = 0f, int subWidth = 0)
		{
			if (subMenuOptions != null && subMenuOptions.Count != 0)
			{
				subMenuCurrentPage = 1;
				sWidth = subWidth;
				SubMenuOpen = true;
				subMenuProcessInitialOptSize = true;
				SubMenuOptions = subMenuOptions;
				subMaxSelectionOption = subMenuOptions.Count;
				if (SubMenuSelection > subMenuOptions.Count)
				{
					SubMenuSelection = 1;
				}
				if (refreshTime > 0f)
				{
					refreshTime = Mathf.Clamp(refreshTime, 0.01f, 5f);
					StartCoroutine(RealTimeMenuUpdate(refreshTime));
				}
				else if (logResponse)
				{
					SkUtilities.Logz(new string[2] { "CONTROLLER", "RESP" }, new string[1] { "Submenu created." });
				}
			}
		}

		public void RequestSubMenu(SkMenu subMenuOptions, float refreshTime = 0f, int subWidth = 0)
		{
			if (subMenuOptions != null)
			{
				RequestSubMenu(subMenuOptions.FlushMenu(), refreshTime, subWidth);
			}
		}

		private void ProcessSubMenu(List<SkMenuItem> subMenuOptions)
		{
			if (!SubMenuOpen)
			{
				return;
			}
			GUIStyle style = new GUIStyle(GUI.skin.box);
			if (subMenuOptions.Count > subMenuMaxItemsPerPage)
			{
				List<SkMenuItem> list = new List<SkMenuItem>();
				if (subMenuCurrentPage > 1)
				{
					list.Add(new SkMenuItem("◄\tPrevious Page", (Action)delegate
					{
						DecreasePage();
					}, "Previous Page"));
				}
				try
				{
					for (int i = subMenuMaxItemsPerPage * subMenuCurrentPage - subMenuMaxItemsPerPage; i < ((subMenuOptions.Count > subMenuMaxItemsPerPage * (subMenuCurrentPage + 1) - subMenuMaxItemsPerPage) ? (subMenuMaxItemsPerPage * (subMenuCurrentPage + 1) - subMenuMaxItemsPerPage) : subMenuOptions.Count); i++)
					{
						list.Add(subMenuOptions[i]);
					}
				}
				catch (IndexOutOfRangeException)
				{
					for (int j = subMenuMaxItemsPerPage * subMenuCurrentPage - subMenuMaxItemsPerPage; j < subMenuOptions.Count - 1; j++)
					{
						list.Add(subMenuOptions[j]);
					}
					subMenuProcessInitialOptSize = true;
				}
				if (subMenuOptions.Count > subMenuMaxItemsPerPage * (subMenuCurrentPage + 1) - subMenuMaxItemsPerPage)
				{
					list.Add(new SkMenuItem("Next Page\t►", (Action)delegate
					{
						IncreasePage();
					}, "Next Page"));
				}
				subMenuOptions = list;
				SubMenuOptionsDisplay = list;
				subMaxSelectionOption = subMenuOptions.Count;
				if (SubMenuSelection > subMenuOptions.Count)
				{
					SubMenuSelection = 1;
				}
			}
			else
			{
				SubMenuOptionsDisplay = subMenuOptions;
			}
			GUILayout.BeginVertical();
			for (int k = 0; k < subMenuOptions.Count; k++)
			{
				if (k == SubMenuSelection - 1)
				{
					GUI.color = menuOptionHighlight;
				}
				else
				{
					GUI.color = Color.white;
				}
				GUILayout.Label(subMenuOptions[k].ItemText, style);
			}
			GUILayout.EndVertical();
			GUI.color = Color.white;
		}

		private void ProcessMenuTip()
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
			gUIStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.BeginVertical();
			if (SubMenuOpen)
			{
				if (SubMenuOptionsDisplay[SubMenuSelection - 1].ItemTip != null && !SubMenuOptionsDisplay[SubMenuSelection - 1].ItemTip.Equals(""))
				{
					GUILayout.Label("- Context -", gUIStyle);
					GUILayout.Label(SubMenuOptionsDisplay[SubMenuSelection - 1].ItemTip);
				}
			}
			else if (MenuOptions[MenuSelection - 1].CallerEntry.ItemTip != null && !MenuOptions[MenuSelection - 1].CallerEntry.ItemTip.Equals(""))
			{
				GUILayout.Label("- Context -", gUIStyle);
				GUILayout.Label(MenuOptions[MenuSelection - 1].CallerEntry.ItemTip);
			}
			GUILayout.Label("- Controls -", gUIStyle);
			GUILayout.Label(contextTipInfo1);
			GUILayout.Label(contextTipInfo2);
			if (SubMenuOpen)
			{
				GUILayout.Label(contextTipInfo3);
			}
			GUILayout.EndVertical();
			GUI.color = Color.white;
		}

		private IEnumerator RealTimeMenuUpdate(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			if (SubMenuOpen)
			{
				RunMethod(SubMenuOptions[SubMenuSelection - 1].ItemClass);
			}
		}

		public void UpdateMenuOptions(List<SkBaseModule> newMenuOptions)
		{
			SubMenuOpen = false;
			MenuOpen = false;
			MenuSelection = 1;
			SubMenuSelection = 1;
			MenuOptions = newMenuOptions;
			List<SkBaseModule> menuOptions = MenuOptions;
			if (menuOptions != null && menuOptions.Count > 0)
			{
				ypos_initial = Screen.height / 2 - MenuOptions.Count / 2 * ypos_offset;
				maxSelectionOption = MenuOptions.Count;
			}
			menuProcessInitialOptSize = true;
		}

		private void RunMethod(Action methodName)
		{
			methodName();
		}

		private void RunMethod(Action<string> methodName, string methodParameter = "")
		{
			try
			{
				methodName?.Invoke(methodParameter);
			}
			catch (Exception ex)
			{
				SkUtilities.Logz(new string[2] { "CONTROLLER", "ERROR" }, new string[1] { "Error running method. Likely not found... " + ex.Message }, LogType.Error);
			}
		}
	}
}
