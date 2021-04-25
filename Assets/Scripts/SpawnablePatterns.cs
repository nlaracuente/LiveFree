using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnablePatterns : MonoBehaviour
{
    [SerializeField]
    PixleColorToPrefab[] prefabs;

    [SerializeField]
    Texture2D[] patternTextures;

    Dictionary<Color, Spawnable> prefabMap;

    [SerializeField]
    List<Spawnable[,]> patterns;

    private void Awake()
    {
        prefabMap = new Dictionary<Color, Spawnable>();
        foreach (var prefab in prefabs)
        {
            if (!prefabMap.ContainsKey(prefab.pixleColor))
                prefabMap.Add(prefab.pixleColor, prefab.prefab); ;
        }

        patterns = new List<Spawnable[,]>();
        for (int i = 0; i < patternTextures.Length; i++)
        {
            var texture = patternTextures[i];
            var pattern = new Spawnable[texture.width, texture.height];

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    var pixle = texture.GetPixel(x, y);
                    if(prefabMap.ContainsKey(pixle))
                        pattern[x, y] = prefabMap[pixle];
                    else
                        pattern[x, y] = null;
                }
            }

            patterns.Add(pattern);
        }
    }

    public Spawnable[,] GetRandomPattern()
    {
        return Utility.ShuffleArray(patterns.ToArray()).First();
    }
}

[System.Serializable]
public struct PixleColorToPrefab
{
    public Color pixleColor;
    public Spawnable prefab;
}
