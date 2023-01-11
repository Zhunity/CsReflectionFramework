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
		/// ������ʱ��֪����ô��������Լ����struct���࣬�����δ���
		/// </summary>
		public static HashSet<Type> BuZhiDaoStruct = new HashSet<Type>()
		{

		};

		/// <summary>
		/// �ж��Ƿ���ԭʼ����
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