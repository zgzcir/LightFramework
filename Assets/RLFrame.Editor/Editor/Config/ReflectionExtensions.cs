using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

public static class ReflectionExtensions 
{
   public static object GetMemberValue(this object obj, string memberName,
      BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
   {
      object value = null;
      Type type = obj.GetType();
      MemberInfo[] memberInfos =
         type.GetMember(memberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
      if (memberInfos.Length <= 0)
      {
         Debug.LogError($"获取成员失败请检查: {memberName} in {obj} ");
         return null;
      }

      switch (memberInfos[0].MemberType)
      {
         case MemberTypes.Field:
            value = type.GetField(memberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
               ?.GetValue(obj);
            break;
         case MemberTypes.Property:
            value = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
               ?.GetValue(obj);
            break;
      }
      return value;
   }
   public static void SetValue(this object obj, string variableName, string value, string type)
   {
      object val = value;
      PropertyInfo propertyInfo = obj.GetType().GetProperty(variableName);
      switch (type)
      {
         case "int":
            val = Convert.ToInt32(val);
            break;
         case "bool":
            val = Convert.ToBoolean(val);
            break;
         case "float":
            val = Convert.ToSingle(val);
            break;
         case "enum":
            val = TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFromString(value);
            break;
      }

      propertyInfo?.SetValue(obj, val);
   }

   public static int GetListCount(this object list)
   {
      return Convert.ToInt32(list.GetType().InvokeMember("get_Count",
         BindingFlags.Default | BindingFlags.InvokeMethod, null, list, null));
   }
   
   
   
   /// <summary>
   /// 获取链表的每一项值
   /// </summary>
   /// <param name="list"></param>
   /// <param name="index"></param>
   /// <returns></returns>
   public static object GetListItemValue(this object list, int index)
   {
      return list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null,
         list, new object[] {index});
   }


   #region creat

   public static object CreateInstance(string className)
   {
      object obj = null;
      Type type = null;

      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
         type = assembly.GetType(className);
         if (type != null)
         {
            break;
         }
      }

      if (type != null)
      {
         obj = Activator.CreateInstance(type);
      }

      return obj;
   }
   public static object CreateList(Type type)
   {
      Type listType = typeof(List<>);
      Type specType = listType.MakeGenericType(type);
      return Activator.CreateInstance(specType);
   }
   #endregion
   public static Type GetTypeByClassName(string className)
   {
      Type type = null;
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
         type = assembly.GetType(className);
         if (type != null) break;
      }

      return type;
   }
   #region reflectionTools

//    private static object GetMemberValue(object obj, string memberName,
//        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
//    {
//        object value = null;
//        Type type = obj.GetType();
//        MemberInfo[] memberInfos =
//            type.GetMember(memberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
//        if (memberInfos.Length <= 0)
//        {
//            Debug.LogError($"获取成员失败请检查: {memberName} in {obj} ");
//            return null;
//        }
//
//        switch (memberInfos[0].MemberType)
//        {
//            case MemberTypes.Field:
//                value = type.GetField(memberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
//                    ?.GetValue(obj);
//                break;
//            case MemberTypes.Property:
//                value = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
//                    ?.GetValue(obj);
//                break;
//        }
//
//        return value;
//    }

   #endregion
}
