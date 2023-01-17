using System;
using System.Collections;
using System.Collections.Generic;

namespace SMFrame.Editor.Refleaction
{
	/// <summary>
	/// 黑名单
	/// </summary>
    public class BlackTypeConfig
    {
		public static HashSet<Type> BlackTypes = new HashSet<Type>()
		{
			typeof(void),
		};

		public static void AddBlackType(Type type) 
		{
			BlackTypes.Add(type);
		}

		public static void AddBlackType(string type) 
		{
			AddBlackType(ReflectionUtils.GetType(type));
		}

		/// <summary>
		/// 判断是否是黑名单类型
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsBlack(Type type)
		{
			if(type == null)
			{
				return true;
			}

			return BlackTypes.Contains(type);
		}
	}
}