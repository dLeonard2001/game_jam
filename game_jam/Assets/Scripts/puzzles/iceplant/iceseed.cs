using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class iceseed : MonoBehaviour
{

    public GameObject bridge;

    public GameObject mound;
    public Vector3 mound_pos;
    
    // Start is called before the first frame update
    void Start()
    {
        bridge.SetActive(false);
        
        mound_pos = mound.transform.position;
    }

    public void PuzzleSolved()
    {
        bridge.SetActive(true);
    }
}
