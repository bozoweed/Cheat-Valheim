using System.Linq;
using System.Threading;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox
{
	public class SkLoader : MonoBehaviour
	{
		private static GameObject _SkGameObject;

		private static bool FirstLoad = true;

		private static bool InitLogging = false;

		public static GameObject Load
		{
			get
			{
				return _SkGameObject;
			}
			set
			{
				_SkGameObject = value;
			}
		}

		public static void Unload()
		{
			Object.Destroy(_SkGameObject, 0f);
			_SkGameObject = null;
		}

		public static void Reload()
		{
			Unload();
			Init();
		}

		private void Start()
		{
			Init();
		}

		public static void Main(string[] args)
		{
			InitThreading();
		}

		public static void InitThreading()
		{
			new Thread((ThreadStart)delegate
			{
				Thread.Sleep(5000);
				Init();
			}).Start();
		}

		public static void InitWithLog()
		{
			InitLogging = true;
			Init();
		}

		public static void Init()
		{
			_SkGameObject = new GameObject("SkToolbox");
			if (InitLogging)
			{
				InitLogging = false;
			}
			if (FirstLoad)
			{
				SkUtilities.Logz(new string[2] { "LOADER", "STARTUP" }, new string[1] { "SUCCESS!" });
			}
			CheckForUnknownInstance();
			Load.transform.parent = null;
			Transform parent = Load.transform.parent;
			if (parent != null && parent.gameObject != Load)
			{
				parent.parent = Load.transform;
			}
			_SkGameObject.AddComponent<SkMenuController>();
			Object.DontDestroyOnLoad(_SkGameObject);
			FirstLoad = false;
		}

		public static void CheckForUnknownInstance()
		{
			foreach (GameObject item in from obj in Resources.FindObjectsOfTypeAll<GameObject>()
				where obj.name == "SkToolbox"
				select obj)
			{
				if (item != _SkGameObject)
				{
					Object.Destroy(item);
					SkUtilities.Logz(new string[2] { "LOADER", "DETECT" }, new string[1] { "Other SkToolbox Destroyed." });
				}
			}
		}

		private void OnDestroy()
		{
			Unload();
		}
	}
}
