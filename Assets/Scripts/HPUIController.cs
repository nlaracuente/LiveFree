using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPUIController : MonoBehaviour
{
    [SerializeField]
    GameObject hpIconPrefab;

    [SerializeField]
    Player player;

    List<GameObject> icons;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        icons = new List<GameObject>();
        for (int i = 0; i < player.HP; i++)
        {
            var icon = Instantiate(hpIconPrefab, transform);
            icons.Add(icon);
        }
    }

    private void LateUpdate()
    {
        // Remember we start at zero so HP is always 1 greater than our index
        for (int i = 0; i < icons.Count; i++)
            icons[i].SetActive(i <= player.HP - 1);
    }
}
