using System.Collections.Generic;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox.SkModules
{
	internal class SkBaseModule : MonoBehaviour
	{
		internal SkMenuController SkMC;

		internal string ModuleName = "UNNAMED";

		internal SkMenu MenuOptions { get; set; } = new SkMenu();


		internal SkMenuItem CallerEntry { get; set; } = new SkMenuItem();


		internal SkUtilities.Status ModuleStatus { get; set; }

		public void Awake()
		{
			SkMC = GetComponent<SkMenuController>();
		}

		public List<SkMenuItem> FlushMenu()
		{
			return MenuOptions.FlushMenu();
		}

		public void RequestMenu()
		{
			SkMC.RequestSubMenu(MenuOptions.FlushMenu());
		}

		public void RequestMenu(SkMenu Menu)
		{
			SkMC.RequestSubMenu(Menu);
		}

		public void RemoveModule()
		{
			Object.Destroy(this);
		}

		internal void Ready()
		{
			ModuleStatus = SkUtilities.Status.Ready;
		}

		internal void Loading()
		{
			ModuleStatus = SkUtilities.Status.Loading;
		}

		internal void Error()
		{
			ModuleStatus = SkUtilities.Status.Error;
		}

		internal void Unload()
		{
			ModuleStatus = SkUtilities.Status.Unload;
		}
	}
}
