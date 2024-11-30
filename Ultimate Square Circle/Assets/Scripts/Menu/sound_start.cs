using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_start : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AudioListener.volume = 100f;
    }
}
