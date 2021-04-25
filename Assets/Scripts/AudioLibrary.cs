using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLibrary : Singleton<AudioLibrary>
{
    public AudioClip playerMoveClip;
    public AudioClip playerCrashedClip;
    public AudioClip scripturePickedUpClip;
    public AudioClip playerPunchCryClip;

    public AudioClip punchHitClip;
    public AudioClip[] punchMissedClips;
}
