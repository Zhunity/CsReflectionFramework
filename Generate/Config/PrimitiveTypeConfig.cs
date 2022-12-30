using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace SMFrame.Editor.Refleaction
{
    public class PrimitiveTypeConfig
    {
		static HashSet<Type> PrimitiveType = new HashSet<Type>()
		{
			typeof(string),
			typeof(void),
		};

		/// <summary>
		/// ������ʱ��֪����ô����������Լ����struct���࣬�����δ���
		/// </summary>
		static HashSet<Type> BuZhiDaoStruct = new HashSet<Type>()
		{

			typeof(NativeSlice<>),
			typeof(Nullable<>),
			typeof(NativeArray<>),
			typeof(StyleEnum<>),
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
			if(type.IsGenericType)
			{
				var define = type.GetGenericTypeDefinition();
				if(BuZhiDaoStruct.Contains(define))
				{
					return true;
				}
			}

			return 
				PrimitiveType.Contains(type) ||
				type.IsGenericParameter ||
				type.IsEnum || type.IsPrimitive; // int float��ֵ����
		}
	}
}