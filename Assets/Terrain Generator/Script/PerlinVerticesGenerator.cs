using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinVerticesGenerator : PerlinNoiseGenerator
{
    #region Variable declaration
    
    [SerializeField] [Range(0,10)] private float horizontalRange; 
    [SerializeField] [Range(1,10)] private float heightRange; 
    #endregion
    public Vector3[,] GeneratePerlinVertices()
    {
        float[,] perlinNoiseData = GeneratePerlinNoise();

        float minHeight=100;
        float maxHeight=0;
        
        Vector3[,] perlinVertices = new Vector3[resolution,resolution];

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                if (perlinNoiseData[i,j] < minHeight) minHeight = perlinNoiseData[i,j];
                else if (perlinNoiseData[i,j] > maxHeight) maxHeight = perlinNoiseData[i,j];
            }
        }

        float perlinHeightRange = maxHeight - minHeight;
                 
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {           
                float x = - horizontalRange + ((float)i/resolution*horizontalRange*2f);
                float z = - horizontalRange + ((float)j/resolution*horizontalRange*2f);

                float normalizedPerlinNoise = (perlinNoiseData[i,j] - minHeight)/perlinHeightRange;             
                float y = heightRange * normalizedPerlinNoise;    
                
                Vector3 position = new Vector3(x, y, z);

                perlinVertices[i,j] = position;
            }
        }

        return perlinVertices;
    }
}
