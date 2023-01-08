using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace SMFrame.Editor.Refleaction
{
    public class CanNotConvertToObjectsConfig
    {
		public static List<Type> CanNotConvertToObjects = new List<Type>()
		{
			typeof(TypedReference),
			typeof(NativeArray<>),
			typeof(NativeSlice<>),
			typeof(Span<>),
			typeof(ReadOnlySpan<>),
		};

		public static bool CanNot(Type type)
		{
			bool canConvertToObject = true;
			foreach (var canNot in CanNotConvertToObjects)
			{
				if (type.ContainType(canNot))
				{
					canConvertToObject = false;
					break;
				}
			}
			return !canConvertToObject;
		}
	}
}
