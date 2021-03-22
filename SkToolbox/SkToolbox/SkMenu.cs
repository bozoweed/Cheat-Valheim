using System;
using System.Collections.Generic;
using System.Linq;

namespace SkToolbox
{
	public class SkMenu
	{
		private List<SkMenuItem> listItems = new List<SkMenuItem>();

		private readonly string toggleStrOn = "[ON]";

		private readonly string toggleStrOff = "[OFF]";

		public void AddItem(string inText, Action outMethod, string inTip = null)
		{
			listItems.Add(new SkMenuItem(inText, outMethod, inTip));
		}

		public void AddItem(string inText, Action<string> outMethod, string inTip = null)
		{
			listItems.Add(new SkMenuItem(inText, outMethod, inTip));
		}

		public void AddItem(SkMenuItem menuItem)
		{
			listItems.Add(menuItem);
		}

		public void AddItemToggle(string inText, ref bool inToggleVar, Action outMethod, string inTip = null)
		{
			listItems.Add(new SkMenuItem(inText + " " + (inToggleVar ? toggleStrOn : toggleStrOff), outMethod, inTip));
		}

		public void AddItemToggle(string inText, ref bool inToggleVar, Action<string> outMethod, string inTip = null)
		{
			listItems.Add(new SkMenuItem(inText + " " + (inToggleVar ? toggleStrOn : toggleStrOff), outMethod, inTip));
		}

		public int RemoveItem(SkMenuItem menuItem)
		{
			int num = 0;
			try
			{
				foreach (SkMenuItem listItem in listItems)
				{
					if (listItem.Compare(menuItem))
					{
						listItems.Remove(menuItem);
						num++;
					}
				}
				return num;
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public List<SkMenuItem> FlushMenu()
		{
			return listItems;
		}

		public void ClearItems()
		{
			listItems.Clear();
		}

		public int Count()
		{
			return Enumerable.Count<SkMenuItem>((IEnumerable<SkMenuItem>)listItems);
		}
	}
}
