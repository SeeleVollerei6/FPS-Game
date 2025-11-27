using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action<Vector3, float> OnGunshotFired;

    public static void TriggerGunshot(Vector3 position, float range)
    {
        OnGunshotFired?.Invoke(position, range);
    }
}

