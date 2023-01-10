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
			string fieldType = GetPropertyType(); 
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
			var paramStr = string.Empty;
			for (int i = 0; i < gParameters.Count; i++)
			{
				paramStr += $", {gParameters[i].GetNewParamStr()}";
			}
			return ", -1" + paramStr;
		}

		private string GetPropertyType()
		{
			return property.PropertyType.ToRtypeString("Property");
		}
	}
}