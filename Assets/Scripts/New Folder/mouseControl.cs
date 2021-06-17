using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseControl : MonoBehaviour
{
    [SerializeField]Camera camerafor;
    // Start is called before the first frame update
    void Start()
    {
        var mouseScript = new PlayBandMouse();
        mouseScript.mouseCamera = camerafor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
