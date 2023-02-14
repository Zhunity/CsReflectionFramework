using System;
using System.Collections;
using System.Collections.Generic;

namespace Hvak.Editor.Refleaction
{
	public class RTypePointer<T> : RType
	{
		public RTypePointer(object instance) : base(instance)
		{
		}
	}
}