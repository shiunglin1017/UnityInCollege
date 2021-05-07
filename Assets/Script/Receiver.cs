using UnityEngine;
using UnityEngine.UI;

public class Receiver : MonoBehaviour
{
	public Transform obj3d;
	public GameObject pitchObj;
	public GameObject yawObj;
	public GameObject rollObj;
    public Text Ax, Ay, Az, Rx, Ry, Rz;
    const float MAX_OFFSET = 3.0f;
    
	void Start()
    {
        /*
#if !UNITY_EDITOR
        PlayBand.Device1.OnIncomingDataEvent += ReceiveDataP1;
#endif
*/
        PlayBand.Device1.OnIncomingDataEvent += ReceiveDataP1;
    }

    void OnDestroy()
    {
        /*
#if !UNITY_EDITOR
		PlayBand.Device1.OnIncomingDataEvent -= ReceiveDataP1;
#endif
*/
        PlayBand.Device1.OnIncomingDataEvent -= ReceiveDataP1;
    }

    public void ReceiveDataP1(PlayBandData data)
    {
		MoveObj(obj3d,data);
        pitchObj.transform.rotation = Quaternion.Euler(new Vector3(data.EulerAngles.y,0,0));
		yawObj.transform.rotation = Quaternion.Euler(new Vector3(0,data.EulerAngles.x,0));
		rollObj.transform.rotation = Quaternion.Euler(new Vector3(0,0,data.EulerAngles.z));
        Ax.text = "Ax : " + data.Acceleration.x;
        Ay.text = "Ay : " + data.Acceleration.y;
        Az.text = "Az : " + data.Acceleration.z;
        Rx.text = "Rx : " + data.EulerAngles.x;
        Ry.text = "Ry : " + data.EulerAngles.y;
        Rz.text = "Rz : " + data.EulerAngles.z;
    }
    
    private void MoveObj(Transform objTrans,PlayBandData data)
	{
		if (objTrans != null)
		{
			Vector3 _V = Vector3.ClampMagnitude(data.Velocity * 15.0f, MAX_OFFSET);
			objTrans.localRotation = data.Rotation;
			objTrans.localPosition = Vector3.Lerp(objTrans.localPosition,_V,0.5f);
		}
	}
}
