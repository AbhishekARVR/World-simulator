using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinSphereVisualisation : MonoBehaviour
{
    #region Variable declaration
    [Header("Visualization Settings")]
    [Range(10,1000)] public int resolution = 10;
    [SerializeField] private float radius = 1f;
    
    [Header("Object Pooling")]
    [SerializeField] private SphereObjectPool spherePool;
    
    private List<GameObject> spawnedSpheres = new List<GameObject>();
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (spherePool == null)
        {
            spherePool = GetComponent<SphereObjectPool>();
            if (spherePool == null)
            {
                spherePool = gameObject.AddComponent<SphereObjectPool>();
            }
        }
    }

    #endregion

    #region Sphere Generation
    public void GeneratePerlinSphereVisualisation(float[,] perlinNoiseData)
    {
        float minHeight=100;
        float maxHeight=0;
        
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                if (perlinNoiseData[i,j] < minHeight) minHeight = perlinNoiseData[i,j];
                else if (perlinNoiseData[i,j] > maxHeight) maxHeight = perlinNoiseData[i,j];
            }
        }

        float heightResolution = maxHeight - minHeight;

        ClearCurrentSpheres();
                 
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {           
                // Calculate sphere position
                float x = -5f +((float)i/resolution*10f);
                float z = -5f + ((float)j/resolution*10f);

                float y = (perlinNoiseData[i,j] - minHeight)/heightResolution;
                
                Vector3 position = new Vector3(x, y, z);
                
                // Get sphere from pool and position it
                GameObject sphere = spherePool.GetSphere();
                sphere.transform.position = position;
                
                spawnedSpheres.Add(sphere);
            }
        }
    }
    
    private void ClearCurrentSpheres()
    {
        foreach (GameObject sphere in spawnedSpheres)
        {
            if (sphere != null)
            {
                spherePool.ReturnSphere(sphere);
            }
        }
        spawnedSpheres.Clear();
    }
    
    public void ClearVisualization()
    {
        ClearCurrentSpheres();
    }
    #endregion
}
