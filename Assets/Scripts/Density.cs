using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Density
{
    public static float Calculate(Vector3 position)
    {
        float density = Perlin.Noise(position * 12) * 3;

        return density;
    }

    public static float Calculate(float x, float y, float z)
    {
        return Calculate(new Vector3(x, y, z));
    }
}
