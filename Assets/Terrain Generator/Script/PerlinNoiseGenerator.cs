using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseGenerator : MonoBehaviour
{
    #region Variable declaration

    [Header("Perlin Noise Settings")]
    [SerializeField] private int octaves = 4;
    [SerializeField] private float lacunarity = 2.0f;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float scale = 10.0f;
    [SerializeField] private int seed = 0;
    [SerializeField] private float amplitude = 1.0f;
    [SerializeField] private float minHeight = 0.0f;

    #endregion

    #region Methods

    public float[,] GeneratePerlinNoise(int resolution) 
    {
        float[,] heights = new float[resolution, resolution];

        System.Random prng = new System.Random(seed);
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int x = 0; x < resolution; x++) 
        {
            for (int y = 0; y < resolution; y++) 
            {
                float freq = scale, amp = amplitude, noiseHeight = 0f;
                for (int o = 0; o < octaves; o++) 
                {
                    float sampleX = x / (float)resolution * freq + offsetX;
                    float sampleY = y / (float)resolution * freq + offsetY;
                    float perlin = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlin * amp;
                    freq *= lacunarity;
                    amp *= persistence;
                }

                heights[x, y] = noiseHeight;
                if(x == 0) Debug.Log(noiseHeight);
            }
        }

        return heights;
    }

    #endregion
}
