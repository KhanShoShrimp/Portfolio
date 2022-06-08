using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ReferenceAttribute : Attribute 
{
    public bool destroyOldObject = false;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SetDefaultAttribute : Attribute 
{
    public bool getChild = false;
}

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

					if (attribute.getChild)
					{
                        field.SetValue(this, GetComponentInChildren(type));
                    }
                    else
					{
                        field.SetValue(this, GetComponent(type));
                    }
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
                            field.SetValue(this, GetArrayMethod.MakeGenericMethod(elementType).Invoke(this, new object[1] { attribute.getChild }));
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
                            field.SetValue(this, GetListMethod.MakeGenericMethod(elementType).Invoke(this, new object[1] { attribute.getChild }));
                        }
                    }
                }
            }
        }
    }

    private T[] GetArray<T>(bool getChild) where T : Component
    {
		if (getChild)
        {
            var list = new List<T>();
            GetChild(transform, list);
            return list.ToArray();
        }
		else
		{
            return transform.GetComponents<T>();
        }
    }

    private List<T> GetList<T>(bool getChild) where T : Component
    {
		if (getChild)
        {
            var list = new List<T>();
            GetChild(transform, list);
            return list;
        }
		else
		{
            return new List<T>(transform.GetComponents<T>());
        }
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
        var attribute = GetType().GetCustomAttribute<ReferenceAttribute>();

        if (attribute != null)
        {
            InsertMethod.MakeGenericMethod(GetType()).Invoke(this, new object[2] { this, attribute.destroyOldObject });
        }
    }

    private void SetIntance<T>(T t, bool destroyOldObject) where T : Component
    {
		if (destroyOldObject)
		{
            DestroyImmediate(Ref<T>.Ins);
		}
        Ref<T>.Ins = t;
    }
    #endregion
    #region Update
    protected virtual void UpdateMethod() { }

    readonly static Type s_baseType = typeof(GameObj);

    private void AddUpdate()
    {
        if (GetType().GetMethod("UpdateMethod", PRIVATE_BINDING).DeclaringType != s_baseType)
        {
            Updater.UpdateEvent += UpdateMethod;
        }
    }

    private void RemoveUpdate()
    {
        Updater.UpdateEvent -= UpdateMethod;
    }
    #endregion
}