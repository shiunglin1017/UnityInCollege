using UnityEngine;
using UnityEngine.UI;

public class PlayBandMouse : MonoBehaviour
{
	public Camera mouseCamera;              //拍攝到滑鼠的相機
	public Vector2 mouseAngleMax = new Vector2 (30f, 30f);          //滑鼠上下左右最大移動角度        
    private Transform mouseCursor;          //滑鼠繪製對象
	public Vector3 _vec3CurrentPosition;   //滑鼠當前螢幕位置
    public Text mouseX, mouseY, mouseZ;
	private int mouseUser = 0;

	[HideInInspector]
	public bool enableMouse = false;
	private Vector3 anglePosition = Vector3.zero;
	private Vector3 anglePositionCache = Vector3.zero;
	private PlayBandData data;

	//無限範圍內的屏幕位置
	private Vector2 _vec2UnlimitedPosition;
    
	public void StartMouseListener ()
	{
		mouseCursor.gameObject.SetActive (true);
		enableMouse=true;
		PlayBand.OnIncomingDataEvent += UpdateMouseCursor;
		PlayBand.Device1.OnButtonClickedEvent -= onButtonClickCallBack;
        PlayBand.Device1.OnButtonClickedEvent += onButtonClickCallBack;
    }
	
	//
	public void StopMouseListener ()
	{
		mouseCursor.gameObject.SetActive (false);
		enableMouse = false;
		PlayBand.OnIncomingDataEvent -= UpdateMouseCursor;
		PlayBand.Device1.OnButtonClickedEvent -= onButtonClickCallBack;
	}
	
	//private void UpdateMouseCursor (PlayBandData data)	
	private void UpdateMouseCursor (PlayBandData[] dataList)
	{
		if (!enableMouse) {
			return;
		}		
		data = dataList[mouseUser];
		float factor = 2.0f;
		Vector3 dir = data.Rotation*Vector3.forward;
		dir *= factor;
		dir.x = (dir.x + 1.0f)*0.5f;
		dir.y = (dir.y + 1.0f)*0.5f;
		
		var vec3UnlimitedAngle = anglePosition;
		
		anglePosition.x = Mathf.Clamp(dir.x, 0.0f, 1.0f);
		anglePosition.y = Mathf.Clamp(dir.y, 0.0f, 1.0f);
		
		anglePosition.z = mouseCamera.nearClipPlane;
		anglePosition = Vector3.Lerp (anglePosition, anglePositionCache, 0.5f);
		
		float oldZ = mouseCursor.position.z;
		var vec3NewPosition = mouseCamera.ViewportToWorldPoint(anglePosition);
		mouseCursor.position = new Vector3(vec3NewPosition.x, vec3NewPosition.y, oldZ);
        
        //unlimited screen position
        oldZ = mouseCursor.position.z;
		var vec3NewUnlimitedPosition = mouseCamera.ViewportToScreenPoint(vec3UnlimitedAngle);
		_vec2UnlimitedPosition = new Vector3(vec3NewUnlimitedPosition.x, vec3NewUnlimitedPosition.y, oldZ);

        anglePositionCache = anglePosition;
		
		updateRaycast(mouseCamera.WorldToScreenPoint(mouseCursor.position));
	}	
	
    void Start () 
	{
		mouseCursor = transform.Find("Cursor");
        /*
#if !UNITY_EDITOR
		PlayBand.StartMouseListener();
#endif
*/
        PlayBand.StartMouseListener();
    }

	void OnDestroy()
	{
        /*
#if !UNITY_EDITOR
		PlayBand.StopMouseListener();
#endif
*/
        PlayBand.StopMouseListener();
    }
	
	void onButtonClickCallBack()
	{
		PlayBand.VibrateOnce(50, 150);
    }
    
    void updateRaycast (Vector3 viewportPoint) 
	{
		_vec3CurrentPosition = viewportPoint;
        mouseX.text = "mouseX:" + _vec3CurrentPosition.x;
        mouseY.text = "mouseY:" + _vec3CurrentPosition.y;
        mouseZ.text = "mouseZ:" + _vec3CurrentPosition.z;
    }

	void Update()
	{
        /*
#if UNITY_EDITOR
		float oldZ = mouseCursor.position.z;
		mouseCursor.position = mouseCamera.ScreenToWorldPoint (Input.mousePosition);
		mouseCursor.position = new Vector3(mouseCursor.position.x, mouseCursor.position.y, oldZ);
		_vec2UnlimitedPosition = Input.mousePosition;
		updateRaycast(Input.mousePosition);
#endif
*/
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopMouseListener();
            PlayBand.Device1.Disconnect();
            Application.Quit();
        }
	}
}