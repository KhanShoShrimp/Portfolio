using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ReferenceAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SetDefaultAttribute : Attribute { }

public class Ref<T> where T : Component { public static T Ins = null; }

public abstract class GameObj : MonoBehaviour
{
    const BindingFlags PROPERTY_BINDING = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
    const BindingFlags SERIALIZEFIELD_BINDING = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    const BindingFlags PRIVATE_BINDING = BindingFlags.Instance | BindingFlags.NonPublic;

    protected virtual void Awake()
    {
        GetInstance();
    }

    protected virtual void OnEnable()
    {
        AddUpdate();
    }

    protected virtual void OnDisable()
    {
        RemoveUpdate();
    }
    #region SetDefault
#if UNITY_EDITOR
    readonly static MethodInfo GetArrayMethod = typeof(GameObj).GetMethod("GetArray", PRIVATE_BINDING);
    readonly static MethodInfo GetListMethod = typeof(GameObj).GetMethod("GetList", PRIVATE_BINDING);

    protected virtual void Reset()
    {
        SetDefault();
    }

    [ContextMenu("SetDefault")]
    private void SetDefault()
	{
        SetDefaultAttribute attribute;
        Type type;
        Type elementType;

        foreach (var field in GetType().GetFields(SERIALIZEFIELD_BINDING))
		{
            if ((attribute = field.GetCustomAttribute<SetDefaultAttribute>()) != null)
            {
                type = field.FieldType;

                if (type.IsSubclassOf(typeof(Component)))
                {
                    if (field.GetValue(this) != null)
                    {
                        continue;
                    }

                    field.SetValue(this, GetComponent(type));
                }
				else
				{
                    if (type.IsArray)
                    {
                        elementType = type.GetElementType();

                        if (elementType.IsSubclassOf(typeof(Component)))
                        {
                            if (field.GetValue(this) != null && (int)typeof(Array).GetProperty("Length", PROPERTY_BINDING).GetValue(field.GetValue(this)) > 0)
                            {
                                continue;
                            }

                            field.SetValue(this, GetArrayMethod.MakeGenericMethod(elementType).Invoke(this, new object[0] { }));
                        }
                    }
                    else if (type.IsGenericType)
                    {
                        elementType = type.GetGenericArguments()[0];

                        if (elementType.IsSubclassOf(typeof(Component)))
                        {
                            if (field.GetValue(this) != null && (int)typeof(List<>).MakeGenericType(elementType).GetProperty("Count", PROPERTY_BINDING).GetValue(field.GetValue(this)) > 0)
                            {
                                continue;
                            }

                            field.SetValue(this, GetListMethod.MakeGenericMethod(elementType).Invoke(this, new object[0] { }));
                        }
                    }
                }
            }
        }
    }

    private T[] GetArray<T>() where T : Component
    {
        var list = new List<T>();
        GetChild(transform, list);
        return list.ToArray();
    }

    private List<T> GetList<T>() where T : Component
    {
        var list = new List<T>();
        GetChild(transform, list);
        return list;
    }

    private void GetChild<T>(Transform transform, List<T> result) where T : Component
    {
        result.AddRange(transform.GetComponents<T>());

        for (int i = 0; i < transform.childCount; i++)
        {
            GetChild(transform.GetChild(i), result);
        }
    }
#endif
    #endregion
    #region Reference

    readonly static MethodInfo InsertMethod = typeof(GameObj).GetMethod("SetIntance", PRIVATE_BINDING);

    private void GetInstance()
    {
        if (GetType().GetCustomAttribute<ReferenceAttribute>() != null)
        {
            InsertMethod.MakeGenericMethod(GetType()).Invoke(this, new object[1] { this });
        }
    }

    private void SetIntance<T>(T t) where T : Component
    {
        Ref<T>.Ins = t;
    }
    #endregion
    #region Update
    protected virtual void UpdateMethod() { }

    static Type m_baseType = typeof(GameObj);

    private void AddUpdate()
    {
        if (GetType().GetMethod("UpdateMethod", PRIVATE_BINDING).DeclaringType != m_baseType)
        {
            Updater.Actions += UpdateMethod;
        }
    }

    private void RemoveUpdate()
    {
        Updater.Actions -= UpdateMethod;
    }
    #endregion
}