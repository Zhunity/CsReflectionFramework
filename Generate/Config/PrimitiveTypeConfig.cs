using System;
using System.Collections;
using System.Collections.Generic;

namespace SMFrame.Editor.Refleaction
{
    public class PrimitiveTypeConfig
    {
		public static HashSet<Type> PrimitiveType = new HashSet<Type>()
		{
			typeof(void),
		};

		public static void AddPrimitiveType(Type type) 
		{
			PrimitiveType.Add(type);
		}

		public static void AddPrimitiveType(string type) 
		{
			AddPrimitiveType(ReflectionUtils.GetType(type));
		}

		/// <summary>
		/// 判断是否是原始类型
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsPrimitive(Type type)
		{
			if(type == null)
			{
				return true;
			}

			return PrimitiveType.Contains(type);
		}
	}
}