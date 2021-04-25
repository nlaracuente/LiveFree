using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : Singleton<MenuController>
{
    [SerializeField]
    VerseMenu verseMenu;

    [SerializeField]
    GameObject gameOverMenu;

    [SerializeField]
    GameObject gameWonMenu;

    private void Start()
    {
        if (verseMenu == null)
            verseMenu = FindObjectOfType<VerseMenu>();
        CloseMenus();
    }

    public void CloseMenus()
    {
        verseMenu.gameObject.SetActive(false);
        gameOverMenu.SetActive(false);
        gameWonMenu.SetActive(false);
    }

    public void Continue() => GameManager.Instance.Continue();
    public void ShowGameOverMenu() => gameOverMenu.SetActive(true);
    public void ShowGameWonMenu() => gameWonMenu.SetActive(true);
    public void ShowVerseMenu(string verse, string[] words, int totalTime = 0)
    {
        verseMenu.SetVerse(verse);

        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            verseMenu.SetWord(word, i);
        }

        verseMenu.gameObject.SetActive(true);
        //verseMenu.SetTimer(totalTime);

    }
    public void HideVerseMenu() => verseMenu.gameObject.SetActive(false);
    public void ChosenWord(int index) => GameManager.Instance.ChosenWord(index);
}
