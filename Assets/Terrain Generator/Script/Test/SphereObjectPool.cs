using System.Collections.Generic;
using UnityEngine;

public class SphereObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 100;
    [SerializeField] private int maxPoolSize = 1000;
    [SerializeField] private Vector3 sphereScale = new Vector3(0.1f, 0.1f, 0.1f);
    
    private GameObject spherePrefab;
    private Queue<GameObject> availableSpheres = new Queue<GameObject>();
    private List<GameObject> activeSpheres = new List<GameObject>();
    [SerializeField] private bool isPrewarmed;
    
    private void PrewarmPool()
    {
        activeSpheres = new List<GameObject>();
        availableSpheres = new Queue<GameObject>();
        
        if (spherePrefab == null)
        {
            // Create a default sphere if no prefab is assigned
            spherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spherePrefab.transform.localScale = sphereScale;
            spherePrefab.transform.parent = transform;
            DestroyImmediate(spherePrefab.GetComponent<SphereCollider>());
            spherePrefab.SetActive(false);
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSphere();
        }

        isPrewarmed = true;
    }
    
    private void CreateNewSphere()
    {
        if(spherePrefab == null) PrewarmPool();
        GameObject sphere = Instantiate(spherePrefab);
        
        #if UNITY_EDITOR
                bool canParent = gameObject.scene.IsValid() && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
        #else
                bool canParent = gameObject.scene.IsValid();
        #endif

        if (canParent)
        {
            sphere.transform.SetParent(transform, false);
        }
        sphere.transform.localScale = sphereScale;
        sphere.SetActive(false);
        availableSpheres.Enqueue(sphere);
    }
    
    public GameObject GetSphere()
    {
        if(!isPrewarmed) PrewarmPool();
        
        GameObject sphere;
        
        if (availableSpheres.Count > 0)
        {
            sphere = availableSpheres.Dequeue();
        }
        else if (activeSpheres.Count < maxPoolSize)
        {
            CreateNewSphere();
            sphere = availableSpheres.Dequeue();
        }
        else
        {
            // Reuse the oldest active sphere
            Debug.Log(activeSpheres);
            sphere = activeSpheres[0];
            activeSpheres.RemoveAt(0);
            TrySetParentToPool(sphere.transform);
        }
        
        sphere.SetActive(true);
        activeSpheres.Add(sphere);
        return sphere;
    }
    
    public void ReturnSphere(GameObject sphere)
    {
        if (sphere == null) return;
        
        sphere.SetActive(false);
        TrySetParentToPool(sphere.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localRotation = Quaternion.identity;
        
        if (activeSpheres.Contains(sphere))
        {
            activeSpheres.Remove(sphere);
        }
        
        availableSpheres.Enqueue(sphere);
    }

    private void TrySetParentToPool(Transform child)
    {
#if UNITY_EDITOR
        bool canParent = gameObject.scene.IsValid() && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
#else
        bool canParent = gameObject.scene.IsValid();
#endif
        if (canParent)
        {
            child.SetParent(transform, false);
        }
    }
    
    public void ReturnAllSpheres()
    {
        for (int i = activeSpheres.Count - 1; i >= 0; i--)
        {
            ReturnSphere(activeSpheres[i]);
        }
    }
    
    public void ClearPool()
    {
        ReturnAllSpheres();
        
        while (availableSpheres.Count > 0)
        {
            GameObject sphere = availableSpheres.Dequeue();
            if (sphere != null)
            {
                DestroyImmediate(sphere);
            }
        }
    }
    
    public int GetActiveSphereCount()
    {
        return activeSpheres.Count;
    }
    
    public int GetAvailableSphereCount()
    {
        return availableSpheres.Count;
    }
    
    public int GetTotalPoolSize()
    {
        return activeSpheres.Count + availableSpheres.Count;
    }
}
