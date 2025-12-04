using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject asteroidPrefab;
    public int asteroidCount = 20;
    public Vector3 spawnAreaSize = new Vector3(50f, 50f, 50f);
    public float minDistanceBetweenAsteroids = 5f;
    public float minDistanceFromAgent = 10f;
    public float minDistanceFromGoal = 10f;
    
    [Header("Asteroid Properties")]
    public Vector2 asteroidScaleRange = new Vector2(1f, 3f);
    public float maxDriftSpeed = 2f;
    
    [Header("References")]
    public Transform agentTransform;
    public Transform goalTransform;
    
    private List<GameObject> asteroids = new List<GameObject>();
    private List<Rigidbody> asteroidRigidbodies = new List<Rigidbody>();
    
    public void InitializeAsteroids()
    {
        ClearAsteroids();
        
        for (int i = 0; i < asteroidCount; i++)
        {
            Vector3 position = FindValidPosition();
            GameObject asteroid = CreateAsteroid(position, i);
            asteroids.Add(asteroid);
        }
    }
    
    public void RandomizeAsteroids()
    {
        if (asteroids.Count == 0)
        {
            InitializeAsteroids();
            return;
        }
        
        for (int i = 0; i < asteroids.Count; i++)
        {
            Vector3 position = FindValidPosition();
            asteroids[i].transform.position = position;
            asteroids[i].transform.rotation = Random.rotation;
            
            float scale = Random.Range(asteroidScaleRange.x, asteroidScaleRange.y);
            asteroids[i].transform.localScale = Vector3.one * scale;
            
            Rigidbody rb = asteroids[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                }
                
                rb.linearVelocity = Random.insideUnitSphere * maxDriftSpeed;
                rb.angularVelocity = Random.insideUnitSphere * 0.5f;
            }
        }
    }
    
    private Vector3 FindValidPosition()
    {
        const int maxAttempts = 100;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
            
            if (IsValidPosition(randomPos))
                return randomPos;
        }
        
        return Vector3.zero;
    }
    
    private bool IsValidPosition(Vector3 position)
    {
        if (agentTransform != null)
        {
            if (Vector3.Distance(position, agentTransform.position) < minDistanceFromAgent)
                return false;
        }
        
        if (goalTransform != null)
        {
            if (Vector3.Distance(position, goalTransform.position) < minDistanceFromGoal)
                return false;
        }
        
        foreach (GameObject asteroid in asteroids)
        {
            if (asteroid != null && Vector3.Distance(position, asteroid.transform.position) < minDistanceBetweenAsteroids)
                return false;
        }
        
        return true;
    }
    
    private GameObject CreateAsteroid(Vector3 position, int index)
    {
        GameObject asteroid;
        
        if (asteroidPrefab != null)
        {
            asteroid = Instantiate(asteroidPrefab, position, Random.rotation, transform);
        }
        else
        {
            asteroid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            asteroid.transform.position = position;
            asteroid.transform.rotation = Random.rotation;
            asteroid.transform.parent = transform;
        }
        
        asteroid.name = $"Asteroid_{index:D3}";
        asteroid.tag = "Asteroid";
        
        float scale = Random.Range(asteroidScaleRange.x, asteroidScaleRange.y);
        asteroid.transform.localScale = Vector3.one * scale;
        
        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb == null)
            rb = asteroid.AddComponent<Rigidbody>();
        
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Random.insideUnitSphere * maxDriftSpeed;
        rb.angularVelocity = Random.insideUnitSphere * 0.5f;
        
        asteroidRigidbodies.Add(rb);
        
        return asteroid;
    }
    
    private void ClearAsteroids()
    {
        foreach (GameObject asteroid in asteroids)
        {
            if (asteroid != null)
                Destroy(asteroid);
        }
        
        asteroids.Clear();
        asteroidRigidbodies.Clear();
    }
    
    public List<AsteroidData> GetAsteroidData()
    {
        List<AsteroidData> data = new List<AsteroidData>();
        
        foreach (Rigidbody rb in asteroidRigidbodies)
        {
            if (rb != null)
            {
                data.Add(new AsteroidData
                {
                    position = rb.position,
                    velocity = rb.linearVelocity,
                    radius = rb.transform.localScale.x / 2f
                });
            }
        }
        
        return data;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}

[System.Serializable]
public struct AsteroidData
{
    public Vector3 position;
    public Vector3 velocity;
    public float radius;
}
