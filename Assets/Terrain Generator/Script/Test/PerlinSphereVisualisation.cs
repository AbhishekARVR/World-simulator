using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinSphereVisualisation : PerlinVerticesGenerator
{
    #region Variable declaration

    [SerializeField] private float radius = 1f;
    
    [Header("Object Pooling")]
    [SerializeField] private SphereObjectPool spherePool;


    private List<GameObject> spawnedSpheres = new List<GameObject>();

    #endregion

    #region Unity Methods

    private void OnValidate() 
    {
        if (spherePool == null) return;
        if (!gameObject.scene.IsValid()) return;

        #if UNITY_EDITOR
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;
        #endif

        Vector3[,] perlinVertices = GeneratePerlinVertices();
        GeneratePerlinSphereVisualisation(perlinVertices);
    }

    #endregion

    #region Sphere Generation
    public void GeneratePerlinSphereVisualisation(Vector3[,] perlinVertices)
    {
        ClearCurrentSpheres();
                 
        for (int i = 0; i < perlinVertices.GetLength(0); i++)
        {
            Debug.Log(i);
            for (int j = 0; j < perlinVertices.GetLength(1); j++)
            {                          
                GameObject sphere = spherePool.GetSphere();
                sphere.transform.position = perlinVertices[i,j];
                sphere.transform.parent = transform;
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
