﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = System.Object;

namespace SMFrame.Editor.Refleaction
{
	public sealed class RMethod : RMember
	{
		static Type[] Empty = new Type[] { };

		MethodInfo methodInfo;


		public RMethod(RType belongMember, string name, int genericCount = -1, params Type[] types) : base(belongMember, name, genericCount, types)
		{
        }

		public RMethod(Type belongType, string name, int genericCount = -1, params Type[] types) : base(belongType, name, genericCount, types)
		{
        }

		protected override void SetInfo(Type belongType, string name)
		{
			if(genericCount < 0)
			{
				if(types == null)
				{
                    methodInfo = belongType.GetMethod(name, flags);
                }
				else
				{
                    methodInfo = belongType.GetMethod(name, flags, null, types, null);
                }
			}
			else
			{
                methodInfo = belongType.GetMethod(name, genericCount, flags, null, types ?? Empty, null);
            }          
        }

		/// <summary>
		/// 创建回调
		/// </summary>
		/// <param name="delegateType"></param>
		/// <returns></returns>
		public Delegate CreateDelegate(Type delegateType)
		{
            if (methodInfo == null || (belong == null && !methodInfo.IsStatic))
            {
				return null;
			}
			return methodInfo.CreateDelegate(delegateType, belong);
		}

		/// <summary>
		/// 函数执行
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public object Invoke(params object[] parameters)
		{
			if(methodInfo == null || (belong == null && !methodInfo.IsStatic))
			{
				return null;
			}
			return methodInfo.Invoke(belong, parameters);
		}

		public object Invoke(Type[] types, params object[] parameters)
		{
			try
			{
				if (methodInfo == null || (belong == null && !methodInfo.IsStatic))
				{
					return null;
				}
				if (types == null || types.Length <= 0)
				{
					return methodInfo.Invoke(belong, parameters);
				}
				else
				{
					return methodInfo.MakeGenericMethod(types).Invoke(belong, parameters);
				}
			}
			catch(Exception e)
			{
				ReflectionUtils.Log(e.ToString());
				return null;
			}
        }
	}
}