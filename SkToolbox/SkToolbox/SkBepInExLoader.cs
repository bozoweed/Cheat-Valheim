using System;
using BepInEx;
using SkToolbox.Configuration;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox
{
	[BepInPlugin(GUID, "SkToolbox", "1.8.2.0")]
	internal class SkBepInExLoader : BaseUnityPlugin
	{
		public const string MODNAME = "SkToolbox";

		public const string AUTHOR = "Skrip";

		public const string GUID = "org.bepinex.plugins.valheim_plus";

		public const string VERSION = "1.8.2.0";

		private void Start()
		{
			InitConfig();
			base.transform.parent = null;
			UnityEngine.Object.DontDestroyOnLoad(this);
			SkLoader.Init();
		}

		private void InitConfig()
		{
			try
			{
				SkConfigEntry.CAllowChatCommandInput = base.Config.Bind("General", "AllowChatCommandInput", defaultValue: true, "Toggle this if you want to allow or disable the entry of commands in the chat. If this is disabled, you can only input commands into the console.");
				SkConfigEntry.CAllowPublicChatOutput = base.Config.Bind("General", "AllowPublicResponse", defaultValue: true, "Toggle this to allow the mod to respond publicly with certain commands, when entered into chat.\nThe /portal command for example, if used in chat and this is true, others nearby will be able to see the response.\nNOTE: If you see a response from your name, it is shown publicly and everyone can see it. If it is a response from (SkToolbox), only you see it.");
				SkConfigEntry.CAllowExecuteOnClear = base.Config.Bind("General", "AllowExecuteOnClear", defaultValue: false, "Toggle this to enable the ability to execute commands by clearing the input (by hitting escape, or selecting all and removing).");
				SkConfigEntry.COpenConsoleWithSlash = base.Config.Bind("General", "OpenConsoleWithSlash", defaultValue: false, "Toggle this to enable the ability to open the console with the slash (/) button.\nThis option takes precedence over OpenChatWithSlash if both are true.");
				SkConfigEntry.COpenChatWithSlash = base.Config.Bind("General", "OpenChatWithSlash", defaultValue: false, "Toggle this to enable the ability to open chat with the slash (/) button.");
				SkConfigEntry.CAutoRun = base.Config.Bind("AutoRun", "AutoRunEnabled", defaultValue: false, "Toggle this to run commands automatically upon spawn into server.\nNote this will only occur once per game launch to prevent unintended command executions.");
				SkConfigEntry.CAutoRunCommand = base.Config.Bind("AutoRun", "AutoRunCommand", "/nosup; /god", "Enter the commands to run upon spawn here. Seperate commands with a semicolon (;).");
				SkConfigEntry.OAltToggle = base.Config.Bind("OnScreenMenu", "AltKeyToggle", "Home", "If /alt has been used (in-game or via auto run), the on screen menu will use this bind for toggling the menu.\nValid key codes are found here: https://docs.unity3d.com/ScriptReference/KeyCode.html");
				SkConfigEntry.OAltUp = base.Config.Bind("OnScreenMenu", "AltKeyUp", "PageUp", "If /alt has been used (in-game or via auto run), the on screen menu will use this bind for changing the on-screen menu selection upwards.");
				SkConfigEntry.OAltDown = base.Config.Bind("OnScreenMenu", "AltKeyDown", "PageDown", "If /alt has been used (in-game or via auto run), the on screen menu will use this bind for changing the on-screen menu selection downwards.");
				SkConfigEntry.OAltChoose = base.Config.Bind("OnScreenMenu", "AltKeyChoose", "Insert", "If /alt has been used (in-game or via auto run), the on screen menu will use this bind for choosing the selected on-screen menu option.");
				SkConfigEntry.OAltBack = base.Config.Bind("OnScreenMenu", "AltKeyBack", "Delete", "If /alt has been used (in-game or via auto run), the on screen menu will use this bind for going back in the on-screen menu.");
				SkConfigEntry.CAllowLookCustomizations = base.Config.Bind("CustomizeConsoleLook", "ConsoleAllowLookCustomizations", defaultValue: true, "Toggle this to enable or disable the customization settings below.");
				SkConfigEntry.CConsoleFont = base.Config.Bind("CustomizeConsoleLook", "ConsoleFont", "Consolas", "Set the font size of the text in the console. Game default = AveriaSansLibre-Bold");
				SkConfigEntry.CConsoleFontSize = base.Config.Bind("CustomizeConsoleLook", "ConsoleFontSize", 18, "Set the font size of the text in the console. Game default = 18");
				SkConfigEntry.CConsoleOutputTextColor = base.Config.Bind("CustomizeConsoleLook", "ConsoleOutputTextColor", "#E6F7FFFF", "Set the color of the output text shown in the console. Game default = #FFFFFFFF. Color format is #RRGGBBAA");
				SkConfigEntry.CConsoleInputTextColor = base.Config.Bind("CustomizeConsoleLook", "ConsoleInputTextColor", "#E6F7FFFF", "Set the color of the input text shown in the console. Game default = #FFFFFFFF. Color format is #RRGGBBAA");
				SkConfigEntry.CConsoleSelectionColor = base.Config.Bind("CustomizeConsoleLook", "ConsoleSelectionColor", "#A8CEFFC0", "Set the color of the selection highlight in the console. Game default = #A8CEFFC0. Color format is #RRGGBBAA");
				SkConfigEntry.CConsoleCaretColor = base.Config.Bind("CustomizeConsoleLook", "ConsoleCaretColor", "#DCE6F5FF", "Set the color of the input text caret shown in the console. Game default = #FFFFFFFF. Color format is #RRGGBBAA");
			}
			catch (Exception ex)
			{
				SkUtilities.Logz(new string[1] { "ERR" }, new string[3] { "Could not load config. Please confirm there is a working version of BepInEx installed.", ex.Message, ex.Source }, LogType.Error);
			}
		}
	}
}
