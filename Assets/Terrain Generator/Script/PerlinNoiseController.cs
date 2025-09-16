using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseController : MonoBehaviour
{
    #region Variable Declaration
    
    public int seed;

    private int octaves = 4;
    private float lacunarity = 1.69f; 
    private float persistence = 0.01f;
    private float scale = 5f;
    private float inputOffsetX = 0f;
    private float inputOffsetY = 0f;
    private float amplitude = 1.0f;
    private int resolution = 1000;
    private float terraceHeight = 0.01f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 1.0f;

    private Renderer perlinRenderer;
    private RenderTexture perlinTexture;
    [Header("Update Control")]
    public float maxUpdatesPerSecond = 30f;
    private float nextUpdateTime;

    #endregion

    #region Unity Methods

    private void Start()
    {
        perlinRenderer = GetComponent<Renderer>();
        AllocateTexture();
        UpdatePerlinTexture();
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

        inputOffsetX = Mathf.Clamp(inputOffsetX,-2.5f,2.5f);
        inputOffsetY = Mathf.Clamp(inputOffsetY,-2.5f,2.5f);
        this.scale = Mathf.Clamp(this.scale,1f,5f);

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

    #endregion
    
}
