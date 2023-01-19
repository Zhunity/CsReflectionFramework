using System;
namespace SMFrame.Editor.Refleaction
{
	public static class ModuleAliasConfig
	{
		static Dictionary<string, string> ModuleAlias = new Dictionary<string, string>();

		public static void Set(string moduleName, string aliasName)
		{
			ModuleAlias[moduleName] = aliasName;
        }

		public static bool TryGetAliasName(this Type type, out string aliasName)
		{
			aliasName = string.Empty;
            if (type == null || type.Module == null || string.IsNullOrEmpty(type.Module.ScopeName))
			{
				return false;
			}
			if(! ModuleAlias.TryGetValue(type.Module.ScopeName, out aliasName))
			{
				return false;
			}
			return !string.IsNullOrWhiteSpace(aliasName);
		}
    }
}

