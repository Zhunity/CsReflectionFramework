using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SMFrame.Editor.Refleaction
{
    public class GProperty : GMember
    {
        PropertyInfo property;
		List<GParameter> gParameters = new List<GParameter>();


		public GProperty(PropertyInfo property)
        {
            this.property = property;
            isStatic = property.IsStatic();

			var parameters = property.GetIndexParameters();
			foreach(var parameter in parameters)
			{
				gParameters.Add(new GParameter(parameter));
			}
        }

		public override void GetRefTypes(HashSet<Type> refTypes)
		{
			property.PropertyType.GetRefType(ref refTypes);
		}

		public override string GetDeclareName()
		{
			return GetPropertyName(property);
		}

		public override void GetDeclareStr(StringBuilder sb)
		{
			string fieldType = GetPropertyType(property.PropertyType); 
			var declareStr = GetDeclareStr(fieldType, property.Name, property.ToString());
			sb.AppendLine(declareStr);
		}

		private string GetPropertyName(PropertyInfo property)
		{
			string paramStr = LegalNameConfig.LegalName(property.Name);

			if (gParameters.Count > 0)
			{
				foreach (var parameter in gParameters)
				{
					paramStr += parameter.ToFieldName();
				}
			}

			return LegalNameConfig.LegalName(paramStr);
		}

		protected override string GetNewParamStr()
		{
			var method = property.GetMethod == null ? property.SetMethod : property.GetMethod;
			var parameters = method.GetParameters();
			var paramStr = string.Empty;
			for (int i = 0; i < parameters.Length; i++)
			{
				paramStr += $", {parameters[i].ParameterType.ToGetMethod()}";
			}
			return ", -1" + paramStr;
		}

		private string GetPropertyType(Type type)
		{
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = true;
			typeTranslater.Array.format = "PropertyArray<{0}>";
			typeTranslater.Pointer.format = "PropertyPointer<{0}>";
			typeTranslater.GenericTypeDefinition.fun = (strs) =>
			{
				var genericDefine = strs[0];
				string genericParamStr = string.Empty;
				for (int i = 1; i < strs.Length; i++)
				{
					var paramName = strs[i];
					genericParamStr += paramName;
					if (i != strs.Length - 1)
					{
						genericParamStr += ", ";
					}
				}
				var defineName = $"{genericDefine}<{genericParamStr}>";
				return defineName;
			};
			typeTranslater.GenericType.fun = (strs) =>
			{
				var genericDefine = strs[1];
				string genericParamStr = string.Empty;
				for (int i = 2; i < strs.Length; i++)
				{
					var paramName = strs[i];
					genericParamStr += paramName;
					if (i != strs.Length - 1)
					{
						genericParamStr += ", ";
					}
				}
				var defineName = $"{genericDefine}<{genericParamStr}>";
				return defineName;
			};
			typeTranslater.GenericParameter.format = "Property";
			typeTranslater.translate = Translater;




			var declare = type.ToString(typeTranslater);
			string nameSpace = GetNameSpace(declare);
			return nameSpace;
		}

		private bool Translater(Type t, TypeTranslater translater, out string result)
		{
			if (PrimitiveTypeConfig.IsPrimitive(t))
			{
				result = "Property";
				return true;
			}
			result = String.Empty;
			return false;
		}

		private string GetNameSpace(string declare)
		{
			if (string.IsNullOrEmpty(declare))
			{
				return string.Empty;
			}
			return $"R{declare.Replace(".", ".R").Replace("<", "<R").Replace(", ", ", R")}";
		}
	}
}