using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SMFrame.Editor.Refleaction
{
    public class LegalNameConfig
    {

		static Dictionary<string, int> replace;

		public static void LoadReplace(string jsonFile)
		{
			if (!File.Exists(jsonFile))
			{
				replace = new Dictionary<string, int>();
				return;
			}
			var lines = File.ReadAllLines(jsonFile);
			foreach(var line in lines)
			{
				if(string.IsNullOrEmpty(line))
				{
					continue;
				}
				var strs = line.Split('=');
				if(strs.Length < 2)
				{
					continue;
				}
				var key = strs[0].Trim();
				if(string.IsNullOrEmpty(key))
				{
					continue;
				}

				if(int.TryParse(strs[1].Trim(), out var value)) 
				{
					continue;
				}
				replace.TryAdd(key, value);
			}
		}

		public static string LegalName(string str)
		{
			var matches = Regex.Matches(str, @"\W");
			if (matches == null || matches.Count <= 0)
			{
				return str;
			}
			foreach (var match in matches)
			{
				var c = match.ToString();
				if (!replace.TryGetValue(c, out int value))
				{
					value = replace.Count;
					replace[c] = value;
				}
				str = str.Replace(c, "__" + value.ToString() + "__");
			}
			return str;
		}

		public static void SaveReplace(string jsonFile)
		{
			if (File.Exists(jsonFile))
			{
				File.Delete(jsonFile);
			}
			if(!Directory.Exists(Path.GetDirectoryName(jsonFile)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(jsonFile));
			}
			var str = string.Empty;
			foreach(var item in replace) 
			{
				str += $"{item.Key}={item.Value}";
			}
			File.WriteAllText(jsonFile, str);
		}
	}
}
