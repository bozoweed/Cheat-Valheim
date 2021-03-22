using System;

namespace SkToolbox
{
	public class SkMenuItem
	{
		private string itemText;

		private Action itemClass;

		private Action<string> itemClassStr;

		private string itemTip;

		public string ItemText
		{
			get
			{
				return itemText;
			}
			set
			{
				itemText = value;
			}
		}

		public Action ItemClass
		{
			get
			{
				return itemClass;
			}
			set
			{
				itemClass = value;
			}
		}

		public string ItemTip
		{
			get
			{
				return itemTip;
			}
			set
			{
				itemTip = value;
			}
		}

		public Action<string> ItemClassStr
		{
			get
			{
				return itemClassStr;
			}
			set
			{
				itemClassStr = value;
			}
		}

		public SkMenuItem()
		{
		}

		public SkMenuItem(string itemText, Action itemClass, string itemTip = "")
		{
			ItemText = itemText;
			ItemClass = itemClass;
			ItemTip = itemTip;
		}

		public SkMenuItem(string itemText, Action<string> itemClassStr, string itemTip = "")
		{
			ItemText = itemText;
			ItemClassStr = itemClassStr;
			ItemTip = itemTip;
		}

		public bool Compare(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			SkMenuItem skMenuItem;
			if ((skMenuItem = obj as SkMenuItem) != null)
			{
				if (ItemText.Equals(skMenuItem?.ItemText))
				{
					return ItemClass.Equals(skMenuItem?.ItemClass);
				}
				return false;
			}
			return false;
		}
	}
}
