using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshHPUIController : IconController
{
    protected override int MaxIcons
    {
        get
        {
            return GameManager.Instance.Flesh.MaxHP;
        }
    }

    protected override int CurrentTotal
    {
        get
        {
            return GameManager.Instance.Flesh.HP;
        }
    }
}
