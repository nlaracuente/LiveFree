using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IconController : MonoBehaviour
{
    [SerializeField]
    GameObject iconPrefab;

    List<GameObject> icons;

    protected abstract int MaxIcons { get; }
    protected abstract int CurrentTotal { get; }

    private void Start()
    {
        icons = new List<GameObject>();
        for (int i = 0; i < MaxIcons; i++)
        {
            var icon = Instantiate(iconPrefab, transform);
            icons.Add(icon);
        }
    }

    private void LateUpdate()
    {
        // Remember we start at zero so HP is always 1 greater than our index
        for (int i = 0; i < icons.Count; i++)
            icons[i].SetActive(i <= CurrentTotal - 1);
    }
}
