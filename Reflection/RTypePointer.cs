using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SMFrame.Editor.Refleaction
{
	public class RTypePointer<T> : RType
	{
		public RTypePointer(object instance) : base(instance)
		{
		}
	}
}