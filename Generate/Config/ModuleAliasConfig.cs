﻿using System;
using System.Collections.Generic;

namespace Hvak.Editor.Refleaction
{
	public static class ModuleAliasConfig
	{
		static Dictionary<string, string> ModuleToAlias = new Dictionary<string, string>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="moduleName">类型的module名， type.Module.ScopeName</param>
		/// <param name="aliasName">dll的alias名</param>
		public static void Set(string moduleName, string aliasName)
		{
			ModuleToAlias[moduleName] = aliasName;
		}

		public static bool TryGetAliasName(this Type type, out string aliasName)
		{
			aliasName = string.Empty;
            if (type == null || type.Module == null || string.IsNullOrEmpty(type.Module.ScopeName))
			{
				return false;
			}
			if(! ModuleToAlias.TryGetValue(type.Module.ScopeName, out aliasName))
			{
				return false;
			}
			return !string.IsNullOrWhiteSpace(aliasName);
		}

		public static bool IsThisModule(this Type type, string aliasName) 
		{
			if(!type.TryGetAliasName(out var typeAliasName))
			{
				return string.IsNullOrEmpty(aliasName);
			}
			return typeAliasName.Equals(aliasName);
		}
    }
}

