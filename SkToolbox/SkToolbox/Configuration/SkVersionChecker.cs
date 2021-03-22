using System;
using System.Net;
using UnityEngine;

namespace SkToolbox.Configuration
{
	internal static class SkVersionChecker
	{
		private static readonly string VersionURL = "https://pastebin.com/raw/ubRAdqxz";

		internal static System.Version currentVersion = new System.Version("1.8.3.0");

		internal static System.Version latestVersion = new System.Version("0.0.0.0");

		public static bool VersionCurrent()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				WebClient val = new WebClient();
				//val.get_Headers().Add("User-Agent: SkToolboxUser" + UnityEngine.Random.Range(0, 999999));
				latestVersion = new System.Version(val.DownloadString(VersionURL));
				if (latestVersion > currentVersion)
				{
					return false;
				}
				return true;
			}
			catch (Exception)
			{
				return true;
			}
		}
	}
}
