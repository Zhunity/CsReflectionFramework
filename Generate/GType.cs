using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hvak.Editor.Refleaction
{
    /// <summary>
    /// 
    /// </summary>
    public class GType
    {
        List<GGenericArgument> genericArgs = new List<GGenericArgument>();

        Dictionary<string, GField> fields = new();
        Dictionary<string, GProperty> properties = new();
        Dictionary<string, GEvent> events = new();
        Dictionary<string, GMethod> methods = new();

        Dictionary<string, GMember> members = new();

        HashSet<Type> refs = new HashSet<Type>();

        public Type type;

        public GType(Type type)
        {
            this.type = type;

            var genericArgs = type.GetGenericArgumentsWithoutDeclareType();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                GGenericArgument arg = new GGenericArgument(genericArgs[i]);
                this.genericArgs.Add(arg);
            }

            HashSet<MethodInfo> getSetHash = new HashSet<MethodInfo>();
            // event 也會生成field？印象中是這樣
            var events = type.GetEvents(RType.flags);
            foreach (var @event in events)
            {
                getSetHash.Add(@event.AddMethod);
                getSetHash.Add(@event.RemoveMethod);
                GEvent gEvent = new(@event);

                string name = gEvent.GetDeclareName();
                if (!this.events.TryAdd(name, gEvent) || !this.members.TryAdd(name, gEvent))
                {
                    ReflectionUtils.Log(type.Name + "重複 event:" + name + "  " + this.events[name] + " " + this.members[name]);
                }
            }

            var fields = type.GetFields(RType.flags);
            foreach (var field in fields)
            {
                GField gField = new(field);
                string name = gField.GetDeclareName();
                if (!this.fields.TryAdd(name, gField) || !this.members.TryAdd(name, gField))
                {
                    ReflectionUtils.Log(type.Name + "重复field:" + name);
                }
            }

            var properties = type.GetProperties(RType.flags);
            foreach (var property in properties)
            {
                getSetHash.Add(property.GetMethod);
                getSetHash.Add(property.SetMethod);
                GProperty gProperty = new(property);

                string name = gProperty.GetDeclareName();
                if (!this.properties.TryAdd(name, gProperty) || !this.members.TryAdd(name, gProperty))
                {
                    ReflectionUtils.Log(type.Name + "重复properties:" + name);
                }
            }

            // �ж�new op_Explicit_Decimal
            var methods = type.GetMethods(RType.flags);
            foreach (var method in methods)
            {
                if (getSetHash.Contains(method))
                {
                    continue;
                }
                GMethod gMethod = new(method);

                string name = gMethod.GetDeclareName();
                if (!this.methods.TryAdd(name, gMethod) || !this.members.TryAdd(name, gMethod))
                {
                    ReflectionUtils.Log(type.Name + "重复methods:" + name);
                }
            }

            foreach (var member in members.Values)
            {
                member.gType = this;
            }
        }

        public override string ToString()
        {
            string externAliasName = GetExternlAiasName();
            string delcareStr = GetMemberDeclareStr();
            string methodInvoke = GetMethodInvokeStr();
            #region nestType
            Type declaringType = type.DeclaringType;
            var nestedTypeDefine = "";
            while (declaringType != null)
            {
                var nowDefine = $@"public partial class R{declaringType.ToClassName()}
{{
	";
                nestedTypeDefine = nowDefine + nestedTypeDefine;
                declaringType = declaringType.DeclaringType;
            }
            #endregion

            #region generic Type
            var genericArgsConstraints = string.Empty;
            foreach (var genericArg in genericArgs)
            {
                genericArgsConstraints += genericArg.ToString();
            }
            #endregion

            string headerStr = $@"{externAliasName}
using Hvak.Editor.Refleaction;
using System;
using System.Reflection;

namespace Hvak.Editor.Refleaction{GetNameSpace()}
{{";

            string curType = $@"
	/// <summary>
    /// https://github.com/Zhunity/CsReflectionFramework/tree/main
	/// {type.GetFullName()}
	/// </summary>
    public partial class R{type.ToClassName()} : RMember //{genericArgsConstraints}
    {{
        public static Type Type
        {{
            get
            {{
                return {type.ToGetType()};
            }}
        }}

        public R{type.ToConstructorName()}() : base(""{type.GetFullName()}"")
        {{
        }}

        public R{type.ToConstructorName()}(System.Object instance) : base(""{type.GetFullName()}"")
		{{
            SetInstance(instance);
		}}

        public R{type.ToConstructorName()}(RMember belongMember, string name, int genericCount = -1, params Type[] types) : base(belongMember, name, genericCount, types)
	    {{
	    }}

		 public R{type.ToConstructorName()}(Type belongType, string name, int genericCount = -1, params Type[] types) : base(belongType, name, genericCount, types)
	    {{
	    }}

{delcareStr}
{methodInvoke}
    }}
}}
";
            nestedTypeDefine += curType;
            declaringType = type.DeclaringType;
            while (declaringType != null)
            {
                nestedTypeDefine += "}";
                declaringType = declaringType.DeclaringType;
            }
            return headerStr + nestedTypeDefine;
        }


        string GetNameSpace()
        {
            string result = string.Empty;
            if (type.TryGetAliasName(out var aliasName))
            {
                result = ".R" + aliasName;
            }

            if (!string.IsNullOrEmpty(type.Namespace))
            {
                result += ".R";
                var nameSpaceSplits = type.Namespace.Split(".");
                for (int i = 0; i < nameSpaceSplits.Length; i++)
                {
                    var nameSpaceSplit = nameSpaceSplits[i];
                    if (string.IsNullOrEmpty(nameSpaceSplit))
                    {
                        continue;
                    }
                    result += LegalNameConfig.LegalName(nameSpaceSplit);
                    if (i != nameSpaceSplits.Length - 1)
                    {
                        result += ".R";
                    }
                }
            }

            return result;
        }

        public HashSet<Type> GetRefTypes()
        {
            if (refs != null && refs.Count > 0)
            {
                return refs;
            }

            HashSet<Type> types = new HashSet<Type>();
            this.type.GetRefType(ref types);
            foreach (var member in members.Values)
            {
                member.GetRefTypes(types);
            }

            foreach (var type in types)
            {
                refs.Add(type.ToBasicType());
            }
            return refs;
        }

        private string GetExternlAiasName()
        {
            var refTypes = GetRefTypes();
            if (refTypes.Count <= 0)
            {
                return string.Empty;
            }
            var aliasName = new HashSet<string>();
            foreach (var refType in refTypes)
            {
                if (refType.TryGetAliasName(out var name))
                {
                    aliasName.Add(name);
                }
            }
            if (aliasName.Count <= 0)
            {
                return string.Empty;
            }
            var result = string.Empty;
            foreach (var name in aliasName)
            {
                result += $"extern alias {name};\n";
            }
            return result;
        }

        private string GetMemberDeclareStr()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var member in members.Values)
            {
                member.GetDeclareStr(sb);
            }
            return sb.ToString();
        }

        private string GetMethodInvokeStr()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var method in methods.Values)
            {
                sb.AppendLine(method.GenerateMethodInvoke());
            }
            return sb.ToString();
        }
    }
}