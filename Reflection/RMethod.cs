using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = System.Object;

namespace Hvak.Editor.Refleaction
{
	public sealed class RMethod : RMember
	{
		static Type[] Empty = new Type[] { };

		protected new MethodInfo memberInfo { get; set; }


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
                    memberInfo = belongType.GetMethod(name, flags);
                }
				else
				{
                    memberInfo = belongType.GetMethod(name, flags, null, types, null);
                }
			}
			else
			{
                memberInfo = belongType.GetMethod(name, genericCount, flags, null, types ?? Empty, null);
            }          
        }

		/// <summary>
		/// 创建回调
		/// </summary>
		/// <param name="delegateType"></param>
		/// <returns></returns>
		public Delegate CreateDelegate(Type delegateType)
		{
            if (memberInfo == null || (belong == null && !memberInfo.IsStatic))
            {
				return null;
			}
			return memberInfo.CreateDelegate(delegateType, belong);
		}

		/// <summary>
		/// 函数执行
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public object Invoke(params object[] parameters)
		{
			if(memberInfo == null || (belong == null && !memberInfo.IsStatic))
			{
				return null;
			}
			return memberInfo.Invoke(belong, parameters);
		}

		public object Invoke(Type[] types, params object[] parameters)
		{
			try
			{
				if (memberInfo == null || (belong == null && !memberInfo.IsStatic))
				{
					return null;
				}
				if (types == null || types.Length <= 0)
				{
					return memberInfo.Invoke(belong, parameters);
				}
				else
				{
					return memberInfo.MakeGenericMethod(types).Invoke(belong, parameters);
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