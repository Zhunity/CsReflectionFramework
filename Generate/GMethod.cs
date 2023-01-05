using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace SMFrame.Editor.Refleaction
{
    public class GMethod : GMember
    {
        MethodInfo method;

        public GMethod(MethodInfo method)
        {
            this.method = method;
			isStatic = method.IsStatic;
        }

		public override void GetRefTypes(HashSet<Type> refTypes)
		{
			method.ReturnType.GetRefType(ref refTypes);
			var parameters = method.GetParameters();
			foreach (var param in parameters)
			{
				param.ParameterType.GetRefType(ref refTypes);
			}
		}

		public override void GetDeclareStr(StringBuilder sb)
		{
			var declareStr = GetDeclareStr("RMethod", method.Name, method.ToString());
			sb.AppendLine(declareStr);
		}

		protected override string GetNewParamStr()
		{
			var generics = method.GetGenericArguments();
			var parameters = method.GetParameters();
			var paramStr = string.Empty;
			for (int i = 0; i < parameters.Length; i++)
			{
				paramStr += $", {parameters[i].ParameterType.ToGetMethod()}";
			}

			return $", {generics.Length}{paramStr}";
		}

		public override string GetDeclareName()
		{
			return GetMethodName(method);
		}

		static public string GetMethodName(MethodInfo method)
		{
			var generics = method.GetGenericArguments();
			string paramStr = LegalNameConfig.LegalName(method.Name);
			foreach (var generic in generics)
			{
				paramStr += "_G" + generic.ToFieldName();
			}

			var parameters = method.GetParameters();
			foreach (var parameter in parameters)
			{
				Type parameterType = parameter.ParameterType;

				var name = parameterType.ToFieldName();

				bool isRef = parameterType.IsByRef;
				if (!isRef)
				{
					paramStr += "_" + name;
					continue;
				}
				if (parameter.IsOut)
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
			}

			return LegalNameConfig.LegalName( paramStr);
		}

		public string GenerateMethodInvoke()
		{
			string name = GetMethodName(method);
			bool isUnsafe = false;

			#region ������
			var genericArgsDelcareStr = string.Empty;
			var genericArgsConstraints = string.Empty;
			var genricArgsStr = string.Empty;
			var generics = method.GetGenericArguments();
			if (generics.Length > 0)
			{
				List<GGenericArgument> gGenericArguments = new List<GGenericArgument>();
				for (int i = 0; i < generics.Length; i++)
				{
					var genericArgs = new GGenericArgument(generics[i]);
					gGenericArguments.Add(genericArgs);
				}

				genericArgsDelcareStr += "<";
				for (int i = 0; i < gGenericArguments.Count; i++)
				{
					genericArgsDelcareStr += gGenericArguments[i].GetName();
					genricArgsStr += $"typeof({gGenericArguments[i].GetName()})";
					if (i < generics.Length - 1)
					{
						genericArgsDelcareStr += ", ";
						genricArgsStr += ", ";
					}
				}
				genericArgsDelcareStr += ">";

				for (int i = 0; i < gGenericArguments.Count; i++)
				{
					var genericArgs = gGenericArguments[i];
					genericArgsConstraints += genericArgs.ToString();
				}
			}
			#endregion

			#region �������
			var parameters = method.GetParameters();
			var paramStr = string.Empty;
			string paramDeclareStr = string.Empty;
			string outDefaultStr = string.Empty;
			string outAssignStr = string.Empty;

			for (int i = 0; i < parameters.Length; i++)
			{
				var param = parameters[i];
				var paramType = param.ParameterType;
				if (!paramType.IsPublic())
				{
					return string.Empty;
				}

				string str = string.Empty;
				if (paramType.IsByRef)
				{
					if (param.IsOut)
					{
						str += "out ";
						outDefaultStr += $"\t\t\t{param.Name} = default;\n";
						outAssignStr += $"\t\t\t{param.Name} = ({paramType.ToClassName(true)})___parameters[{param.Position}];\n";
					}
					else if (param.IsIn)
					{
						str += "in ";
					}
					else
					{
						str += "ref ";
						outAssignStr += $"\t\t\t{param.Name} = ({paramType.ToClassName(true)})___parameters[{param.Position}];\n";
					}
				}
				if (paramType.IsUnsafe())
				{
					isUnsafe = true;
				}

				str += paramType.ToClassName(true) + "  @" + param.Name;
				paramDeclareStr += str;
				
				if (!GetParamName(param, out var paramName))
				{
					return string.Empty;
				}
				paramStr += paramName;
				if (i < parameters.Length - 1)
				{
					paramDeclareStr += ", ";
					paramStr += ", ";
				}
			}
			#endregion

			#region ������ֵ
			string returnStr = GetReturn(method.ReturnType, out string returnTypeStr);
			if (method.ReturnType.IsUnsafe())
			{
				isUnsafe = true;
			}
			#endregion

			if(isUnsafe)
			{
				return string.Empty;
			}
			var result = $@"
        public {(isUnsafe ? "unsafe " : "")}{(method.IsStatic ? "static" : "virtual")} {returnTypeStr} {LegalNameConfig.LegalName(method.Name)}{genericArgsDelcareStr}({paramDeclareStr}){genericArgsConstraints}
        {{
{outDefaultStr}
            var ___genericsType = new Type[] {{{genricArgsStr}}};
            var ___parameters = new object[]{{{paramStr}}};
            var ___result = R{name}.Invoke(___genericsType, ___parameters);
{outAssignStr}
            {returnStr}
        }}
";
			return result;
		}

		static bool GetParamName(ParameterInfo param, out string result)
		{
			result = string.Empty;
			foreach (var canNot in CanNotConvertToObjects)
			{
				if (param.ParameterType.ContainType(canNot))
				{
					return false;
				}
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


		private static List<Type> CanNotConvertToObjects = new List<Type>()
		{
			typeof(TypedReference),
			typeof(NativeArray<>),
			typeof(NativeSlice<>),
			typeof(Span<>),
			typeof(ReadOnlySpan<>),
		};

		static string GetReturn(Type returnType, out string returnTypeStr)
		{
			bool canConvertToObject = true;
			foreach(var canNot in CanNotConvertToObjects)
			{
				if(returnType.ContainType(canNot))
				{
					canConvertToObject = false;
				}
			}
			bool isPublic = returnType.IsPublic();
			returnTypeStr = (canConvertToObject && isPublic) ? returnType.ToClassName(true) : typeof(System.Object).ToClassName(true);
			bool hasReturn = returnType != typeof(void);
			if (!hasReturn)
			{
				return String.Empty;
			}

			return $"return ({returnTypeStr})___result;";
		}
	}
}