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
		List<GGenericArgument> gGenericArguments= new List<GGenericArgument>();
		List<GParameter> gParameters= new List<GParameter>();

        public GMethod(MethodInfo method)
        {
            this.method = method;
			isStatic = method.IsStatic;

			var generics = method.GetGenericArguments();
			foreach(var generic in generics) 
			{
				gGenericArguments.Add(new GGenericArgument(generic));
			}

			var parameters = method.GetParameters();
			foreach(var parameter in parameters)
			{
				gParameters.Add(new GParameter(parameter));
			}
		}

		public override void GetRefTypes(HashSet<Type> refTypes)
		{
			method.ReturnType.GetRefType(ref refTypes);

			foreach(var generic in gGenericArguments)
			{
				generic.GetRefTypes(refTypes);
			}

			foreach (var param in gParameters)
			{
				param.GetRefTypes(refTypes);
			}
		}

		public override void GetDeclareStr(StringBuilder sb)
		{
			var declareStr = GetDeclareStr("RMethod", method.Name, method.ToString());
			sb.AppendLine(declareStr);
		}

		protected override string GetNewParamStr()
		{
			var paramStr = string.Empty;
			for (int i = 0; i < gParameters.Count; i++)
			{
				paramStr += $", {gParameters[i].GetNewParamStr()}";
			}

			return $", {gGenericArguments.Count}{paramStr}";
		}

		public override string GetDeclareName()
		{
			return GetMethodName(method);
		}

		 public string GetMethodName(MethodInfo method)
		{
			string paramStr = LegalNameConfig.LegalName(method.Name);
			foreach (var generic in gGenericArguments)
			{
				paramStr += generic.ToFieldName();
			}

			foreach (var parameter in gParameters)
			{
				paramStr += parameter.ToFieldName();
			}

			return LegalNameConfig.LegalName( paramStr);
		}

		public string GenerateMethodInvoke()
		{
			string name = GetMethodName(method);
			bool isUnsafe = false;

			#region 处理泛型
			var genericArgsDelcareStr = string.Empty;
			var genericArgsConstraints = string.Empty;
			var genricArgsStr = string.Empty;
			if (gGenericArguments.Count > 0)
			{
				genericArgsDelcareStr += "<";
				for (int i = 0; i < gGenericArguments.Count; i++)
				{
					genericArgsDelcareStr += gGenericArguments[i].GetName();
					genricArgsStr += $"typeof({gGenericArguments[i].GetName()})";
					if (i < gGenericArguments.Count - 1)
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

			#region 处理参数
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

			#region 处理返回值
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