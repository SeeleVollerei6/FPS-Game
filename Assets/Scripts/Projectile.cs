using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Projectile
{
    public static Vector3 CalculatePosition(Vector3 startPosition, Vector3 initialVelocity, float time)
    {
        return startPosition + initialVelocity * time + 0.5f * Physics.gravity * time * time;
    }
}