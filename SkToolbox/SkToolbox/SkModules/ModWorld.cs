namespace SkToolbox.SkModules
{
	internal class ModWorld : SkBaseModule, IModule
	{
		private int radius = 5;

		private int height = 5;

		public ModWorld()
		{
			ModuleName = "World";
			Loading();
		}

		public void Start()
		{
			BeginMenu();
			Ready();
		}

		public void BeginMenu()
		{
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("List Portals", ListPortals);
			skMenu.AddItem("Reveal all Map", ExplorMap);
			skMenu.AddItem("Remove Drops", RemoveDrops);
			skMenu.AddItem("T - Level - " + radius, TLevel, "Current radius: " + radius);
			skMenu.AddItem("T - Raise - " + radius + " - " + height, TRaise, "Current radius: " + radius + "\nCurrent height: " + height);
			skMenu.AddItem("T - Dig - " + radius + " - " + height, TLower, "Current radius: " + radius + "\nCurrent height: " + height);
			skMenu.AddItem("T - Reset - " + radius + " - " + height, TReset, "Current radius: " + radius + "\nCurrent height: " + height);
			skMenu.AddItem("Set T Radius\t►", BeginRadiusMenu, "Current: " + radius);
			skMenu.AddItem("Set T Height\t►", BeginHeightMenu, "Current: " + height);
			base.MenuOptions = skMenu;
		}

		public void BeginRadiusMenu()
		{
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("5", SetRadius, "Current: " + radius);
			skMenu.AddItem("7", SetRadius, "Current: " + radius);
			skMenu.AddItem("10", SetRadius, "Current: " + radius);
			skMenu.AddItem("20", SetRadius, "Current: " + radius);
			skMenu.AddItem("30", SetRadius, "Current: " + radius);
			skMenu.AddItem("40", SetRadius, "Current: " + radius);
			RequestMenu(skMenu);
		}

		public void SetRadius(string ln = "")
		{
			radius = int.Parse(ln);
			BeginMenu();
		}

		public void BeginHeightMenu()
		{
			SkMenu skMenu = new SkMenu();
			skMenu.AddItem("1", SetHeight, "Current: " + height);
			skMenu.AddItem("2", SetHeight, "Current: " + height);
			skMenu.AddItem("3", SetHeight, "Current: " + height);
			skMenu.AddItem("5", SetHeight, "Current: " + height);
			skMenu.AddItem("6", SetHeight, "Current: " + height);
			skMenu.AddItem("8", SetHeight, "Current: " + height);
			RequestMenu(skMenu);
		}

		public void SetHeight(string ln = "")
		{
			height = int.Parse(ln);
			BeginMenu();
		}

		public void ListPortals()
		{
			SkCommandProcessor.ProcessCommand("/portals", SkCommandProcessor.LogTo.Chat);
		}
		
		public void ExplorMap()
		{
			SkCommandProcessor.ProcessCommand("/revealmap", SkCommandProcessor.LogTo.Chat);
		}

		public void RemoveDrops()
		{
			SkCommandProcessor.ProcessCommand("/removedrops", SkCommandProcessor.LogTo.Chat);
		}

		public void TLevel()
		{
			SkCommandProcessor.ProcessCommand("/tl " + radius, SkCommandProcessor.LogTo.Chat);
		}

		public void TRaise()
		{
			SkCommandProcessor.ProcessCommand("/tr " + radius + " " + height, SkCommandProcessor.LogTo.Chat);
		}

		public void TLower()
		{
			SkCommandProcessor.ProcessCommand("/td " + radius + " " + height, SkCommandProcessor.LogTo.Chat);
		}

		public void TReset()
		{
			SkCommandProcessor.ProcessCommand("/tu " + radius + " " + height, SkCommandProcessor.LogTo.Chat);
		}
	}
}
