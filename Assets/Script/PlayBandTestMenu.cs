using UnityEngine;
using System.Collections;

public class PlayBandTestMenu : MonoBehaviour
{
	private enum PAGE
	{
		OPERATION = 0,
		BATTERY,
		LED,
		VIBRATE,
		DATETIME,
		SYSTEM,
		countDown,//add zhuzhao
		MAGNETIC,
		JIUGONGGE,
		POWER,
	}
	;
	
	private string m_ViParam1 = "100";
	private string m_ViParam2 = "100";
	private string m_ViParam3 = "100";
	private string m_LEDParam1 = "0";
	private string m_LEDParam2 = "100";
	private string m_LEDParam3 = "100";
	private string m_LEDParam4 = "100";
	//
	private PAGE m_CurPage;
	private int leftSpace = 20;
	private int topSpace = 20;
	private Rect buttonRect = new Rect ();
	private Rect textRect = new Rect ();
	private string res_Connect = "Disconnect";
	
	private int battery1 = 0;
	private bool calibrated1 = false;
	private int JiugonggePos = 5;
	//
	private bool menuOpen = true;
	PlayBandMouse playBandMouse;
	//===============================
	//
	//===============================
	void Start ()
	{
		InitBandListener();
	}
	//------------------------------------------------
	// Unity callback update()
	// Create: 2015.07.16 by Eric Fei
	// Modify: 2015.07.16 by Eric Fei
	//------------------------------------------------
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Escape))
		{
            Application.Quit();
		}
	}

    private void stopListener()
    {
        PlayBand.OnConnectResultEvent -= ConnectResult;
        PlayBand.Device1.OnBatteryStatusEvent -= BatteryStatusP1;
        PlayBand.OnCalibratedEvent -= CalibratedP1;
    }

    //------------------------------------------------
    //设定监听
    //------------------------------------------------	
    private void InitBandListener ()
	{
		PlayBand.OnConnectResultEvent += ConnectResult;
		PlayBand.Device1.OnBatteryStatusEvent += BatteryStatusP1;
		PlayBand.OnCalibratedEvent+= CalibratedP1;
	}
	//===============================
	//接收器
	//===============================
	public void ConnectResult (PlayBandConnectData connectData)
	{
		if (connectData.success) {
			menuOpen = false;
		} else {
			res_Connect = connectData.status.ToString ();
		}
	}

	public void BatteryStatusP1 (PlayBandBatteryData data)
	{
		battery1 = data.life;
	}
	
	public void CalibratedP1 (PlayBandID id)
	{
		calibrated1 = true;
	}

	//===============================
	//功能测试
	//===============================
	void OnGUI ()
	{
#if UNITY_EDITOR
		//return;
#endif
		GUI.skin.button.fontSize = 18;
		GUI.skin.textField.fontSize = 18;
		GUI.skin.label.fontSize = 18;
		
		textRect.x = leftSpace;
		textRect.y = topSpace;
		textRect.width = 80;
		textRect.height = 40;
		
		buttonRect.x = leftSpace;
		buttonRect.y = topSpace;
		buttonRect.width = 80;
		buttonRect.height = 80;

        if (GUI.Button (buttonRect, "Menu")) {
			menuOpen = !menuOpen;
			m_CurPage = PAGE.OPERATION;
		}
        
		if (!menuOpen) {
			return;
		}
        
        buttonRect.x += 100;
        if (!PlayBand.Device1.connected) {
			if (GUI.Button (buttonRect, "Connect")) {
				PlayBand.Connect ();
				m_CurPage = PAGE.OPERATION;
            }
		} else {
			if (GUI.Button (buttonRect, "Disconn")) {
				m_CurPage = PAGE.OPERATION;
				PlayBand.Disconnect();
				calibrated1=false;
			}
		}
		
		if (PlayBand.Device1.connected) {
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "Fixed\nNorth")) {
				//	m_CurPage = PAGE.MAGNETIC;
				PlayBand.FixedNorth();
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "Vibrate")) {
				m_CurPage = PAGE.VIBRATE;
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "LED")) {
				m_CurPage = PAGE.LED;
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "Battery")) {
				m_CurPage = PAGE.BATTERY;
			}

			buttonRect.x += 100;//add zhuzhao
			if (GUI.Button (buttonRect, "isCount\nDown")) {
				m_CurPage = PAGE.countDown;
			}

			buttonRect.x += 100;//add zhuzhao
			if (GUI.Button (buttonRect, "JIUGONGGE")) {
				m_CurPage = PAGE.JIUGONGGE;
			}

			buttonRect.x += 100;//add zhuzhao
			if (GUI.Button (buttonRect, "PowerMode")) {
				m_CurPage = PAGE.POWER;
			}

		}
		
		if (m_CurPage == PAGE.OPERATION) {
			textRect.x = leftSpace + 40;
			textRect.y = topSpace + 100;
			textRect.height = 40;
			textRect.width = 240;
			
			if(PlayBand.Device1.connected){
				GUI.TextField (textRect, PlayBand.Device1.address);
			}else{
				GUI.TextField (textRect, "Disconnect");
			}
			
			textRect.y += 60;
			GUI.TextField (textRect, "Calibrated:"+calibrated1);
			
			
			
		} else if (m_CurPage == PAGE.LED) {	
			
			textRect.x = leftSpace;
			textRect.y = topSpace + 100;
			
			GUI.Label (textRect, "Color");
			textRect.x += 100;
			GUI.Label (textRect, "Bright");
			textRect.x += 100;
			GUI.Label (textRect, "On Per");
			textRect.x += 100;
			GUI.Label (textRect, "Off Per");
			
			textRect.x = leftSpace;
			textRect.y += textRect.height;
			m_LEDParam1 = GUI.TextField (textRect, m_LEDParam1);
			textRect.x += 100;
			m_LEDParam2 = GUI.TextField (textRect, m_LEDParam2);
			textRect.x += 100;
			m_LEDParam3 = GUI.TextField (textRect, m_LEDParam3);
			textRect.x += 100;
			m_LEDParam4 = GUI.TextField (textRect, m_LEDParam4);
			
			buttonRect.x = leftSpace;
			buttonRect.y = topSpace + 200;
			if (GUI.Button (buttonRect, "LED\nOff")) {
				int _Ret = PlayBand.LEDOff ((PlayBandLEDColor)(int.Parse (m_LEDParam1)));
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "LED\nOn")) {
				int _Ret = PlayBand.LEDOn ((PlayBandLEDColor)(int.Parse (m_LEDParam1)), int.Parse (m_LEDParam2));
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "LED\nFlash")) {
				int _Ret = PlayBand.LEDFlash ((PlayBandLEDColor)(int.Parse (m_LEDParam1)), int.Parse (m_LEDParam2), int.Parse (m_LEDParam3), int.Parse (m_LEDParam4));
			}
			
		} else if (m_CurPage == PAGE.VIBRATE) {	
			textRect.x = leftSpace;
			textRect.y = topSpace + 100;
			
			GUI.Label (textRect, "Power");
			textRect.x += 100;
			GUI.Label (textRect, "On Per");
			textRect.x += 100;
			GUI.Label (textRect, "Off Per");
			
			textRect.x = leftSpace;
			textRect.y += textRect.height;
			
			m_ViParam1 = GUI.TextField (textRect, m_ViParam1);
			textRect.x += 100;
			m_ViParam2 = GUI.TextField (textRect, m_ViParam2);
			textRect.x += 100;
			m_ViParam3 = GUI.TextField (textRect, m_ViParam3);
			
			buttonRect.x = leftSpace;
			buttonRect.y = topSpace + 200;
			if (GUI.Button (buttonRect, "Vibrate\nOnce")) {
				int _Ret = PlayBand.VibrateOnce (int.Parse (m_ViParam1), int.Parse (m_ViParam2));
			}
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "Vibrate\nTwice")) {
				int _Ret = PlayBand.VibrateTwice (int.Parse (m_ViParam1), int.Parse (m_ViParam2), int.Parse (m_ViParam3));
			}
			
			
			
		} else if (m_CurPage == PAGE.BATTERY) {	
			textRect.x = leftSpace;
			textRect.y = topSpace + 100;
			
			GUI.Label (textRect, "Life");
			textRect.x += 100;
			
			
			textRect.x = leftSpace;
			textRect.y += textRect.height;
			GUI.TextField (textRect, battery1.ToString ());
			
			buttonRect.x = leftSpace;
			buttonRect.y = topSpace + 200;
			
			if (GUI.Button (buttonRect, "Battery")) {
				int _Ret = PlayBand.InquireBatteryStatus ();
			}
		} else if (m_CurPage == PAGE.POWER) {	
			
			textRect.x = leftSpace;
			textRect.y = topSpace + 100;
			
			buttonRect.x = leftSpace;
			buttonRect.y = topSpace + 200;
			if (GUI.Button (buttonRect, "StandBy")) {
				int _Ret = PlayBand.SetPowerMode(PlayBandPowerMode.Standby);
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "SaveMode")) {
				int _Ret = PlayBand.SetPowerMode(PlayBandPowerMode.SavePower);
			}
			
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "Normal")) {
				int _Ret = PlayBand.SetPowerMode(PlayBandPowerMode.Normal);
			}
			buttonRect.x += 100;
			if (GUI.Button (buttonRect, "FullMode")) {
				int _Ret = PlayBand.SetPowerMode(PlayBandPowerMode.Performance);
			}

		} 

	}
}