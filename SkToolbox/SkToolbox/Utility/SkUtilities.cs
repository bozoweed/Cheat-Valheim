using System;
using System.Reflection;
using UnityEngine;

namespace SkToolbox.Utility
{
	internal static class SkUtilities
	{
		public enum Status
		{
			Initialized,
			Loading,
			Ready,
			Error,
			Unload
		}

		public static bool ConvertInternalWarningsErrors = false;

		public static BindingFlags BindFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public static void SetPrivateField(this object obj, string fieldName, object value)
		{
			obj.GetType().GetField(fieldName, BindFlags).SetValue(obj, value);
		}

		public static T GetPrivateField<T>(this object obj, string fieldName)
		{
			return (T)obj.GetType().GetField(fieldName, BindFlags).GetValue(obj);
		}

		public static void SetPrivateProperty(this object obj, string propertyName, object value)
		{
			obj.GetType().GetProperty(propertyName, BindFlags).SetValue(obj, value, null);
		}

		public static void InvokePrivateMethod(this object obj, string methodName, object[] methodParams)
		{
			obj.GetType().GetMethod(methodName, BindFlags).Invoke(obj, methodParams);
		}

		public static Component CopyComponent(Component original, Type originalType, Type overridingType, GameObject destination)
		{
			Component component = destination.AddComponent(overridingType);
			FieldInfo[] fields = originalType.GetFields(BindFlags);
			foreach (FieldInfo obj in fields)
			{
				object value = obj.GetValue(original);
				obj.SetValue(component, value);
			}
			return component;
		}

		public static void Logz(string[] categories, string[] messages, LogType logType = LogType.Log)
		{
			string text = string.Empty;
			if (categories != null)
			{
				string[] array = categories;
				foreach (string text2 in array)
				{
					text = text + " (" + text2 + ") -> ";
				}
			}
			if (messages != null)
			{
				string[] array = messages;
				foreach (string text3 in array)
				{
					text = ((text3 == null) ? (text + "NULL | ") : (text + text3 + " | "));
				}
				text = text.Remove(text.Length - 2, 1);
			}
			if (!ConvertInternalWarningsErrors)
			{
				switch (logType)
				{
				case LogType.Error:
					Debug.LogError("(SkToolbox) -> " + text);
					break;
				case LogType.Warning:
					Debug.LogWarning("(SkToolbox) -> " + text);
					break;
				default:
					Debug.Log("(SkToolbox) -> " + text);
					break;
				}
			}
			else
			{
				Debug.Log("(SkToolbox) -> " + text);
			}
		}

		public static string Logr(string[] categories, string[] messages)
		{
			string text = string.Empty;
			if (categories != null)
			{
				string[] array = categories;
				foreach (string text2 in array)
				{
					text = text + " (" + text2 + ") -> ";
				}
			}
			if (messages != null)
			{
				string[] array = messages;
				foreach (string text3 in array)
				{
					text = ((text3 == null) ? (text + "NULL | ") : (text + text3 + " | "));
				}
				text = text.Remove(text.Length - 2, 1);
			}
			return "(SkToolbox) -> " + text;
		}

		public static void Logz(string message)
		{
			string empty = string.Empty;
			empty += " (OUT) -> ";
			empty = empty + message + " ";
			Debug.Log("(SkToolbox) -> " + empty);
		}

		public static void RectFilled(float x, float y, float width, float height, Texture2D text)
		{
			GUI.DrawTexture(new Rect(x, y, width, height), text);
		}

		public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
		{
			RectFilled(x, y, thickness, height, text);
			RectFilled(x + width - thickness, y, thickness, height, text);
			RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
			RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
		}
	}
}
