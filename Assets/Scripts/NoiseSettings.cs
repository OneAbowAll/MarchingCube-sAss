using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseSettings : ScriptableObject
{
    [Header("Noise Main Settings")]
    public Vector3 offsetPosition = Vector3.zero;
    public float frequency = 1f;
    public float amplitude = 1f;
    public float minValue = 0f;

    [Header("Octaves Settings")]
    public int octaves = 1;
    public float roughness = 2f;
    public float persistence = 0.5f;

    public float Generate(Vector3 worldPosition)
    {
        float noise = 0;
        float freq = frequency;
        float ampl = amplitude;
        Vector3 position = worldPosition + offsetPosition;

        for (int i = 0; i < octaves; i++)
        {
            noise += Perlin.Noise(position * (freq + i)) * ampl;
            ampl *= persistence;
            freq *= roughness;
        }

        noise = Mathf.Min(0, noise + minValue);
        //noise = Mathf.Max(0, noise - minValue);
        return noise; //Perlin.Noise(position * frequency) * amplitude;
    }
}
