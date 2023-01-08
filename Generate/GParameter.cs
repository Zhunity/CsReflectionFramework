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

		public void GetRefTypes(HashSet<Type> refTypes)
		{
			parameter.ParameterType.GetRefType(ref refTypes);
		}

		public string ToFieldName()
		{
			string paramStr = string.Empty;
			var parameterType = parameter.ParameterType;
			var name = parameterType.ToFieldName();

			bool isRef = parameterType.IsByRef;
			if (!isRef)
			{
				paramStr += "_" + name;
			}
			else if (parameter.IsOut)
			{
				paramStr += "_Out_" + name;
			}
			else if (parameter.IsIn)
			{
				paramStr += "_In_" + name;
			}
			else
			{
				paramStr += "_Ref_" + name;
			}

			return paramStr;
		}

		public string GetNewParamStr()
		{
			return parameter.ParameterType.ToGetMethod();
		}
	}
}