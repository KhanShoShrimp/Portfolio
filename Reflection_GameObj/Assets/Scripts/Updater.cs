using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updater : MonoBehaviour
{
    static Updater instance = null;
    static event Action actions;
    public static event Action UpdateEvent
	{
        add
		{
			if (instance == null)
			{
				GameObject obj = new GameObject("Updater");
				DontDestroyOnLoad(obj);
				instance = obj.AddComponent<Updater>();
			}
			actions += value;
		}
		remove
		{
			actions -= value;
		}
	}

    private void Update()
    {
        actions?.Invoke();
    }
}
