using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

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


		public string GetParamStr()
		{
			var paramStr = string.Empty;

			var paramType = parameter.ParameterType;
			var paramName = GetName();

			if (!paramType.IsPublic())
			{
				return $"{paramName}.Value";
			}

			if (CanNotConvertToObjectsConfig.CanNot(parameter.ParameterType))
			{
				return string.Empty;
			}
			if (parameter.ParameterType.IsPointer)
			{
				paramStr = $"Pointer.Box({paramName}, typeof({parameter.ParameterType.GetElementType().ToDeclareName()}))";
			}
			else if (parameter.ParameterType == typeof(TypedReference))
			{
				paramStr = $"TypedReference.ToObject({paramName})";
			}
			else
			{
				paramStr = paramName;
			}

			return paramStr;
		}

		/// <summary>
		/// 函数生命括号内的参数
		/// </summary>
		/// <returns></returns>
		public string GetDeclareStr()
		{
			string paramDeclareStr = string.Empty;

			var paramType = parameter.ParameterType;
			var paramName = GetName();

			string str = string.Empty;
			if (paramType.IsByRef)
			{
				if (parameter.IsOut)
				{
					str += "out ";
				}
				else if (parameter.IsIn)
				{
					str += "in ";
				}
				else
				{
					str += "ref ";
				}
			}

			if(paramType.IsPublic())
			{
				str += paramType.ToClassName(true) + " " + paramName;
			}
			else
			{
				str += paramType.ToRtypeString("Type") + " " + paramName;
			}
			
			paramDeclareStr += str;

			return paramDeclareStr;
		}


		public string GetOutDefaultStr()
		{
			string outDefaultStr = string.Empty;
			var paramType = parameter.ParameterType;
			var paramName = GetName();

			if (paramType.IsByRef)
			{
				if (parameter.IsOut)
				{
					outDefaultStr += $"\t\t\t{paramName} = default;\n";
				}
			}
			return outDefaultStr;
		}

		public string GetOutAssignStr()
		{
			var paramType = parameter.ParameterType;
			if (!paramType.IsByRef || parameter.IsIn)
			{
				return string.Empty;
			}

			string outAssignStr = string.Empty;

			var paramName = GetName();

			if (paramType.IsPublic())
			{
				outAssignStr = $"\t\t\t{paramName} = ({paramType.ToClassName(true)})___parameters[{parameter.Position}];\n";
			}
			else
			{
				outAssignStr = $"\t\t\t{paramName} = new {paramType.ToRtypeString("Type")}(___parameters[{parameter.Position}]);\n";
			}

			return outAssignStr;
		}

		public bool IsUnsafe()
		{
			return parameter.ParameterType.IsUnsafe();
		}

		public bool CanNotConvertToObjects()
		{
			return CanNotConvertToObjectsConfig.CanNot(parameter.ParameterType);
		}

		public string GetName()
		{
			return "@" + LegalNameConfig.LegalName(parameter.Name);
		}
	}
}