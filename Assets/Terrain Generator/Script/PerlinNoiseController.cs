using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PerlinNoiseController : MonoBehaviour
{
    #region Variable Declaration
    
    [Serializable]
    private struct heightBand
    {
        public float maxHeight;
        public Color minHeightColor;
        public Color maxHeightColor;
    }
        
    public int seed;

    private int octaves = 4;
    private float lacunarity = 1.69f; 
    private float persistence = 0.03f;
    private float scale = 5f;
    private float inputOffsetX = 0f;
    private float inputOffsetY = 0f;
    float maxOffset;
    private float amplitude = 1.0f;
    private int resolution = 1000;
    private float terraceHeight = 0.0001f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 1.0f;

    private Renderer perlinRenderer;
    private RenderTexture perlinTexture;
    [Header("Update Control")]
    public float maxUpdatesPerSecond = 30f;
    private float nextUpdateTime;

    [SerializeField] private List<heightBand> heightBands = new List<heightBand>();
    #endregion

    #region Unity Methods

    private void Start()
    {
        perlinRenderer = GetComponent<Renderer>();

        GenerateMesh();
        AllocateTexture();
        UpdatePerlinTexture();

        float[] maxHeights = heightBands.Select(b => b.maxHeight).ToArray();
        Color[] minHeightColors = heightBands.Select(b => b.minHeightColor).ToArray();
        Color[] maxHeightColors = heightBands.Select(b => b.maxHeightColor).ToArray();
        int bandCount = heightBands.Count;

        perlinRenderer.material.SetFloatArray("_MaxHeights", maxHeights);
        perlinRenderer.material.SetColorArray("_MinHeightColors", minHeightColors);
        perlinRenderer.material.SetColorArray("_MaxHeightColors", maxHeightColors);
        perlinRenderer.material.SetInt("_BandCount", bandCount);
    }

    private void Update()
    {
        if(HandleWASDInput())
        {
            if (Time.time >= nextUpdateTime)
            {
                UpdatePerlinTexture();
                float interval = maxUpdatesPerSecond > 0 ? 1f / maxUpdatesPerSecond : 0f;
                nextUpdateTime = Time.time + interval;
            }
        }
    }
    
    #endregion

    #region Input Handling
    
    private bool HandleWASDInput()
    {
        float horizontal = 0f;
        float vertical = 0f;
        float scale = 0f;

        bool isInputDone = false;

        if (Input.GetKey(KeyCode.W))
        {
            vertical -= 1f;
            isInputDone = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            vertical += 1f;
            isInputDone = true;
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            horizontal += 1f;
            isInputDone = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            horizontal -= 1f;
            isInputDone = true;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            scale += 1f;
            isInputDone = true;
        }
        if (Input.GetKey(KeyCode.E))
        {
            scale -= 1f;
            isInputDone = true;
        }

        if (!isInputDone) return false;
        
        // Apply movement to offset values
        inputOffsetX += horizontal * moveSpeed * Time.deltaTime;
        inputOffsetY += vertical * moveSpeed * Time.deltaTime;
        this.scale += scale * moveSpeed * Time.deltaTime;
        this.scale = Mathf.Clamp(this.scale,1f,5f);

        maxOffset = 2.5f - this.scale*0.5f;

        inputOffsetX = Mathf.Clamp(inputOffsetX,-maxOffset,maxOffset);
        inputOffsetY = Mathf.Clamp(inputOffsetY,-maxOffset,maxOffset);

        return true;
    }
    
    #endregion

    #region Noise Generation
        
    private void AllocateTexture()
    {
        if (perlinTexture == null)
        {
            perlinTexture = new RenderTexture(resolution, resolution, 0);
            perlinTexture.enableRandomWrite = true;
            perlinTexture.Create();
        }
        else if (perlinTexture.width != resolution || perlinTexture.height != resolution)
        {
            perlinTexture.Release();
            perlinTexture.width = resolution;
            perlinTexture.height = resolution;
            perlinTexture.enableRandomWrite = true;
            perlinTexture.Create();
        }
        perlinRenderer.material.SetTexture("_MainTex", perlinTexture);
    }

    private void UpdatePerlinTexture()
    {
        PerlinNoiseGenerator.UpdatePerlinIntoTexture(perlinTexture, octaves, lacunarity, persistence, scale, inputOffsetX, inputOffsetY, seed, amplitude, resolution, terraceHeight);
    }

    private void GenerateMesh()
    {
        float width  = 10f;
        float length = 10f;
            
        var mesh = new Mesh {
            name = "HighResGrid",
            indexFormat = IndexFormat.UInt32  // ← allow >65k vertices
        };
        GetComponent<MeshFilter>().mesh = mesh;

        int vertsPerLine = resolution + 1;
        Vector3[] vertices = new Vector3[vertsPerLine * vertsPerLine];
        Vector2[] uvs   = new Vector2[vertices.Length];
        int[] trianlges     = new int[resolution * resolution * 6];

        float halfW = width * 0.5f;
        float halfL = length * 0.5f;

        for (int i = 0, y = 0; y <= resolution; y++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float px = ((float)x / resolution) * width  - halfW;
                float pz = ((float)y / resolution) * length - halfL;
                vertices[i] = new Vector3(px, 0f, pz);
                uvs[i]      = new Vector2((float)x / resolution, (float)y / resolution);
            }
        }

        for (int y = 0, ti = 0, vi = 0; y < resolution; y++, vi++)
        for (int x = 0; x < resolution; x++, ti += 6, vi++)
        {
            trianlges[ti    ] = vi;
            trianlges[ti + 1] = vi + vertsPerLine;
            trianlges[ti + 2] = vi + 1;
            trianlges[ti + 3] = vi + 1;
            trianlges[ti + 4] = vi + vertsPerLine;
            trianlges[ti + 5] = vi + vertsPerLine + 1;
        }

        mesh.vertices  = vertices;
        mesh.triangles = trianlges;
        mesh.uv        = uvs;
        mesh.RecalculateNormals();
    }

    #endregion
    
}
