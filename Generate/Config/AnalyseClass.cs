using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Hvak.Editor.Refleaction
{
	public delegate bool Translate(Type t, TypeTranslater translater, out string result);
	public delegate string Format(Type t, params string[] args);

    public class TypeFormater
    {
		public const string defaultFormat = "{0}";

		public bool can = true;
		public virtual string format { get; set; } = defaultFormat;
		public Format fun;

		public string Format(Type t, params string[] elementStr)
        {
			if(!can)
			{
				return String.Empty;
			}
			if(fun != null)
			{
				return fun(t, elementStr);
			}
            return string.Format(format, elementStr);
        }
	}

	public class GenericTypeFormater : TypeFormater
	{
		public const string GenericSuffix = @"`\d+";

		public bool needDeclareTypeGeneric = false;
		public bool onlyDefination = false;

		public string genericBegin = "<";
		public string genericSplit = ", ";
		public string genericEnd = ">";

		public override string format { get; set; } = "{0}{1}";

		public TypeFormater formatDefine = new TypeFormater
		{
			format = defaultFormat,
		};

		public TypeFormater genericDefine = new TypeFormater
		{
			format = defaultFormat,
		};

		public string FormatDefine(Type type, TypeTranslater translater)
		{
			string defineName = Regex.Replace(type.Name, GenericSuffix, string.Empty);
			defineName = LegalNameConfig.LegalName(defineName);
			
			if (translater.fullName)
			{
				defineName = TypeToString.GetNestedFullName(type, translater, defineName);
			}
			defineName = formatDefine.Format(type, defineName);
			return defineName;
		}

		public string FormatGeneric(Type[] types, params string[] elementStr)
		{
			string result = string.Empty;
			for (int i = 0; i < elementStr.Length; i++)
			{
				var paramName = elementStr[i];
				result += genericDefine.Format(types[i], paramName);
				if (i != elementStr.Length - 1)
				{
					result += genericSplit;
				}
			}
			if (elementStr.Length > 0)
			{
				result = $"{genericBegin}{result}{genericEnd}";
			}
			return result;
		}
	}

	public class TypeTranslater
    {
		public bool fullName;
		public Translate translate;
		public TypeFormater Array = new TypeFormater();
		public TypeFormater ByRef = new TypeFormater();
		public TypeFormater Pointer = new TypeFormater();
        public GenericTypeFormater GenericTypeDefinition = new GenericTypeFormater();
		public GenericTypeFormater GenericType = new GenericTypeFormater();
        public TypeFormater GenericParameter = new TypeFormater();
        public Translate defaultTran;
	}

    public static class TypeToString
    {

		public static string ToGetType(this Type type)
		{
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = false;
			typeTranslater.Array.format = "{0}.MakeArrayType()";
			typeTranslater.Pointer.format = "{0}.MakePointerType()";
			typeTranslater.ByRef.format = "{0}.MakeByRefType()";

			typeTranslater.GenericTypeDefinition.needDeclareTypeGeneric = true;
			typeTranslater.GenericTypeDefinition.formatDefine.fun = PublicToGetMethod;
			typeTranslater.GenericTypeDefinition.genericBegin = ".MakeGenericType(";
			typeTranslater.GenericTypeDefinition.genericEnd = ")";


			typeTranslater.GenericType.needDeclareTypeGeneric = true;
			typeTranslater.GenericType.formatDefine.fun = PublicToGetMethod;
			typeTranslater.GenericType.genericBegin = ".MakeGenericType(";
			typeTranslater.GenericType.genericEnd = ")";

			typeTranslater.GenericParameter.format = "TypeToString.GetType(typeof({0}))";
			typeTranslater.defaultTran = PublicToGetMethod;


			return type.ToString(typeTranslater);
		}

		public static Type GetType(Type t)
		{
			if(t.IsSubclassOf(typeof(RType)))
			{
				var result = t.GetProperty("Type", BindingFlags.Public | BindingFlags.Static);
				return result.GetValue(null) as Type;
			}
			else
			{
				return t;
			}	
		}

		public static string ToFieldName(this Type type)
        {
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = false;
			typeTranslater.Array.format = "{0}Array";
			typeTranslater.Pointer.format = "{0}Pointer";
			typeTranslater.GenericTypeDefinition.genericSplit = "_";
			typeTranslater.GenericTypeDefinition.genericBegin = "_d_";
			typeTranslater.GenericTypeDefinition.genericEnd = "_p_";

			typeTranslater.GenericType.genericSplit = "_";
			typeTranslater.GenericType.genericBegin = "_d_";
			typeTranslater.GenericType.genericEnd = "_p_";

			return type.ToString(typeTranslater);
        }

		public static string ToClassName(this Type type, bool fullName = false)
		{
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = fullName;
			typeTranslater.Array.format = "{0}[]";
			typeTranslater.Pointer.format = "{0}*";

			typeTranslater.defaultTran = VoidToDeclareName;


			return type.ToString(typeTranslater);
		}

		public static string ToConstructorName(this Type type)
		{
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = false;
			typeTranslater.Array.format = "{0}[]";
			typeTranslater.Pointer.format = "{0}*";
			typeTranslater.GenericTypeDefinition.format = "{0}";

			typeTranslater.defaultTran = VoidToDeclareName;


			return type.ToString(typeTranslater);
		}


		public static string ToDeclareName(this Type type, bool fullName = true)
        {
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = fullName;
			typeTranslater.Array.format = "{0}[]";
			typeTranslater.Pointer.format = "{0}*";

			typeTranslater.GenericTypeDefinition.genericDefine.format = "";

			typeTranslater.defaultTran = VoidToDeclareName;


            return type.ToString(typeTranslater);
        }

		static bool VoidToDeclareName(Type t, TypeTranslater translater, out string result)
		{
			result = string.Empty;
			if(t == typeof(void))
			{
				result = "void";
				return true;
			}
			return false;	
		}


		static bool PublicToGetMethod(Type t, TypeTranslater translater, out string result)
		{
			result = PublicToGetMethod(t);
			return true;
		}

		static string PublicToGetMethod(Type t, params string[] args)
		{
			if (t.IsPublic)
			{
				return $"typeof({t.ToDeclareName(true)})";
			}
			else
			{
				return $" ReflectionUtils.GetType(\"{t.GetFullName()}\")";
			}
		}

		public static string ToGetMethod(this Type type)
        {
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = false;
			typeTranslater.Array.format = "{0}.MakeArrayType()";
			typeTranslater.Pointer.format = "{0}.MakePointerType()";
			typeTranslater.ByRef.format = "{0}.MakeByRefType()";
			typeTranslater.GenericTypeDefinition.can = false;

			typeTranslater.GenericType.needDeclareTypeGeneric = true;
			typeTranslater.GenericType.onlyDefination = true;
			typeTranslater.GenericType.formatDefine.fun = PublicToGetMethod;
			typeTranslater.GenericType.genericBegin = ".MakeGenericType(";
			typeTranslater.GenericType.genericEnd = ")";
			typeTranslater.GenericParameter.format = "Type.MakeGenericMethodParameter({1})";
			typeTranslater.defaultTran = PublicToGetMethod;


			return type.ToString(typeTranslater);
		}

        public static Type ToBasicType(this Type type)
        {
			if(type == null)
			{
				ReflectionUtils.LogError("type is null");
				return null;
			}
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				return elementType.ToBasicType();
			}
			else if (type.IsGenericParameter)
            {
                return null;
            }
            else if(type.IsByRef)
            {
				var elementType = type.GetElementType();
				return elementType.ToBasicType();
			}
            else if(type.IsPointer)
            {
				var elementType = type.GetElementType();
				return elementType.ToBasicType();
			}
			else if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                return type.GetGenericTypeDefinition().ToBasicType();
			}
            else
            {
                return type;
            }
		}

		public static void GetRefType(this Type type, ref HashSet<Type> child)
		{
			if(child.Contains(type))
			{
				return;
			}
			child.Add(type);
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				elementType.GetRefType(ref child);
			}
			else if (type.IsByRef)
			{
				var elementType = type.GetElementType();
				elementType.GetRefType(ref child);
			}
			else if (type.IsPointer)
			{
				var elementType = type.GetElementType();
				elementType.GetRefType(ref child);
			}
			else if(type.IsGenericParameter)
			{
				Type[] tpConstraints = type.GetGenericParameterConstraints();
				foreach (Type tpc in tpConstraints)
				{
					tpc.GetRefType(ref child);
				}
			}
			else if(type.IsGenericTypeDefinition)
			{
				var genericArguments = type.GetGenericArguments();
				foreach(var genericArgument in genericArguments)
				{
					genericArgument.GetRefType(ref child);
				}
			}
			else if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				var definition = type.GetGenericTypeDefinition();
				definition.GetRefType(ref child);

				var paras = type.GetGenericArguments();
				foreach(var para in paras)
				{
					para.GetRefType(ref child);
				}
			}
		}

		static string _prefix = string.Empty;
		public static string ToRtypeString(this Type type, string prefix)
		{
			_prefix = prefix;
			TypeTranslater typeTranslater = new TypeTranslater();
			typeTranslater.fullName = true;
			typeTranslater.Array.format = prefix + "Array<{0}>";
			typeTranslater.Pointer.format = prefix + "Pointer<{0}>";

			typeTranslater.GenericParameter.format = prefix;
			typeTranslater.translate = TranslaterRType;


			var declare = type.ToString(typeTranslater);
			string nameSpace = TypeToString.ToRTypeStr(declare);
			return nameSpace;
		}

		private static bool TranslaterRType(Type t, TypeTranslater translater, out string result)
		{
			if (BlackTypeConfig.IsBlack(t))
			{
				result = _prefix;
				return true;
			}
			result = String.Empty;
			return false;
		}

		public static string ToString(this Type type, TypeTranslater translater)
		{
			bool needFullName = translater.fullName;


            if (translater.translate != null && translater.translate(type, translater, out string result))
            {
                return result;
            }
            else if(type == null)
			{
				return String.Empty;
			}
			else if (type.IsArray && translater.Array.can)
			{
				var elementType = type.GetElementType();
				return translater.Array.Format(elementType, elementType.ToString(translater));
			}
			else if (type.IsPointer && translater.Pointer.can)
			{
				var elementType = type.GetElementType();
				return translater.Pointer.Format(elementType, elementType.ToString(translater));
			}
			else if (type.IsByRef && translater.ByRef.can)
			{
				var elementType = type.GetElementType();
				return translater.ByRef.Format(elementType, elementType.ToString(translater));
			}
			// 这个要在IsGenericType前，因为IsGenericTypeDefinition也是IsGenericType
			else if (type.IsGenericTypeDefinition && translater.GenericTypeDefinition.can)
			{
				string defineName = translater.GenericTypeDefinition.FormatDefine(type, translater);
				string paramsStr = string.Empty;
				var genericTypes = translater.GenericTypeDefinition.needDeclareTypeGeneric ? type.GetGenericArguments() : type.GetGenericArgumentsWithoutDeclareType();
				if (genericTypes.Length > 0)
				{
					string[] genericParamStr = new string[genericTypes.Length];
					for (int i = 0; i < genericTypes.Length; i++)
					{
						var genericType = genericTypes[i];
						var paramName = genericType.ToString(translater);
						genericParamStr[i] = paramName;
					}

					paramsStr = translater.GenericTypeDefinition.FormatGeneric(genericTypes, genericParamStr);
				}

				result = translater.GenericTypeDefinition.Format(type, defineName, paramsStr);
				return result;
			}
			else if (type.IsGenericType && !type.IsGenericTypeDefinition && translater.GenericType.can)
			{
				var genericDefine = type.GetGenericTypeDefinition();
				string defineName = translater.GenericType.FormatDefine(translater.GenericType.onlyDefination ? genericDefine : type, translater);
				string paramsStr = string.Empty;
				// https://docs.microsoft.com/zh-cn/dotnet/framework/reflection-and-codedom/how-to-examine-and-instantiate-generic-types-with-reflection
				var genericTypes = translater.GenericType.needDeclareTypeGeneric ? type.GetGenericArguments() : type.GetGenericArgumentsWithoutDeclareType();
				if (genericTypes.Length > 0)
				{
					string[] genericParamStr = new string[genericTypes.Length];

					for (int i = 0; i < genericTypes.Length; i++)
					{
						var genericType = genericTypes[i];
						var paramName = genericType.ToString(translater);
						genericParamStr[i] = paramName;
					}
					paramsStr = translater.GenericType.FormatGeneric(genericTypes, genericParamStr);
				}
				
				result = translater.GenericType.Format(genericDefine, defineName, paramsStr);
				return result;
			}
			else if (type.IsGenericParameter && translater.GenericParameter.can)
			{
				return translater.GenericParameter.Format(type, LegalNameConfig.LegalName(type.Name), type.GenericParameterPosition.ToString());
			}
			else if (translater.defaultTran != null && translater.defaultTran(type, translater, out result))
			{
				return result;
			}
			else
			{
				if (needFullName)
				{
					return GetNestedFullName(type, translater, LegalNameConfig.LegalName(type.Name));
				}
				else
				{
					return LegalNameConfig.LegalName(type.Name);
				}
			}
		}

		public static string GetNestedFullName(Type type, TypeTranslater translater, string name)
		{
			if (type.IsNested)
			{
				string result = name;
				var declareStr = type.DeclaringType.ToString(translater);
				result = declareStr + "." + result;
				return result;
			}
			else
			{
                string result = string.Empty;
				if(ModuleAliasConfig.TryGetAliasName(type, out var aliasName))
				{
					result += aliasName + "::";
                }
                if (!string.IsNullOrEmpty(type.Namespace))
				{
                    var nameSpaceSplit = type.Namespace.Split(".");
                    foreach (var item in nameSpaceSplit)
                    {
                        result += LegalNameConfig.LegalName(item) + ".";
                    }
                }
				
				result += name;
				return result;
			}
		}

		public static bool IsPublic(this Type type)
		{
			HashSet<Type> refTypes = new HashSet<Type>();
			type.GetRefType(ref refTypes);
			foreach(var refType in refTypes)
			{
				if(!refType.IsPublic)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsUnsafe(this Type type)
		{
			HashSet<Type> refTypes = new HashSet<Type>();
			type.GetRefType(ref refTypes);
			foreach (var refType in refTypes)
			{
				if (refType.IsPointer)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainType(this Type type, Type targetType)
		{
			HashSet<Type> refTypes = new HashSet<Type>();
			type.GetRefType(ref refTypes);
			foreach(var refType in refTypes)
			{
				if(refType == targetType)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsStatic(this PropertyInfo propertyInfo)
		{
			return ((propertyInfo.CanRead && propertyInfo.GetMethod.IsStatic) ||
				(propertyInfo.CanWrite && propertyInfo.SetMethod.IsStatic));
		}

		public static bool IsStatic(this EventInfo eventInfo)
		{
			return ((eventInfo.AddMethod != null && eventInfo.AddMethod.IsStatic) ||
				(eventInfo.RemoveMethod != null && eventInfo.RemoveMethod.IsStatic));
		}

		public static Type[] GetGenericArgumentsWithoutDeclareType(this Type type)
		{
			var totalGenericArguments = type.GetGenericArguments();
			if(totalGenericArguments == null)
			{
				return new Type[] { };
			}
			if (!type.IsNested)
			{
				return totalGenericArguments;
			}

			int declareGenericCount = 0;
			var declareType = type.DeclaringType;
			while (declareType != null)
			{
				if (declareType.IsGenericType)
				{
					var genericArgs2 = declareType.GetGenericArguments();
					declareGenericCount += genericArgs2.Length;
				}
				declareType = declareType.DeclaringType;
			}

			if(totalGenericArguments.Length - declareGenericCount <= 0)
			{
				return new Type[] { };
			}

			Type[] genericArgs = new Type[totalGenericArguments.Length - declareGenericCount];
			for(int i = declareGenericCount; i < totalGenericArguments.Length; i++)
			{
				genericArgs[i - declareGenericCount] = totalGenericArguments[i];
			}
			return genericArgs;
		}

		public static string ToRTypeStr(string typeStr)
		{
			if (string.IsNullOrEmpty(typeStr))
			{
				return string.Empty;
			}
			return $"Hvak.Editor.Refleaction.R{typeStr.Replace("::", ".").Replace(".", ".R").Replace("<", "<Hvak.Editor.Refleaction.R").Replace(", ", ", Hvak.Editor.Refleaction.R")}";
		}

		public static string GetFullName(this Type type)
		{
			if(type.TryGetAliasName(out var aliasName))
			{
				return $"{aliasName}::{type.FullName}";
			}
			return type.FullName;
		}
	}
}
