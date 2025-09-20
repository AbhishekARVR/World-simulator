using UnityEngine;

public struct PerlinNoiseResult
{
    public float[,] Heights;
    public RenderTexture Texture;
    
    public PerlinNoiseResult(float[,] heights, RenderTexture texture)
    {
        Heights = heights;
        Texture = texture;
    }
}

public static class PerlinNoiseGenerator
{
    #region Static Compute Shader
    private static ComputeShader _perlinComputeShader;
    
    private static ComputeShader PerlinComputeShader
    {
        get
        {
            if (_perlinComputeShader == null)
            {
                _perlinComputeShader = Resources.Load<ComputeShader>("PerlinNoise");
                if (_perlinComputeShader == null)
                {
                    Debug.LogError("PerlinNoise compute shader not found in Resources folder!");
                }
            }
            return _perlinComputeShader;
        }
    }
    #endregion

    #region Perlin functions 

    public static void UpdatePerlinIntoTexture(RenderTexture targetTexture, int octaves, float lacunarity, float persistence, float scale, float inputOffsetX, float inputOffsetY, int seed, float amplitude, int resolution, float terraceHeight)
    {
        if (targetTexture == null)
        {
            Debug.LogError("Target RenderTexture is null in UpdatePerlinIntoTexture.");
            return;
        }

        if (!targetTexture.IsCreated() || targetTexture.width != resolution || targetTexture.height != resolution)
        {
            targetTexture.Release();
            targetTexture.width = resolution;
            targetTexture.height = resolution;
            targetTexture.enableRandomWrite = true;
            targetTexture.Create();
        }

        // Dispatch compute into the provided texture, skip CPU readback
        int size = Mathf.Max(1, resolution);
        int total = size * size;

        System.Random prng = new System.Random(seed);
        float offsetX = ((float)prng.NextDouble()*2f - 1f) * 100000f;
        float offsetY = ((float)prng.NextDouble()*2f - 1f) * 100000f;

        float maxHeight = amplitude * (1 - Mathf.Pow(persistence, (float)octaves) / 1 - persistence);

        int kernel = PerlinComputeShader.FindKernel("CSMain");

        // Minimal buffer to satisfy kernel if it expects Heights; allocate and discard without GetData to avoid stall
        ComputeBuffer buffer = new ComputeBuffer(total, sizeof(float));
        PerlinComputeShader.SetBuffer(kernel, "Heights", buffer);
        PerlinComputeShader.SetTexture(kernel, "Result", targetTexture);

        PerlinComputeShader.SetInt("_Resolution", size);
        PerlinComputeShader.SetInt("_Octaves", Mathf.Max(1, octaves));
        PerlinComputeShader.SetFloat("_Lacunarity", lacunarity);
        PerlinComputeShader.SetFloat("_Persistence", persistence);
        PerlinComputeShader.SetFloat("_Scale", Mathf.Max(0.0001f, scale));
        PerlinComputeShader.SetFloat("_Amplitude", amplitude);
        PerlinComputeShader.SetFloat("_OffsetX", offsetX);
        PerlinComputeShader.SetFloat("_OffsetY", offsetY);
        PerlinComputeShader.SetFloat("_OffsetManualX", inputOffsetX);
        PerlinComputeShader.SetFloat("_OffsetManualY", inputOffsetY);
        PerlinComputeShader.SetFloat("_MaxHeight", maxHeight);
        PerlinComputeShader.SetFloat("_TerraceHeight", terraceHeight);

        uint tx, ty, tz;
        PerlinComputeShader.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
        int groupsX = Mathf.CeilToInt(size / (float)tx);
        int groupsY = Mathf.CeilToInt(size / (float)ty);

        PerlinComputeShader.Dispatch(kernel, groupsX, groupsY, 1);

        buffer.Dispose();
    }

    #endregion
}