using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnablePool : Singleton<SpawnablePool>
{
    [SerializeField]
    Spawnable[] spawnables;

    [SerializeField]
    int poolSizePerType = 10;

    Dictionary<SpawnableType, Spawnable[]> pool;

    private void Start()
    {
        CreatePool();        
    }

    void CreatePool()
    {
        pool = new Dictionary<SpawnableType, Spawnable[]>();

        foreach (var prefab in spawnables)
        {
            var type = prefab.Type;

            // Ignore duplicate obstacle types
            if (pool.ContainsKey(type))
                continue;

            pool.Add(type, new Spawnable[poolSizePerType]);
            for (int i = 0; i < poolSizePerType; i++)
            {
                // Spawn the object where the pool is at 
                // which should be outside of the game view
                var spawnable = Instantiate(prefab.gameObject, transform).GetComponent<Spawnable>();
                spawnable.transform.position = new Vector3(
                    transform.position.x,
                    spawnable.transform.position.y,
                    transform.position.z
                );
                pool[type][i] = spawnable;
            }
        }
    }

    public Spawnable GetNextAvailable(SpawnableType type)
    {
        Spawnable available = null;

        if (!pool.ContainsKey(type))
            return available;

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
