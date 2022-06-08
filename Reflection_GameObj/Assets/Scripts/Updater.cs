using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updater : MonoBehaviour
{
    public static event Action Actions;

    private void Update()
    {
        Actions?.Invoke();
    }
}
