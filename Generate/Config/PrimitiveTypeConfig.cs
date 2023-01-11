using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SMFrame.Editor.Refleaction
{
    public class PrimitiveTypeConfig
    {
		public static HashSet<Type> PrimitiveType = new HashSet<Type>()
		{
			typeof(void),
		};

		/// <summary>
		/// 由于暂时不知道怎么处理泛型中约束是struct的类，先屏蔽处理
		/// </summary>
		public static HashSet<Type> BuZhiDaoStruct = new HashSet<Type>()
		{

		};

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

			return PrimitiveType.Contains(type) || type.IsEnum;
		}
	}
}