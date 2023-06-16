using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Hvak.Editor.Refleaction
{
	/// <summary>
	/// 通用的有返回值类型的委托
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public delegate object UniversalFunc(params object[] obj);

	/// <summary>
	/// 通用的无返回值类型的委托
	/// 不过感觉反射一般用上面那个就好了
	/// </summary>
	/// <param name="obj"></param>
	public delegate void UniversalAction(params object[] obj);

	public static class ReflectionUtils
	{
		static private Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

		public static Type GetType<T>()
		{
			return GetType(typeof(T));
		}

		public static Type GetType(Type t)
		{
			if (t.IsSubclassOf(typeof(RType)))
			{
				var typeProperty = t.GetProperty("Type", BindingFlags.Public | BindingFlags.Static);
				var result = typeProperty.GetValue(null) as Type;
				if(result != null)
				{
					return result;
				}
			}
			return t;
		}

		/// <summary>
		/// 获取类型
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public static Type GetType(string typeName)
		{
			Type type = null;
			if (_typeCache.TryGetValue(typeName, out type))
			{
				return type;
			}

			var names = typeName.Split("::");
			var aliasName = string.Empty;
			var fullName = string.Empty;
			if(names.Length >= 2)
			{
				aliasName = names[0];
				fullName = names[1];
			}
			else
			{
				aliasName = string.Empty;
				fullName = names[0];
			}
			

			Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
			int assemblyArrayLength = assemblyArray.Length;
			for (int i = 0; i < assemblyArrayLength; ++i)
			{
				type = assemblyArray[i].GetType(fullName);
				if (type == null)
				{
					continue;
				}
				if (!type.IsThisModule(aliasName))
				{
					continue;
				}
				_typeCache.Add(typeName, type);
				return type;
			}

			for (int i = 0; (i < assemblyArrayLength); ++i)
			{
				Type[] typeArray = assemblyArray[i].GetTypes();
				int typeArrayLength = typeArray.Length;
				for (int j = 0; j < typeArrayLength; ++j)
				{
					if (!typeArray[j].Name.Equals(fullName))
					{
						continue;
					}
					if (!typeArray[j].IsThisModule(aliasName))
					{
						continue;
					}
					_typeCache.Add(typeName, typeArray[j]);
					return typeArray[j];
				}
			}
			return type;
		}

		public static T Convert<T>(object reuslt)
		{
			if (typeof(RType).IsAssignableFrom(typeof(T)) && !typeof(RType).IsAssignableFrom(reuslt.GetType()))
			{
				T result = Activator.CreateInstance<T>();
				RType rt = result as RType;
				rt.SetInstance(reuslt);
				return result;
			}
			else
			{
				return (T)reuslt;
			}
		}

		

		public static void Log(object str)
		{
#if UNITY_EDITOR
			UnityEngine.Debug.Log(str);
#else
			Console.WriteLine(str);
#endif
		}

		public static void LogError(object str)
		{
#if UNITY_EDITOR
			UnityEngine.Debug.LogError(str);
#else
			Console.WriteLine("[Error]" + str);
#endif
		}
	}
}