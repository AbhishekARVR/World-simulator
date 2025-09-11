using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PerlinNoiseGenerator : MonoBehaviour
{
    #region Variable declaration

    [Header("Perlin Noise Settings")]
    // Number of noise layers to accumulate. More octaves = richer detail but more work.
    [SerializeField] private int octaves = 4;
    // Frequency multiplier per octave (>1 increases detail each octave)
    [SerializeField] private float lacunarity = 2.0f;
    // Amplitude multiplier per octave (<1 reduces contribution of higher octaves)
    [SerializeField] private float persistence = 0.5f;
    // Base frequency scale applied to the (x,y) samples
    [SerializeField] private float scale = 10.0f;
    // Seed used to produce deterministic random offsets
    [SerializeField] private int seed = 0;
    // Base amplitude for the first octave
    [SerializeField] private float amplitude = 1.0f;
    // Width/Height of the generated heightmap in samples
    [Range(2,1000)] public int resolution = 2;
    [Range(0,0.5f)] public float terraceHeight = 0.1f;

    [SerializeField] private ComputeShader perlinCompute;
    [SerializeField] private ComputeShader perlinDensityCompute;

    private float lastResolution;
    private RenderTexture perlinNoiseRenderTexture;
    private RenderTexture perlinDensityRenderTexture;
    private Renderer meshRenderer;

    #endregion

    void Start()
    {
        CreateRenderTexture();  
        GeneratePerlinNoise();
    }

    private void CreateRenderTexture()
    {
        perlinNoiseRenderTexture = new RenderTexture(resolution, resolution, 0);
        perlinNoiseRenderTexture.enableRandomWrite = true;
        perlinNoiseRenderTexture.Create();

        meshRenderer = GetComponent<Renderer>();
        meshRenderer.sharedMaterial.SetTexture("_MainTex", perlinNoiseRenderTexture);
    }


    void Update()
    {
        if(resolution != lastResolution) CreateRenderTexture();
        GeneratePerlinNoise();
    }

    #region Methods


    public float[,] GeneratePerlinNoise() 
    {        
        return GeneratePerlinNoiseGPU();
    }

    /// <summary>
    /// CPU implementation of multi-octave 2D Perlin noise.
    /// Matches the previous implementation for parity and determinism.
    /// </summary>
    // private float[,] GeneratePerlinNoiseCPU()
    // {
    //     float[,] heights = new float[resolution, resolution];

    //     System.Random prng = new System.Random(seed);
    //     // Large random offsets reduce visible tiling for small scales
    //     float offsetX = prng.Next(-100000, 100000);
    //     float offsetY = prng.Next(-100000, 100000);

    //     for (int x = 0; x < resolution; x++)
    //     {
    //         for (int y = 0; y < resolution; y++)
    //         {
    //             float freq = scale, amp = amplitude, noiseHeight = 0f;
    //             for (int o = 0; o < octaves; o++)
    //             {
    //                 float sampleX = x / (float)resolution * freq + offsetX;
    //                 float sampleY = y / (float)resolution * freq + offsetY;
    //                 float perlin = Mathf.PerlinNoise(sampleX, sampleY);
    //                 noiseHeight += perlin * amp;
    //                 freq *= lacunarity;
    //                 amp *= persistence;
    //             }

    //             heights[x, y] = noiseHeight;
    //         }
    //     }

    //     return heights;
    // }

    /// <summary>
    /// GPU implementation using a compute shader. Writes results into a ComputeBuffer,
    /// dispatches thread groups over the 2D domain, then reads back into a float[,].
    /// </summary>
    private float[,] GeneratePerlinNoiseGPU()
    {
        int size = Mathf.Max(1, resolution);
        float[,] heights2D = new float[size, size];
        int total = size * size;

        System.Random prng = new System.Random(seed);
        // Keep the same offset logic as CPU for determinism
        float offsetX = ((float)prng.NextDouble()*2f - 1f) * 100000f;;
        float offsetY = ((float)prng.NextDouble()*2f - 1f) * 100000f;;

        float maxHeight = amplitude* (1-Mathf.Pow(persistence,(float)octaves)/1-persistence) ;
        
        int kernel = perlinCompute.FindKernel("CSMain");

        // Allocate a 1D buffer to store size*size floats produced by the GPU
        ComputeBuffer buffer = new ComputeBuffer(total, sizeof(float));
        perlinCompute.SetBuffer(kernel, "Heights", buffer);
        perlinCompute.SetTexture(kernel, "Result", perlinNoiseRenderTexture);

        // Set shader parameters to mirror the CPU loop
        perlinCompute.SetInt("_Resolution", size);
        perlinCompute.SetInt("_Octaves", Mathf.Max(1, octaves));
        perlinCompute.SetFloat("_Lacunarity", lacunarity);
        perlinCompute.SetFloat("_Persistence", persistence);
        perlinCompute.SetFloat("_Scale", Mathf.Max(0.0001f, scale));
        perlinCompute.SetFloat("_Amplitude", amplitude);
        perlinCompute.SetFloat("_OffsetX", offsetX);
        perlinCompute.SetFloat("_OffsetY", offsetY);
        perlinCompute.SetFloat("_MaxHeight", maxHeight);
        perlinCompute.SetFloat("_TerraceHeight", terraceHeight);

        // Determine thread group counts from the kernel's declared numthreads
        uint tx, ty, tz;
        perlinCompute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
        int groupsX = Mathf.CeilToInt(size / (float)tx);
        int groupsY = Mathf.CeilToInt(size / (float)ty);

        // Execute the compute shader over the 2D domain
        perlinCompute.Dispatch(kernel, groupsX, groupsY, 1);

        // Read back into managed memory and release GPU resources
        float[] heights1D = new float[total];
        buffer.GetData(heights1D);
        buffer.Dispose();

        // Convert 1D buffer layout back to 2D [x,y]
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int index = x + y * size;
                heights2D[x, y] = heights1D[index];
            }
        }

        return heights2D;
    }

    private float[,] GeneratePerlinDensity()
    {   
        // int kernel = perlinDensityCompute.FindKernel("CSMain");

        // // Allocate a 1D buffer to store size*size floats produced by the GPU
        // ComputeBuffer buffer = new ComputeBuffer(total, sizeof(float));
        // perlinCompute.SetBuffer(kernel, "Heights", buffer);
        // perlinCompute.SetTexture(kernel, "Result", perlinDensityRenderTexture);

        // // Set shader parameters to mirror the CPU loop
        // perlinCompute.SetInt("_Resolution", size);
        // perlinCompute.SetInt("_Octaves", Mathf.Max(1, octaves));
        // // Determine thread group counts from the kernel's declared numthreads
        // uint tx, ty, tz;
        // perlinCompute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
        // int groupsX = Mathf.CeilToInt(size / (float)tx);
        // int groupsY = Mathf.CeilToInt(size / (float)ty);

        // // Execute the compute shader over the 2D domain
        // perlinCompute.Dispatch(kernel, groupsX, groupsY, 1);

        // // Read back into managed memory and release GPU resources
        float[] heights1D = new float[1];
        // buffer.GetData(heights1D);
        // buffer.Dispose();

        // // Convert 1D buffer layout back to 2D [x,y]
        // for (int x = 0; x < size; x++)
        // {
        //     for (int y = 0; y < size; y++)
        //     {
        //         int index = x + y * size;
        //         heights2D[x, y] = heights1D[index];
        //     }
        // }
        float[,] heights2D = new float[1, 1];
        return heights2D;
    }

    #endregion
}
