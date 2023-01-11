using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SMFrame.Editor.Refleaction
{
    public class CanNotConvertToObjectsConfig
    {
		public static List<Type> CanNotConvertToObjects = new List<Type>()
		{
			typeof(TypedReference),
			typeof(Span<>),
			typeof(ReadOnlySpan<>),
		};

		public static bool CanNot(Type type)
		{
			bool canConvertToObject = true;

			HashSet<Type> types = new HashSet<Type>();
			type.GetRefType(ref types);
			foreach(var t in types)
			{
				if(CanNotConvertToObjects.Contains(t))
				{
					canConvertToObject = false;
					break;
				}

				if(t.IsGenericParameter)
				{
					GenericParameterAttributes gpa = t.GenericParameterAttributes;
					GenericParameterAttributes att = gpa & GenericParameterAttributes.SpecialConstraintMask;
					if ((att & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
					{
						canConvertToObject = false;
						break;
					}
				}
			}

			return !canConvertToObject;
		}
	}
}
