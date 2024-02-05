using UnityEngine;

public static class MathUtils
{
    public static float SignedLerp(float value, float minValue, float maxValue)
    {
        return Mathf.InverseLerp(minValue, maxValue, Mathf.Abs(value)) * Mathf.Sign(value); ;
    }
}
