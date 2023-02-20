using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = System.Object;

namespace Hvak.Editor.Refleaction
{
	public sealed class REvent : RMember
	{
		protected new EventInfo memberInfo { get; set; }

		public REvent(RType belongMember, string name) : base(belongMember, name)
		{
		}

		public REvent(Type belongType, string name) : base(belongType, name)
		{
		}

		public void AddEventHandler(Delegate handler)
		{
			if(belong == null && !memberInfo.GetAddMethod().IsStatic)
			{
				return;
			}
			memberInfo.AddEventHandler(belong, handler);
		}

		public void RemoveEventHandler(Delegate handler)
		{
			if (belong == null && !memberInfo.GetRemoveMethod().IsStatic)
			{
				return;
			}
			memberInfo.RemoveEventHandler(belong, handler);
		}

		protected override void SetInfo(Type belongType, string name)
		{
			memberInfo = belongType.GetEvent(name, flags);
		}

		protected override void SetType()
		{
			if (memberInfo == null)
			{
				ReflectionUtils.LogError("can not find " + name);
				return;
			}
			type = memberInfo.EventHandlerType;
		}
	}
}