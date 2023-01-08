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
			if (!paramType.IsPublic())
			{
				return string.Empty;
			}

			if (!GetParamName(parameter, out var paramName))
			{
				return string.Empty;
			}
			paramStr += paramName;

			return paramStr;
		}
		public string GetDeclareStr()
		{
			string paramDeclareStr = string.Empty;

			var paramType = parameter.ParameterType;
			// TODO R
			if (!paramType.IsPublic())
			{
				return string.Empty;
			}

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

			str += paramType.ToClassName(true) + "  @" + parameter.Name;
			paramDeclareStr += str;

			return paramDeclareStr;
		}


		public string GetOutDefaultStr()
		{
			string outDefaultStr = string.Empty;
			var paramType = parameter.ParameterType;
			if (!paramType.IsPublic())
			{
				return string.Empty;
			}

			if (paramType.IsByRef)
			{
				if (parameter.IsOut)
				{
					outDefaultStr += $"\t\t\t{parameter.Name} = default;\n";
				}
			}
			return outDefaultStr;
		}

		public string GetOutAssignStr()
		{
			string outAssignStr = string.Empty;

			var paramType = parameter.ParameterType;
			if (!paramType.IsPublic())
			{
				return string.Empty;
			}

			string str = string.Empty;
			if (paramType.IsByRef)
			{
				if (parameter.IsOut)
				{
					outAssignStr += $"\t\t\t{parameter.Name} = ({paramType.ToClassName(true)})___parameters[{parameter.Position}];\n";
				}
				else if (!parameter.IsIn)
				{
					outAssignStr += $"\t\t\t{parameter.Name} = ({paramType.ToClassName(true)})___parameters[{parameter.Position}];\n";
				}
			}

			return outAssignStr;
		}

		public bool IsPublic()
		{
			return parameter.ParameterType.IsPublic();
		}

		public bool IsUnsafe()
		{
			return parameter.ParameterType.IsUnsafe();
		}

		public bool CanNotConvertToObjects()
		{
			return CanNotConvertToObjectsConfig.CanNot(parameter.ParameterType);
		}


		static bool GetParamName(ParameterInfo param, out string result)
		{
			result = string.Empty;
			if(CanNotConvertToObjectsConfig.CanNot(param.ParameterType))
			{
				return false;
			}
			if (param.ParameterType.IsPointer)
			{
				result = $"Pointer.Box(@{param.Name}, typeof({param.ParameterType.GetElementType().ToDeclareName()}))";
			}
			else if (param.ParameterType == typeof(TypedReference))
			{
				result = $"TypedReference.ToObject(@{param.Name})";
			}
			else
			{
				result = "@" + param.Name;
			}
			return true;
		}
	}
}