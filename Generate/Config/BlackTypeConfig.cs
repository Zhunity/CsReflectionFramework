using System;
using System.Collections;
using System.Collections.Generic;

namespace Hvak.Editor.Refleaction
{
	/// <summary>
	/// 黑名单
	/// TODO 和CanNotConvertToObjectsConfig合并一下
	/// TODO 像DeserializationToken这种又没有，又public的，这时候要怎么处理
	/// </summary>
	public class BlackTypeConfig
    {
		public static HashSet<Type> BlackTypes = new HashSet<Type>()
		{
			typeof(void),
			ReflectionUtils.GetType("System.Threading.Lock"),
			ReflectionUtils.GetType("System.Runtime.Serialization.DeserializationToken"),
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