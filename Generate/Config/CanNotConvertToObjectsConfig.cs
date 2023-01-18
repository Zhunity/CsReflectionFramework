using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SMFrame.Editor.Refleaction
{
	/// <summary>
	/// 判断参数是否是自己的类型，还是R类型
	/// </summary>
    public class CanNotConvertToObjectsConfig
    {
		public static HashSet<Type> CanNotConvertToObjects = new HashSet<Type>()
		{
		};

		public static void Add(Type type)
		{
			CanNotConvertToObjects.Add(type);
		}

		public static void Add(string type)
		{
			CanNotConvertToObjects.Add(ReflectionUtils.GetType(type));
		}

		public static bool CanNot(Type type)
		{
			HashSet<Type> types = new HashSet<Type>();
			type.GetRefType(ref types);
			foreach(var t in types)
			{
				if(CanNotConvertToObjects.Contains(t))
				{
					return true;
				}

				if(!t.IsPublic)
				{
					return true;
				}

				if(t.IsByRefLike)
				{
					return true;
				}

				if(t.IsGenericParameter)
				{
					GenericParameterAttributes gpa = t.GenericParameterAttributes;
					GenericParameterAttributes att = gpa & GenericParameterAttributes.SpecialConstraintMask;
					if ((att & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
