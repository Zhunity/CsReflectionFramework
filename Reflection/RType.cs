﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = System.Object;

namespace Hvak.Editor.Refleaction
{
	public class RType
	{
		public const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		private static int _idGenerater = 0;
		public int id;
        public string name;         // 名字（在member中相当于变量名）
		public virtual Type type { get; protected set; }           // 对象的实际类型
		public Object instance;     // 这个对象的实例

		protected int genericCount = -1;
		protected Type[] types;
		protected List<RMember> memberList = new List<RMember>();

		public Object Value
		{
			set
			{
				SetValue(value);
			}
			get
			{
				return GetValue();
			}
		}

		protected RType()
		{
			SetID();
			memberList.Clear();
		}

		/// <summary>
		/// 传进一个类型名字，创建该类型的反射类
		/// 毕竟主工程中可能拿不到该类型，只能通过传名字来完成操作
		/// </summary>
		/// <param name="type"></param>
		public RType(string type, int genericCount = -1, params Type[] types)
		{
			SetID();
			this.genericCount = genericCount;
			this.types = types;
			this.type = ReflectionUtils.GetType(type);
			name = type;
			memberList.Clear();
			OnInit();
		}

		/// <summary>
		/// 直接传进类型，省去查找类型的过程
		/// </summary>
		/// <param name="type"></param>
		public RType(Type type, int genericCount = -1, params Type[] types)
		{
			SetID();
			this.type = type;
			this.genericCount = genericCount;
			this.types = types;
			name = type.GetFullName();
			memberList.Clear();
			OnInit();
		}

		protected virtual void OnInit()
		{

		}

		public RType(object instance) : this(instance.GetType())
		{
			SetInstance(instance);
		}

		public virtual void SetValue(object value)
		{
			SetInstance(value);
		}

		public virtual Object GetValue()
		{
			return instance;
		}

		public static T CreateInatance<T>() where T : RType, new()
		{
			T rt = new T();
			var instace = Activator.CreateInstance(rt.type);
			rt.SetInstance(instace);
			return rt;
		}

		/// <summary>
		/// 设置实例
		/// </summary>
		/// <param name="instance"></param>
		public void SetInstance(Object instance)
		{
			// 如果是同一个，无需继续操作
			if (this.instance == instance)
			{
				return;
			}

			var instanceType = instance?.GetType();
			if (instanceType != null && !type.IsAssignableFrom(instanceType))
			{
				ReflectionUtils.LogError($"{instance} is not type {type}");
				return;
			}

			this.instance = instance;

			// 给反射成员设置所属对象
			if (memberList != null && memberList.Count > 0)
			{
				foreach (var member in memberList)
				{
					member.SetBelong(this);
				}
			}

			OnSetInstance();
		}

		/// <summary>
		/// 子类重写的函数，不建议直接重写SetInstance
		/// 反射类设置好实例之后
		/// </summary>
		protected virtual void OnSetInstance()
		{

		}

		/// <summary>
		/// 添加反射类成员
		/// </summary>
		/// <param name="belongMember"></param>
		public void AddMember(RMember member)
		{
			memberList.Add(member);
		}

		/// <summary>
		/// 获取对象的模板参数
		/// </summary>
		/// <returns></returns>
		public Type[] GetGenericTypeArguments()
		{
			if (type.IsGenericType)
			{
				return type.GenericTypeArguments;
			}
			return null;
		}

		public RMember CreateMember(string name)
		{
			return new RMember(this, name);
		}

		public RField CreateField(string name)
		{
			return new RField(this, name);
		}

		public RProperty CreateProperty(string name)
		{
			return new RProperty(this, name);
		}

		public RMethod CreateMethod(string name)
		{
			return new RMethod(this, name);
		}

		public REvent CreateEvent(string name)
		{
			return new REvent(this, name);
		}

		/// <summary>
		/// Name:变量名
		/// ReflectedType：当前变量所在的变量
		/// MemberType：RProperty, Field等类型
		/// DeclaringType：定义这个变量的类型位置
		/// </summary>
		public void ShowMembers()
		{
			var list = type.GetMembers(flags);
			foreach (var item in list)
			{
				ReflectionUtils.Log("Name:\t\t" + item.Name + "\nReflectedType:\t" + item.ReflectedType + "\nMemberType:\t" + item.MemberType + "\nDeclaringType:\t" + item.DeclaringType);
			}
		}

		protected void SetID()
		{
			id = _idGenerater++;
		}

		/// <summary>
		/// 显示内部成员的值
		/// </summary>
		public void ShowMembersValue(string desc = "")
		{
			Object instance = GetValue();
			if (instance == null)
			{
				return;
			}

			ReflectionUtils.Log("");
			ReflectionUtils.Log("----------------------------" + name + "  " + desc + " begin--------------------------------");
			var list = type.GetMembers(flags);
			foreach (var item in list)
			{
				if (item.MemberType == MemberTypes.Property)
				{
					PropertyInfo info = item as PropertyInfo;
					object value = RProperty.GetPropertyValue(info, instance);
					ReflectionUtils.Log("Name:\t\t" + item.Name + "\nvalue:\t\t" + value + "\nReflectedType:\t" + item.ReflectedType + "\nMemberType:\t" + item.MemberType + "\nPropertyType:\t" + info.PropertyType + "\ndesc:\t\t" + desc);
				}
				else if (item.MemberType == MemberTypes.Field)
				{
					FieldInfo info = item as FieldInfo;
					object value = RField.GetFieldValue(info, instance);
					ReflectionUtils.Log("Name:\t\t" + item.Name + "\nvalue:\t\t" + value + "\nReflectedType:\t" + item.ReflectedType + "\nMemberType:\t" + item.MemberType + "\nFieldType:\t" + info.FieldType + "\ndesc:\t\t" + desc);
				}
			}
			ReflectionUtils.Log("----------------------------" + name + "  " + desc + " end--------------------------------");
			ReflectionUtils.Log("");
		}
	}
}