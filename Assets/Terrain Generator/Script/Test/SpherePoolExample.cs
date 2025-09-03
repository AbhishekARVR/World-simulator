using UnityEngine;

public class SpherePoolExample : MonoBehaviour
{
    [Header("Example Settings")]
    [SerializeField] private KeyCode spawnKey = KeyCode.Space;
    [SerializeField] private KeyCode returnKey = KeyCode.R;
    [SerializeField] private KeyCode clearKey = KeyCode.C;
    [SerializeField] private int spheresToSpawn = 10;
    
    private SphereObjectPool spherePool;
    
    private void Start()
    {
        spherePool = GetComponent<SphereObjectPool>();
        if (spherePool == null)
        {
            spherePool = gameObject.AddComponent<SphereObjectPool>();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnRandomSpheres();
        }
        
        if (Input.GetKeyDown(returnKey))
        {
            ReturnAllSpheres();
        }
        
        if (Input.GetKeyDown(clearKey))
        {
            ClearPool();
        }
    }
    
    private void SpawnRandomSpheres()
    {
        for (int i = 0; i < spheresToSpawn; i++)
        {
            GameObject sphere = spherePool.GetSphere();
            sphere.transform.position = Random.insideUnitSphere * 5f;
            sphere.transform.SetParent(transform);
        }
        
        Debug.Log($"Spawned {spheresToSpawn} spheres. Active: {spherePool.GetActiveSphereCount()}, Available: {spherePool.GetAvailableSphereCount()}");
    }
    
    private void ReturnAllSpheres()
    {
        spherePool.ReturnAllSpheres();
        Debug.Log("All spheres returned to pool");
    }
    
    private void ClearPool()
    {
        spherePool.ClearPool();
        Debug.Log("Pool cleared");
    }
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("Sphere Object Pool Example");
        GUILayout.Label($"Active Spheres: {spherePool?.GetActiveSphereCount() ?? 0}");
        GUILayout.Label($"Available Spheres: {spherePool?.GetAvailableSphereCount() ?? 0}");
        GUILayout.Label($"Total Pool Size: {spherePool?.GetTotalPoolSize() ?? 0}");
        GUILayout.Label($"Press {spawnKey} to spawn {spheresToSpawn} spheres");
        GUILayout.Label($"Press {returnKey} to return all spheres");
        GUILayout.Label($"Press {clearKey} to clear pool");
        GUILayout.EndArea();
    }
}
