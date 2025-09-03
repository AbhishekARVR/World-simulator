using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTextureGenerator : PerlinNoiseGenerator
{
    #region Variable declaration
    [SerializeField] private PerlinSphereVisualisation perlinSphereVisualisation;
    private float[,] perlinNoise;
    private Texture2D texture;
    private MeshRenderer meshRenderer;

    #endregion

    #region Unity methods   
    private void OnValidate() 
    {
        perlinNoise = GeneratePerlinNoise(perlinSphereVisualisation.resolution);
        texture = GenerateTexture(perlinNoise);

        if(meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = texture;
    }

    #endregion

    #region Methods
    public Texture2D GenerateTexture(float[,] perlinNoise)
    {
        Texture2D texture = new Texture2D(perlinNoise.GetLength(0), perlinNoise.GetLength(1));
        
        for (int x = 0; x < perlinNoise.GetLength(0); x++)
        {
            for (int y = 0; y < perlinNoise.GetLength(1); y++)
            {
                texture.SetPixel(x, y, new Color(perlinNoise[x, y], perlinNoise[x, y], perlinNoise[x, y]));
            }
        }

        texture.Apply();
        perlinSphereVisualisation.GeneratePerlinSphereVisualisation(perlinNoise);
        return texture;
    }

    #endregion
}
