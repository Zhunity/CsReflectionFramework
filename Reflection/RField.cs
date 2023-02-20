using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = System.Object;

namespace Hvak.Editor.Refleaction
{
	public class RField : RMember
	{
		protected new FieldInfo memberInfo { get; set; }

		public RField(RType belongMember, string name, int genericCount = -1) : base(belongMember, name, genericCount)
		{
		}

		public RField(Type belongType, string name, int genericCount = -1) : base(belongType, name, genericCount)
		{
		}

		public static object GetFieldValue(FieldInfo info, object belong)
		{
			// 判断静态类型
			if (belong == null && !info.IsStatic)
			{
				return null;
			}

			return info.GetValue(belong);
		}

		public override void SetValue(object value)
		{
			if (belong == null && !memberInfo.IsStatic)
			{
				return;
			}
			memberInfo.SetValue(belong, value);
		}

		public override object GetValue()
		{
			return GetFieldValue(memberInfo, belong);
		}

		protected override void SetInfo(Type belongType, string name)
		{
			memberInfo = belongType.GetField(name, flags);
		}

		protected override void SetType()
		{
			if (memberInfo == null)
			{
				ReflectionUtils.LogError("can not find " + name);
				return;
			}
			type = memberInfo.FieldType;
		}
	}
}