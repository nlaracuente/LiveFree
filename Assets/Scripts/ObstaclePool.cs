using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : Singleton<ObstaclePool>
{
    [SerializeField]
    Obstacle[] obstacles;

    [SerializeField]
    int poolSizePerType = 10;

    Dictionary<ObstacleType, Obstacle[]> pool;

    private void Awake()
    {
        CreatePool();        
    }

    void CreatePool()
    {
        pool = new Dictionary<ObstacleType, Obstacle[]>();

        foreach (var prefab in obstacles)
        {
            var type = prefab.Type;

            // Ignore duplicate obstacle types
            if (pool.ContainsKey(type))
                continue;

            pool.Add(type, new Obstacle[poolSizePerType]);
            for (int i = 0; i < poolSizePerType; i++)
            {
                // Spawn the object where the pool is at 
                // which should be outside of the game view
                var obstacle = Instantiate(prefab.GameObject, transform).GetComponent<Obstacle>();
                obstacle.transform.position = new Vector3(
                    transform.position.x,
                    obstacle.transform.position.y,
                    transform.position.z
                );
                pool[type][i] = obstacle;
            }
        }
    }

    public Obstacle GetNextAvailable(ObstacleType type)
    {
        Obstacle available = null;

        foreach (var obstacle in pool[type])
        {
            if (!obstacle.IsAvailable)
                continue;

            available = obstacle;
            break;
        }

        return available;
    }
}
