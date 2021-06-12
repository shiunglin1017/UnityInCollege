using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class playercontrolbyblue : BoyCtrl
{
    
    // Start is called before the first frame update
    void Start()
    {
        PlayBand.Connect();
        PlayBand.Device1.OnIncomingDataEvent += receiveData;
        PlayBand.Device1.On4WayTriggerEventV += PlayerWay;

    }

    public void receiveData(PlayBandData data)
    {
        Debug.Log(data.Acceleration);


    }

    public void PlayerWay(PlayBandDirection direction, PlayBandData data)
    {
        if(direction == PlayBandDirection.Up)
        {
            isJump = true;
        }
        else
        {
            isJump = false;
        }

        
    }

    private void OnDestroy()
    {
        PlayBand.Device1.OnIncomingDataEvent -= receiveData;
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    
    
}
