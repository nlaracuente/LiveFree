using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : Singleton<MenuController>
{
    [SerializeField]
    GameObject gameOverMenu;

    private void Start()
    {
        gameOverMenu.SetActive(false);
    }

    public void Continue() => GameManager.Instance.Continue();
    public void ShowGameOverMenu() => gameOverMenu.SetActive(true);
}
