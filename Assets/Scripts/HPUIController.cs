﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPUIController : IconController
{
    protected override int MaxIcons
    {
        get
        {
            return GameManager.Instance.Player.MaxHP;
        }
    }

    protected override int CurrentTotal
    {
        get
        {
            return GameManager.Instance.Player.HP;
        }
    }
}
