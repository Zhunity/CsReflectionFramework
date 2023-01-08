using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SMFrame.Editor.Refleaction
{
    public class GGenericArgument
    {
        Type genericArgument;
        List<string> constraints = new();

        public GGenericArgument(Type type)
        {
            genericArgument = type;
			CollectConstraints();
		}

        void CollectConstraints()
        {
			Type[] tpConstraints = genericArgument.GetGenericParameterConstraints();

			GenericParameterAttributes gpa = genericArgument.GenericParameterAttributes;
			GenericParameterAttributes att = gpa &
			GenericParameterAttributes.SpecialConstraintMask;
			if (tpConstraints.Length <= 0 && att == GenericParameterAttributes.None)
			{
				return;
			}

			if ((att & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
			{
				constraints.Add("class");
			}

			if ((att & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
			{
				constraints.Add("struct");
			}


			foreach (Type tpc in tpConstraints)
			{
				if (tpc == typeof(ValueType) || !tpc.IsPublic())
				{
					continue;
				}
				constraints.Add(tpc.ToClassName(true));
			}

			if ((att & GenericParameterAttributes.DefaultConstructorConstraint) != 0 && (att & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0)
			{
				constraints.Add("new()");
			}
		}

		public override string ToString()
		{
			if(constraints.Count <= 0)
			{
				return string.Empty;
			}
			string result = $" where {GetName()} : ";
			for(int i = 0; i < constraints.Count; i++)
			{
				result += constraints[i];
				if(i < constraints.Count - 1)
				{
					result += ", ";
				}
			}
			return result;
		}

		public string GetName()
		{
			return LegalNameConfig.LegalName(genericArgument.Name);
		}

		public void GetRefTypes(HashSet<Type> refTypes)
		{
			Type[] tpConstraints = genericArgument.GetGenericParameterConstraints();
			foreach (Type tpc in tpConstraints) 
			{
				tpc.GetRefType(ref refTypes);
			}
		}

		public string ToFieldName()
		{
			return "_G" + genericArgument.ToFieldName(); ;
		}
	}
}