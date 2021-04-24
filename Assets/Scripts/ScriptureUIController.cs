using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptureUIController : IconController
{
    protected override int MaxIcons
    {
        get
        {
            return GameManager.Instance.TotalScriptures;
        }
    }

    protected override int CurrentTotal
    {
        get
        {
            return GameManager.Instance.TotalScripturesCollected;
        }
    }
}
