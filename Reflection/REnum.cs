using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace SMFrame.Editor.Refleaction
{
	public class REnum : RMember
	{

		public REnum(string type) : base(type, -1)
		{
		}

		public REnum(Type type) : base(type, -1)
		{
		}

		public REnum(RType belongMember, string name) : this(belongMember?.type, name)
		{
			belongMember.AddMember(this as RMember);
		}

		public REnum(Type belongType, string name) : base(belongType, name)
		{
		}

		public REnum(object instance) : this(instance.GetType())
		{
			SetInstance(instance);
		}
	}
}