using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerseMenu : MonoBehaviour
{
    [SerializeField]
    Text verseText;

    [SerializeField]
    Text[] wordTexts;

    //[SerializeField]
    //Text timerText;

    public void SetVerse(string verse)
    {
        verseText.text = verse;
    }

    public void SetWord(string word, int index)
    {
        if (index >= 0 && index < wordTexts.Length)
            wordTexts[index].text = word;
    }

    //public void SetTimer(int total)
    //{
    //    StartCoroutine(TimerRoutine(total));
    //}

    //IEnumerator TimerRoutine(int total)
    //{
    //    timerText.text = $"{total}";

    //    while (total > -1)
    //    {
    //        yield return new WaitForSeconds(1f);
    //        total--;
    //        timerText.text = $"{total}";
    //    }

    //    timerText.text = "";
    //}
}
