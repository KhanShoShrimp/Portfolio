using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updater : MonoBehaviour
{
	public static event Action UpdateEvent;
	public static event Action DestroyEvent;

	private void Update()
	{
		UpdateEvent?.Invoke();
	}

	private void OnDestroy()
	{
		DestroyEvent?.Invoke();
	}
}
