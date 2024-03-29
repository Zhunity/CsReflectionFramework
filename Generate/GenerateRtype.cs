using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hvak.Editor.Refleaction
{
	/// <summary>
	/// 生成RType的入口类型
	/// </summary>
	public static class GenerateRtype
	{
		/// <summary>
		/// 生成代码的文件夹路径
		/// </summary>
		public static string UnityCSReflectionPath;

		public static string GenerateDirectory
		{
			get
			{
				return $"{UnityCSReflectionPath}Generate/";
			}
		}

		private static Queue<Type> _waitToGenerate = new Queue<Type>();
		static HashSet<Type> _cacheType = new HashSet<Type>();
		public static void AddGenerateClass(Type type)
		{
			_waitToGenerate.Enqueue(type);
		}

		public static void GenerateClasses()
		{
			int i = 0;
			while (_waitToGenerate.Count > 0)
			{
				i++;
				Type type = _waitToGenerate.Dequeue();
				if (type == null)
				{
					continue;
				}
#if UNITY_EDITOR
				if (EditorUtility.DisplayCancelableProgressBar("生成文件", $"已生成{i}，正在生成{type.FullName}, 剩余{_waitToGenerate.Count}", (float)i / (float)_waitToGenerate.Count))
				{
					break;
				}
#else
				Console.WriteLine($"已生成{i}，正在生成{type.GetFullName()}, 剩余{_waitToGenerate.Count}");
#endif
				try
				{
					if (IsPrimitive(type) || _cacheType.Contains(type))
					{
						continue;
					}
					_cacheType.Add(type);
					GenerateInternal(type);
				}
				catch (Exception e)
				{
					ReflectionUtils.LogError(type + "\n" + e.ToString());
				}
			}
#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
#endif
		}

		#region 生成单个
		public static void Generate(Type classType, bool refType = true)
		{
			_waitToGenerate.Clear();
			_cacheType.Clear();
			string jsonFile = UnityCSReflectionPath + "Config/Replace.txt";
			LegalNameConfig.LoadReplace(jsonFile);
			GenerateInternal(classType, refType);
			if (refType)
			{
				GenerateClasses();
			}
			LegalNameConfig.SaveReplace(jsonFile);
#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif
		}

		public static void Generate<T>()
		{
			Generate(typeof(T));
		}

		public static void Generate(string className)
		{
			Generate(ReflectionUtils.GetType(className));
		}

		public static void Generate(object instance)
		{
			Generate(instance.GetType());
		}
		#endregion

		#region 生成多个
		/// <summary>
		/// 根据传进来的类型，生成指定的R类
		/// </summary>
		/// <param name="types"></param>
		public static void Generate(IEnumerable<Type> types)
		{
			ClearGenerateDirectory();
			_waitToGenerate.Clear();
			_cacheType.Clear();
			string jsonFile = UnityCSReflectionPath + "Config/Replace.txt";
			LegalNameConfig.LoadReplace(jsonFile);
			foreach (var type in types)
			{
				AddGenerateClass(type);
			}
			GenerateClasses();
			LegalNameConfig.SaveReplace(jsonFile);
#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif
		}

		/// <summary>
		/// 根据传进来的类型名字，生成指定的R类
		/// </summary>
		/// <param name="types"></param>
		public static void Generate(IEnumerable<string> types)
		{
			ClearGenerateDirectory();
			_waitToGenerate.Clear();
			_cacheType.Clear();
			string jsonFile = UnityCSReflectionPath + "Config/Replace.txt";
			LegalNameConfig.LoadReplace(jsonFile);
			foreach (var type in types)
			{
				AddGenerateClass(ReflectionUtils.GetType(type));
			}
			GenerateClasses();
			LegalNameConfig.SaveReplace(jsonFile);
#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif
		}

		/// <summary>
		/// 根据传进来的实例object，生成指定的R类
		/// 传进来的是Type，就是该Type的R类型
		/// 传进来的是string，就是该名字的R类型
		/// 其他类型则GetType，再生成该类型
		/// </summary>
		/// <param name="objs"></param>
		public static void Generate(IEnumerable<object> objs)
		{
			ClearGenerateDirectory();
			_waitToGenerate.Clear();
			_cacheType.Clear();
			string jsonFile = UnityCSReflectionPath + "Config/Replace.txt";
			LegalNameConfig.LoadReplace(jsonFile);
			foreach (var obj in objs)
			{
				Type type;
				switch (obj)
				{
					case Type t:
						type = t;
						break;
					case string name:
						type = ReflectionUtils.GetType(name);
						break;
					default:
						type = obj.GetType();
						break;
				}
				AddGenerateClass(type);
			}
			GenerateClasses();
			LegalNameConfig.SaveReplace(jsonFile);
#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif
		}
		#endregion

		public static void GenerateInternal(Type classType, bool refType = true)
		{
			classType = classType.ToBasicType();
			GType gType = new GType(classType);
			if (refType)
			{
				var types = gType.GetRefTypes();
				foreach (var type in types)
				{
					AddGenerateClass(type);
				}
			}

			var generateStr = gType.ToString();
			var path = GetPath(classType);

			var folder = Path.GetDirectoryName(path);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllText(path, generateStr);
			//ReflectionUtils.Log(generateStr);
		}


		private static string GetPath(Type classType)
		{
			string path = classType.FullName.Remove(classType.FullName.Length - classType.Name.Length);
			var nameSpaceSplits = path.Split('.');
			string result = GenerateDirectory;
			// 脗盲鈥燿ll脗脿麓脗锚莽
			if (ModuleAliasConfig.TryGetAliasName(classType, out var aliasName))
			{
				result += $"{AddLineBeforeUpper(LegalNameConfig.LegalName(aliasName))}/";
			}
			for (int i = 0; i < nameSpaceSplits.Length; i++)
			{
				var nameSpaceSplit = nameSpaceSplits[i];
				if (string.IsNullOrEmpty(nameSpaceSplit))
				{
					continue;
				}
				var nestedTypeSplits = nameSpaceSplit.Split('+');
				for (int j = 0; j < nestedTypeSplits.Length; j++)
				{
					var nestedTypeSplit = nestedTypeSplits[j];
					if (string.IsNullOrEmpty(nestedTypeSplit))
					{
						continue;
					}
					result += AddLineBeforeUpper(LegalNameConfig.LegalName(nestedTypeSplit)) + "/";
				}
			}

			string className = AddLineBeforeUpper(classType.Name);

			result += $"R{LegalNameConfig.LegalName(className)}.cs";
			return result;
		}

		/// <summary>
		/// 因为windows上路径不区分大小写，这里在文件里在大写字母前加一个下划线，以区分大小写
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static string AddLineBeforeUpper(string str)
		{
			string result = str;
			var upperLetters = Regex.Match(str, "[A-Z]");
			List<Match> upperLettersIndex = new();
			while (upperLetters != null && upperLetters != Match.Empty)
			{
				upperLettersIndex.Add(upperLetters);
				upperLetters = upperLetters.NextMatch();
			}
			for (int i = upperLettersIndex.Count - 1; i >= 0; i--)
			{
				result = result.Insert(upperLettersIndex[i].Index, "_");
			}
			return result;
		}
		

		/// <summary>
		/// 判断是否是原始类型
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsPrimitive(Type type)
		{
			return BlackTypeConfig.IsBlack(type);
		}

		private static void ClearGenerateDirectory()
		{
			if(!Directory.Exists(GenerateDirectory))
			{
				return;
			}
			Directory.Delete(GenerateDirectory, true);
		}
	}
}