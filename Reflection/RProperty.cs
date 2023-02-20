using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = System.Object;

namespace Hvak.Editor.Refleaction
{
	public class RProperty : RMember
	{
		protected new PropertyInfo memberInfo{ get; set; }

		public RProperty(RType belongMember, string name, int genericCount = -1, params Type[] types) : base(belongMember, name, genericCount, types)
		{
		}

		public RProperty(Type belongType, string name, int genericCount = -1, params Type[] types) : base(belongType, name, genericCount, types)
		{
		}

		/// <summary>
		/// 设置PropertyInfo值的静态类，供Mermber、Property用
		/// </summary>
		public static object GetPropertyValue(PropertyInfo info, object belong, params object[] index)
		{
			// 判断静态类型
			if (belong == null && !info.GetMethod.IsStatic)
			{
				return null;
			}

			// 参数个数大于0，表示是索引器
			// 返回索引器的函数，供外面调用
			try
			{
				if (info.GetIndexParameters().Length > 0)
				{
					return info.GetValue(belong, index);
				}
				else
				{
					return info.GetValue(belong);
				}
			}
			catch (Exception ex)
			{
				ReflectionUtils.LogError(belong.GetType().Name + "\t" + info + "\n" + ex.ToString());
				return null;
			}
		}

		public override void SetValue(object value)
		{
			if (belong == null && !memberInfo.SetMethod.IsStatic)
			{
				return;
			}

			memberInfo.SetValue(belong, value);
		}

		public override void SetValue(object value, params object[] index)
		{
			if (belong == null && !memberInfo.SetMethod.IsStatic)
			{
				return;
			}

			memberInfo.SetValue(belong, value, index);
		}

		/// <summary>
		/// 数组会有问题
		/// 如RPackageItem.Item
		/// 类型是UnityEngine.UIElements.VisualElement Item [Int32]
		/// 修改方法：object value = info.GetValue(instance, new object[] { 0 });
		/// TODO 看看怎么判断是数组
		/// </summary>
		/// <returns></returns>
		public override object GetValue()
		{
			return GetPropertyValue(memberInfo, belong);
		}

		public override object GetValue(params object[] index)
		{
			return GetPropertyValue(memberInfo, belong, index);
		}

		/// <summary>
		/// 判断是不是索引器
		/// </summary>
		/// <returns></returns>
		public bool IsIndexer()
		{
			return memberInfo.GetIndexParameters().Length > 0;
		}

		protected override void SetInfo(Type belongType, string name)
		{
			memberInfo = belongType.GetProperty(name, RType.flags, null, null, types, null); 
		}

		protected override void SetType()
		{
			if (memberInfo == null)
			{
				ReflectionUtils.LogError("can not find " + name);
				return;
			}
			type = memberInfo.PropertyType;
		}
	}
}