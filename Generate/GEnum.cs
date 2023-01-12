using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace SMFrame.Editor.Refleaction
{
	/// <summary>
	/// 
	/// </summary>
    public class GEnum
    {
		public Type type;

        public GEnum(Type type)
        {
            this.type = type;
		}

		public override string ToString()
		{
			return "";
		}
	}
}