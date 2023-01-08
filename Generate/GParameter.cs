using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SMFrame.Editor.Refleaction
{
    public class GParameter
    {
		ParameterInfo parameter;

		public GParameter(ParameterInfo info)
		{
			parameter = info;
		}
	}
}