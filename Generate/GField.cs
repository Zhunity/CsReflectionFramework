using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SMFrame.Editor.Refleaction
{
    public class GField : GMember
    {
        FieldInfo field;
        public GField(FieldInfo info)
        {
            field = info;
            isStatic = field.IsStatic;
		}

        public override void GetRefTypes(HashSet<Type> refTypes)
        {
			field.FieldType.GetRefType(ref refTypes);
		}

		public override string GetDeclareName()
		{
			return LegalNameConfig.LegalName( field.Name);
		}

		public override void GetDeclareStr(StringBuilder sb)
        {
            string fieldType = GetFieldType();
			var declareStr = GetDeclareStr(fieldType, field.Name, field.ToString());
            sb.AppendLine(declareStr);
		}

		public override bool IsDeclareInType()
		{
			return field.DeclaringType == this.gType.type;
		}

		private string GetFieldType()
        {
			return field.FieldType.ToRtypeString("Field"); ;
		}
    }
}