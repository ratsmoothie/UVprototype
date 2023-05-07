using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class limitFps : MonoBehaviour
{
    //locking the frame rate to 60 to hopefully fix camera stuttering
    void Start()
    {
        Application.targetFrameRate = 60;
    }

}
