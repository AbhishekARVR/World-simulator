using UnityEngine;

[ExecuteAlways]
public class PerlinTerrainGenerator : MonoBehaviour
{
    public Terrain terrain;
    public int octaves = 4;
    public float lacunarity = 2.0f;
    public float persistence = 0.5f;
    public float scale = 10.0f;
    public int seed = 0;
    public float amplitude = 1.0f;
    public float minHeight = 0.0f;
    void OnValidate() {
        GenerateTerrain();
    }

    public void GenerateTerrain() {
        if (terrain == null) return;

        int width = terrain.terrainData.heightmapResolution;
        int height = terrain.terrainData.heightmapResolution;
        Debug.Log("Generating terrain with width: " + width + " and height: " + height);
        float[,] heights = new float[width, height];

        System.Random prng = new System.Random(seed);
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float freq = scale, amp = amplitude, noiseHeight = 0f;
                for (int o = 0; o < octaves; o++) {
                    float sampleX = (x / (float)width) * freq + offsetX;
                    float sampleY = (y / (float)height) * freq + offsetY;
                    float perlin = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlin * amp;
                    freq *= lacunarity;
                    amp *= persistence;
                }
                if(noiseHeight < minHeight) {
                    noiseHeight = minHeight;
                }
                heights[x, y] = noiseHeight;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
    }
}