//=====================================
//CavtTech 20150525
//=====================================
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

#if UNITY_IPHONE && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

#if UNITY_WSA || NETFX_CORE || UNITY_METRO
using CavyBandSDKWindows;
#endif

public class PlayBand : AbstractPlayBand
{
	public const long Version = 0x020100;

	//-----------------------------------------
	//All the playband control parameters begin
	//if you want to set parameters, just as [ _triggerShakePower -> PlayBand::GetInstance()->setTriggerShakePower(0.03) ];
	//-----------------------------------------
	//shake => OnShakeTriggerEventV
	float _triggerShakePower =             0.02f;       //Waving a hand back and forth power
	float _triggerShakeLockTime =          0.6f;        //Triggering event time interval
	float _triggerShakeIntervalTime =      0.5f;        //Swinging back and forth time interval

	//finger => OnFingerTriggerEvent
	PlayBandTriggerMode _triggerFingerMode = PlayBandTriggerMode.tNormal;
	
	//way => On4WayTriggerEventV  On8WayTriggerEventV OnZWayTriggerEventV ...
	float _triggerWayPowerV =              0.03f;       //Waving a hand once power
	float _triggerWayLockTime =            0.6f;        //Triggering event time interval
	
	//peak => On4WayPeakEventV On8WayPeakEventV OnZWayPeakEventV ...
	float _triggerMinPeakZoneV =           0.05f;       //Wave a hand peak min power
	float _triggerMaxPeakZoneV =           0.07f;       //Wave a hand peak max power
	float _triggerPeakZoneLockTime =       0.7f;        //Triggering event time interval
	
	//getsutre
	float _triggerGestureAccurateRate =    0.0f;         //Gesture events triggered the lowest ratio
	float _triggerGestureInputLockTime =   0.3f;        //Gesture Input time interval
	float _triggerGestureCompareLockTime = 0.3f;        //Triggering event time interval
	float _triggerGesturePower =           0.03f;       //Compare gestures need power
	
	//rotate angle => OnRotateTriggerEvent
	float _triggerRotateAngle =            30.0f;       //how many angle rotation to trigger
	float _triggerRotateIntervalTime =     0.01f;       //rotate time interval
	float _triggerRotateLockTime =         0.6f;        //Triggering event time interval
	//-----------------------------------------
	//All the playband control parameters end
	//-----------------------------------------

	public static string ConnectAddress = "";
	public static PlayBandConnectStatus  ConnectStatus = PlayBandConnectStatus.Disconnect;
	public static bool Calibrated = false;
	public static bool IsCalibrating = false;
	//
	public static PlayBandMobileSensor MobileSensor = new PlayBandMobileSensor ();
	//--------------------------------------------------------------------
	public static Action<PlayBandConnectData> OnConnectResultEvent;
	public static Action<PlayBandData[]> OnIncomingDataEvent;
	public static Action<PlayBandID> OnButtonClickedEvent;
	public static Action<PlayBandBatteryData> OnBatteryStatusEvent;
	public static Action<PlayBandWarningData> OnWarningDataEvent;
	public static Action<PlayBandID> OnCalibratedEvent;
	//--------------------------------------------------------------------
	[HideInInspector]
	public PlayBandPowerMode powerMode = PlayBandPowerMode.Performance;
	public PlayBandSensorRate sensorDataRate = PlayBandSensorRate.Normal_25HZ;
	public PlayBandSensorRate updateDataRate = PlayBandSensorRate.Normal_25HZ;
	//
	public static PlayBandPowerMode PowerMode {
		get { return instance.powerMode; }
		set { instance.powerMode = value;}
	}
	//
	public static PlayBandSensorRate SensorDataRate {
		get { return instance.sensorDataRate; }
		set { instance.sensorDataRate = value;}
	}
	//
	public static PlayBandSensorRate UpdateDataRate {
		get { return instance.updateDataRate; }
		set { instance.updateDataRate = value;}
	}
	//
	private float updateDataTime = 0.03f;
	private float accumulateTime = 0f;
	private bool doUpdateData = false;
	private float deltaTime = 0f;
	
	//--------------------------------------------------------------------
	private static PlayBandData[] bandDataList = new PlayBandData[4] {
		new PlayBandData (),
		new PlayBandData (),
		new PlayBandData (),
		new PlayBandData ()
	};
	//--------------------------------------------------------------------
	private  PlayBandDevice[] deviceList= new PlayBandDevice[4];
	private  int[] sortMap=new int[]{0,1,2,3};
	
	public static PlayBandDevice[]  DeviceList {
		get { return instance.deviceList; }
	}
	
	public static PlayBandDevice Device1 {
		get { return instance.deviceList [instance.sortMap[0]]; }
	}
	
	public static PlayBandDevice Device2 {
		get { return instance.deviceList [instance.sortMap[1]]; }
	}
	
	public static PlayBandDevice Device3 {
		get { return instance.deviceList [instance.sortMap[2]]; }
	}
	
	public static PlayBandDevice Device4 {
		get { return instance.deviceList [instance.sortMap[3]]; }
	}
	//--------------------------------------------------------------------
	public  bool useButtonCorrection = false;
	
	public static bool UseButtonCorrection {
		get { return instance.useButtonCorrection; }
		set { instance.useButtonCorrection = value;}
	}
	//--------------------------------------------------------------------
	private static PlayBand instance;
	
	public static PlayBand GetInstance ()
	{
		return instance;
	}
	//--------------------------------------------------------------------
	void Awake ()
	{
		//bool duplicate = GameObject.Find ("PlayBand") != null;
		bool duplicate =instance!=null;
		
		if (duplicate) {
			Destroy (gameObject);
		} else {
			Initinal ();
		}
	}
	//
	public void Initinal ()
	{
		instance = this;
		gameObject.name = "PlayBand";
		DontDestroyOnLoad (this);
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		SetUpdateDataRate ((int)sensorDataRate);
		
		deviceList[0] = createDeviceObj (0);
		deviceList[1]  = createDeviceObj (1);
		deviceList[2]  = createDeviceObj (2);
		deviceList[3]  = createDeviceObj (3);
		
		Debug.Log ("deviceList[0]: address(" + deviceList[0].address + "), No(" + deviceList[0].deviceNo + ")");
		Debug.Log ("deviceList[1]: address(" + deviceList[1].address + "), No(" + deviceList[1].deviceNo + ")");
		Debug.Log ("deviceList[2]: address(" + deviceList[2].address + "), No(" + deviceList[2].deviceNo + ")");
		Debug.Log ("deviceList[3]: address(" + deviceList[3].address + "), No(" + deviceList[3].deviceNo + ")");

#if UNITY_STANDALONE_WIN
        OnUnitySetCallback(new OnJniResult(this.onJniResult));
#endif
    }
    //
    private PlayBandDevice createDeviceObj (int deviceNo)
	{
		PlayBandDevice deviceObj = new PlayBandDevice();

		deviceObj.deviceNo = deviceNo;
		deviceObj.mExactAngleMode = _exactAngleMode;
		deviceObj.setTriggerFingerMode(_triggerFingerMode);
		deviceObj.mTriggerWayPowerV = _triggerWayPowerV;
		deviceObj.mTriggerWayLockTime = _triggerWayLockTime;
		deviceObj.mTriggerShakePower = _triggerShakePower;
		deviceObj.mTriggerShakeLockTime = _triggerShakeLockTime;
		deviceObj.mTriggerShakeIntervalTime = _triggerShakeIntervalTime;
		deviceObj.mTriggerMinPeakZoneV = _triggerMinPeakZoneV;
		deviceObj.mTriggerMaxPeakZoneV = _triggerMaxPeakZoneV;
		deviceObj.mTriggerPeakZoneLockTime = _triggerPeakZoneLockTime;
		deviceObj.mTriggerGestureAccurateRate = _triggerGestureAccurateRate;
		deviceObj.mTriggerGestureInputLockTime = _triggerGestureInputLockTime;
		deviceObj.mTriggerGestureCompareLockTime = _triggerGestureCompareLockTime;
		deviceObj.mTriggerGesturePower = _triggerGesturePower;
		deviceObj.mTriggerRotateAngle = _triggerRotateAngle;
		deviceObj.mTriggerRotateLockTime = _triggerRotateLockTime;
		deviceObj.mTriggerRotateIntervalTime = _triggerRotateIntervalTime;
		
		deviceObj.mOrderNo = deviceNo;
		
		deviceObj.init();
		return deviceObj;
	}
	//
	public static void SwapDevice (int fromID,int toID){
		instance.SwapDeviceObj(fromID,toID);
	}
	
	private void SwapDeviceObj (int fromID,int toID)
	{
		int tempID=sortMap[fromID];
		sortMap[fromID]=sortMap[toID];
		sortMap[toID]=tempID;
		
		SwapPlayBandDevice(ref deviceList[fromID],ref deviceList[toID]);
		deviceList[fromID].mOrderNo = sortMap[fromID];
		deviceList[toID].mOrderNo = sortMap[toID];
	}
	
	private void SwapPlayBandDevice ( ref PlayBandDevice fromObj, ref PlayBandDevice toObj)
	{
		PlayBandDevice temp=fromObj;
		fromObj=toObj;
		toObj=temp;
		
		PlayBandDevice temp2=new PlayBandDevice();
		temp2.SetProperty(fromObj);
		fromObj.SetProperty(toObj);
		toObj.SetProperty(temp2);
	}
	//
	public void SetUpdateDataRate (int dataRate)
	{
		updateDataTime = (float)(1000 / dataRate) * 0.001f;
	}
	
	public static void SetDataRate (int dataRate)
	{
		instance.SetUpdateDataRate (dataRate);
	}
	//--------------------------------------------------------------------
	void Update ()
	{
		if (doUpdateData) {
			bool updateSensorData = false;
			
			deltaTime = Time.deltaTime;
			accumulateTime += deltaTime;
			if (accumulateTime >= updateDataTime) {
				accumulateTime -= updateDataTime;
				updateSensorData = true;
				GetSensorData ();
				
				if (OnIncomingDataEvent != null) {
					OnIncomingDataEvent (bandDataList);
				}
			}
			
			deviceList [0].Update (deltaTime, updateSensorData, bandDataList [0]);
			deviceList [1].Update (deltaTime, updateSensorData, bandDataList [1]);
			deviceList [2].Update (deltaTime, updateSensorData, bandDataList [2]);
			deviceList [3].Update (deltaTime, updateSensorData, bandDataList [3]);
			
#if UNITY_STANDALONE_WIN
			CallbackOnMainThread();
#endif
			//UpdateMouseCursor (bandDataList [mouseUser]);
			//
		}
	}
	//
	public void StartReciveData ()
	{
		instance.accumulateTime = 0;
		instance.doUpdateData = true;
	}
	
	public void StopReciveData ()
	{
		instance.doUpdateData = false;
		instance.accumulateTime = 0;
		
	}
#if UNITY_ANDROID
	//=====================================
	//Receiver
	//=====================================
	private string[] separators={"@"};
	private string[] stringList=new string[3];
	private string deviceMac="";
	private int deviceNo=-1;
	
	public void OnJniConnectResult (string result)
	{
		stringList = result.Split(separators,0);
		Debug.Log("OnJniConnectResult() stringList length: " + stringList.Length);
		
		deviceNo = int.Parse(stringList[0]);
		deviceMac = stringList[1];
		result = stringList[2];
		
		PlayBandConnectData resultData = new PlayBandConnectData (result,deviceMac,deviceNo);
		ConnectAddress = resultData.address;
		
		Debug.Log("deviceNo: " + deviceNo + ",  [errorMessage] status: " + resultData.status + ",  msg: " + resultData.errorMessage);
		
		//edit by BlackSmith Jack, 20150717
		if( OnConnectResultEvent != null )
		{
			OnConnectResultEvent (resultData);
		}
		
		if(deviceNo==0){
			if (resultData.success) {
				Invoke ("FixedNorth", 1.0f);
				StartReciveData ();
			} else {
				Debug.Log("error : " + resultData.errorMessage);
				Calibrated = false;
				StopReciveData ();
			}
		}
		Debug.Log("deviceList[deviceNo]: " + deviceList[deviceNo]);
		deviceList[deviceNo].CallOnConnectResultEvent(resultData);
		
	}
	
	public void OnJniButtonClicked (string result)
	{
		stringList=result.Split(separators,0);
		deviceNo=int.Parse(stringList[0]);
		deviceMac=stringList[1];
		result=stringList[2];
		//		UpdateMouseClick (1);
		
		if (OnButtonClickedEvent != null) {
			OnButtonClickedEvent (new PlayBandID (deviceMac,deviceNo));
		}
		Debug.Log("TTT_OnJniButtonClicked In:"+deviceMac);
		Debug.Log("TTT_OnJniButtonClicked To:"+deviceNo+"  "+deviceList[deviceNo].address);
		
		deviceList[deviceNo].CallOnButtonClickedEvent();
		
		if(useButtonCorrection){
			FixedNorth(deviceNo);
		}
	}
	//
	public void OnJniBatteryStatus (string result)
	{
		stringList=result.Split(separators,0);
		deviceNo=int.Parse(stringList[0]);
		deviceMac=stringList[1];
		result=stringList[2];
		
		PlayBandBatteryData batteryData=new PlayBandBatteryData (result,deviceMac,deviceNo);
		
		if (OnBatteryStatusEvent != null) {
			OnBatteryStatusEvent (batteryData);
		}
		
		deviceList[deviceNo].CallOnBatteryStatusEvent(batteryData);
		
	}
	
	public void OnJniWarningStatus (string result)
	{
		stringList=result.Split(separators,0);
		deviceNo=int.Parse(stringList[0]);
		deviceMac=stringList[1];
		result=stringList[2];
		
		PlayBandWarningData warningData=new PlayBandWarningData (result,deviceMac,deviceNo);
		
		if (OnWarningDataEvent != null) {
			OnWarningDataEvent (warningData);
		}
		
		deviceList[deviceNo].CallOnWarningDataEvent(warningData);
	}
	
	public void OnJniMagnetCalibrated (string result)
	{
		//stringList = result.Split(separators,0);
		//deviceNo = int.Parse(stringList[0]);
		//deviceMac = stringList[1];
		//result = stringList[2];
		
		//Calibrated = (result == "1");
		
		//PlayBandID playBandID=new PlayBandID (deviceMac,deviceNo);
		
		//if (OnCalibratedEvent != null) {
		//	OnCalibratedEvent (playBandID);
		//}
		
		//deviceList[deviceNo].CallOnCalibratedEvent(playBandID);
		
		//Debug.Log("OnJniMagnetCalibrated:"+Calibrated);
		
		Debug.Log("OnJniMagnetCalibrated result:"+result);
		string[] separators={","};
		string[] dataList=result.Split(separators,0);
		Calibrated = (result == "1");
		if( dataList[0] == "1" ) // done calibrate
		{
			PlayBand.IsCalibrating = false;
		}
		else if(dataList[0] == "0") // is calibrating
		{
			PlayBand.IsCalibrating = true;
		}
		else
		{
			PlayBand.IsCalibrating = false;
		}
		PlayBandID playBandID=new PlayBandID (deviceMac,deviceNo);
		
		if (OnCalibratedEvent != null) {
			OnCalibratedEvent (playBandID);
		}
		
		Debug.Log("OnJniMagnetCalibrated:"+PlayBand.IsCalibrating);
	}
	//
	public void OnJniSensorStatus (string result)
	{
		stringList=result.Split(separators,0);
		deviceNo=int.Parse(stringList[0]);
		deviceMac=stringList[1];
		result=stringList[2];
		
		MobileSensor.Update (result);
	}
	
	//=====================================
	//Sender
	//=====================================
	private static AndroidJavaObject _CurrentActivity;
	
	public static AndroidJavaObject GetActivity()
	{
		if(_CurrentActivity==null){
			AndroidJavaClass _UnityPlayer=new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			_CurrentActivity = _UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		}
		return _CurrentActivity;
	}
	//--------------------------------------------------------------------
	private static float[] cacheData;
	
	public override void GetSensorData()
	{
		cacheData=GetActivity().Call<float[]>("OnUnityGetSensor");
		bandDataList[0].Update(cacheData);
		if(DeviceList[1].connected){
			bandDataList[1].Update(cacheData,1);
		}
		if(DeviceList[2].connected){
			bandDataList[2].Update(cacheData,2); 
		}
		if(DeviceList[3].connected){
			bandDataList[3].Update(cacheData,3);
		}
	}
	
	public override int InquireBatteryStatus(int deviceNo)
	{
		object[] _Param = new object[1];
		_Param[0] = DeviceList[deviceNo].address;
		
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityInquireBatteryStatus", _Param);
		return _Ret;
	}
	
	public override int LEDOff(PlayBandLEDColor ledColor,int deviceNo)
	{
		object[] _Param = new object[6];
		_Param[0] = (int)ledColor;
		_Param[1] = 0;
		_Param[2] = 1;
		_Param[3] = 0;
		_Param[4] = 0;
		_Param[5] = DeviceList[deviceNo].address;
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityControlLED", _Param);
		_Param = null;
		return _Ret;
	}
	
	public override int LEDOff(int ledColor,int deviceNo)
	{
		return LEDOff((PlayBandLEDColor)ledColor,deviceNo);
	}
	
	public override int LEDOn(PlayBandLEDColor ledColor, int brightness,int deviceNo)
	{
		object[] _Param = new object[6];
		_Param[0] = (int)ledColor;
		_Param[1] = 1;
		_Param[2] = brightness;
		_Param[3] = 0;
		_Param[4] = 0;
		_Param[5] = DeviceList[deviceNo].address;
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityControlLED", _Param);
		_Param = null;
		return _Ret;
	}
	
	public override int LEDOn(int ledColor, int brightness,int deviceNo)
	{
		return LEDOn((PlayBandLEDColor)ledColor,brightness,deviceNo);
	}
	
	
	public override int LEDFlash(PlayBandLEDColor ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo)
	{
		object[] _Param = new object[6];
		_Param[0] = (int)ledColor;
		_Param[1] = 2;
		_Param[2] = brightness;
		_Param[3] = onPeriod;
		_Param[4] = offPeriod;
		_Param[5] = DeviceList[deviceNo].address;
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityControlLED", _Param);
		_Param = null;
		return _Ret;
	}
	
	public override int LEDFlash(int ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo)
	{
		return LEDFlash((PlayBandLEDColor)ledColor,brightness,onPeriod,offPeriod,deviceNo);
	}
	
	public override int VibrateOnce(int power, int period,int deviceNo)
	{
		object[] _Param = new object[5];
		_Param[0] = 1;
		_Param[1] = power;
		_Param[2] = period;
		_Param[3] = 0;
		_Param[4] = DeviceList[deviceNo].address;
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityDoVibrate", _Param);
		_Param = null;
		return _Ret;
	}
	
	public override int VibrateTwice(int power, int onPeriod, int offPeriod,int deviceNo)
	{
		object[] _Param = new object[5];
		_Param[0] = 2;
		_Param[1] = power;
		_Param[2] = onPeriod;
		_Param[3] = offPeriod;
		_Param[4] = DeviceList[deviceNo].address;
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityDoVibrate", _Param);
		_Param = null;
		return _Ret;
	}
	
	//--------------------------------------------------------------------
	public override int Connect(PlayBandPowerMode mode,string deviceName,PlayBandSensorRate sensorRate)
	{
		PowerMode=mode;
		SensorDataRate=sensorRate;
		object[] _Param = new object[3];
		_Param[0] = deviceName;
		//_Param[1] = (int) mode;
		_Param[1] = (int) 3;
		_Param[2] = (int) 25;
		int _Ret = 0;
		Debug.Log("OnUnityConnect(" + deviceName + ", " + mode + ", " + sensorRate + ")");

		_Ret = GetActivity().Call<int>("OnUnityConnect", _Param);
		_Param = null;
		if(_Ret==0){
			Calibrated=false;
		}
		return _Ret;
	}
	
	//
	public override int SetPowerMode(PlayBandPowerMode mode,PlayBandSensorRate sensorRate)
	{
		object[] _Param = new object[2];
		_Param[0] =(int)mode;
		_Param[1] =(int)sensorRate;
		int _Ret = 0;
		_Ret = GetActivity().Call<int>("OnUnityChangeMode", _Param);
		Debug.Log("Mode = "+ mode + " Sensor = " + sensorRate);
		_Param = null;
		return _Ret;
	}
	
	public override int Disconnect(bool result)
	{
		AndroidJavaObject  _CurActivity=GetActivity();
		_CurActivity.Call("OnUnityDisconnect");
		instance.StopReciveData();
		instance.deviceList[0].Disconnect();
		instance.deviceList[1].Disconnect();
		instance.deviceList[2].Disconnect();
		instance.deviceList[3].Disconnect();
		return 0;
	}
	
	public static int OnUnitySaveMagneticCalibrationData()
	{
		AndroidJavaObject  _CurActivity=GetActivity();
		_CurActivity.Call("OnUnitySaveMagneticCalibrationData");
		return 0;
	}
	
	public override int SetMagenticMode(PlayBandMagenticMode mode,int deviceNo)
	{
		AndroidJavaObject  _CurActivity=GetActivity();
		object[] _Param = new object[2];
		_Param[0] = (int) mode;
		_Param[1] = DeviceList[deviceNo].address;
		int _Ret=0;
		_Ret =_CurActivity.Call<int>("OnUnityMagneticMode",_Param);
		_Param = null;
		return _Ret;
	}
	
	//
	public override string GetCountry()
	{
		//CN TW HK
		AndroidJavaObject  _CurActivity=GetActivity();
		string _Ret =_CurActivity.Call<string>("OnUnityGetCountry");
		return _Ret;
	}
	
	public override int Reconnect()
	{
		return 0;
	}
#elif UNITY_IPHONE && !UNITY_EDITOR
	//=====================================
	//Receiver
	//=====================================
	private string deviceMac="";
	private int deviceNo=-1;
	
	public void OnJniConnectResult (string result)
	{
		Debug.Log("OnJniConnectResult(): " + result);
		PlayBandConnectData resultData = new PlayBandConnectData (result);
		ConnectAddress = resultData.address;
		
		//edit by BlackSmith Jack, 20150717
		if(OnConnectResultEvent != null)
		{
			OnConnectResultEvent (resultData);
		}
		
		deviceMac=resultData.address;
		deviceNo=0;
		
		if (resultData.success) {
			Invoke ("FixedNorth", 1.0f);
			StartReciveData ();
		} else {
			Debug.Log("Message : " + resultData.errorMessage);
			Calibrated = false;
			StopReciveData ();
			PlayBand.IsCalibrating = false;
			//PlayBand.OnIncomingDataEvent -= UpdateJiugongge;
			//PlayBand.Device1.OnButtonClickedEvent -= ButtonClickedP1;
		}
		deviceList[0].CallOnConnectResultEvent(resultData);
	}
	
	public void OnJniButtonClicked (string result)
	{
		if (OnButtonClickedEvent != null) {
			OnButtonClickedEvent (new PlayBandID (deviceMac,deviceNo));
		}
		
		deviceList[deviceNo].CallOnButtonClickedEvent();
		
		if(useButtonCorrection){
			FixedNorth(deviceNo);
		}
	}
	//
	public void OnJniBatteryStatus (string result)
	{
		PlayBandBatteryData batteryData=new PlayBandBatteryData (result,deviceMac,deviceNo);
		
		if (OnBatteryStatusEvent != null) {
			OnBatteryStatusEvent (batteryData);
		}
		
		deviceList[deviceNo].CallOnBatteryStatusEvent(batteryData);
		
	}
	
	public void OnJniWarningStatus (string result)
	{
		PlayBandWarningData warningData=new PlayBandWarningData (result,deviceMac,deviceNo);
		
		if (OnWarningDataEvent != null) {
			OnWarningDataEvent (warningData);
		}
		
		deviceList[deviceNo].CallOnWarningDataEvent(warningData);
	}
	
	public void OnJniMagnetCalibrated (string result)
	{
		Debug.Log("OnJniMagnetCalibrated result:"+result);
		string[] separators={","};
		string[] dataList=result.Split(separators,0);
		Calibrated = (result == "1");
		if( dataList[0] == "1" ) // done calibrate
		{
			PlayBand.IsCalibrating = false;
		}
		else if(dataList[0] == "0") // is calibrating
		{
			PlayBand.IsCalibrating = true;
		}
		else
		{
			PlayBand.IsCalibrating = false;
		}
		PlayBandID playBandID=new PlayBandID (deviceMac,deviceNo);
		
		if (OnCalibratedEvent != null) {
			OnCalibratedEvent (playBandID);
		}
		
		Debug.Log("OnJniMagnetCalibrated:"+PlayBand.IsCalibrating);
	}
	//
	public void OnJniSensorStatus (string result)
	{
		MobileSensor.Update (result);
	}
	//=====================================
	//Sender
	//=====================================
	[DllImport ("__Internal")]
	private static extern string OnUnityGetSensor();
	
	[DllImport ("__Internal")]
	private static extern string OnUnityGetCountry();
	
	[DllImport ("__Internal")]
	private static extern int OnUnityConnect (string deviceName,int mode,int sensorRate);
	
	[DllImport ("__Internal")]
	private static extern int OnUnityReconnect ();
	
	[DllImport ("__Internal")]
	private static extern int OnUnityChangeMode(int mode, int dataPerSecond);
	
	[DllImport ("__Internal")]
	private static extern int OnUnityDisconnect();
	
	[DllImport ("__Internal")]
	private static extern int OnUnityInquireBatteryStatus();
	
	[DllImport ("__Internal")]
	private static extern int OnUnityControlLED(int iParam1, int iParam2, int iParam3, int iParam4,int iParam5);
	
	[DllImport ("__Internal")]
	private static extern int OnUnityDoVibrate (int iParam1, int iParam2, int iParam3, int iParam4);
	
	[DllImport ("__Internal")]
	private static extern int OnUnityMagneticMode(int mode);
	
	[DllImport ("__Internal")]
	private static extern int OnUnityDoCalibrate();
	
	[DllImport ("__Internal")]
	private static extern int OnUnityCancelCalibration();
	
	[DllImport ("__Internal")]
	private static extern int OnUnitySaveMagneticCalibrationData();
	
	
	private static string cacheData;
	
	public override void GetSensorData()
	{
		if(PlayBand.IsCalibrating)
			return;
		cacheData=OnUnityGetSensor();
		bandDataList[0].Update(cacheData);
	}
	
	public override int InquireBatteryStatus(int deviceNo)
	{
		return OnUnityInquireBatteryStatus ();
	}
	
	public override int LEDOff(PlayBandLEDColor ledColor,int deviceNo)
	{
		return OnUnityControlLED((int)ledColor,0,0,0,0);
	}
	
	public override int LEDOff(int ledColor,int deviceNo)
	{
		return LEDOff((PlayBandLEDColor)ledColor,deviceNo);
	}
	
	public override int LEDOn(PlayBandLEDColor ledColor, int brightness,int deviceNo)
	{
		return OnUnityControlLED((int)ledColor,1,brightness,0,0);
	}
	
	public override int LEDOn(int ledColor, int brightness,int deviceNo)
	{
		return LEDOn((PlayBandLEDColor)ledColor,brightness,deviceNo);
	}
	
	public override int LEDFlash(PlayBandLEDColor ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo)
	{
		return OnUnityControlLED((int)ledColor,2,brightness,onPeriod,offPeriod);
	}
	
	public override int LEDFlash(int ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo)
	{
		return LEDFlash((PlayBandLEDColor)ledColor,brightness,onPeriod,offPeriod,deviceNo);
	}
	
	public override int VibrateOnce(int power, int period,int deviceNo)
	{
		return OnUnityDoVibrate(1,power,period,0);
	}
	
	public override int VibrateTwice(int power, int onPeriod, int offPeriod,int deviceNo)
	{
		return OnUnityDoVibrate(2,power,onPeriod,offPeriod);
	}
	//--------------------------------------------------------------------
	public override int Connect(PlayBandPowerMode mode,string deviceName,PlayBandSensorRate sensorRate )
	{
		PowerMode=mode;
		SensorDataRate=sensorRate;
		int _Ret = OnUnityConnect(deviceName, (int) mode,(int)sensorRate);
		if(_Ret==0){
			Calibrated=false;
		}
		return _Ret;
	}
	//
	public override int Reconnect()
	{
		return OnUnityReconnect();
	}
	
	public override int SetPowerMode(PlayBandPowerMode mode,PlayBandSensorRate sensorRate)
	{
		return OnUnityChangeMode((int) mode,(int)sensorRate);
	}
	
	public override int Disconnect(bool result)
	{
		//connected=false;
		//return OnUnityDisconnect();
		instance.StopReciveData();
		instance.deviceList[0].Disconnect();
		OnUnityDisconnect();
		return 0;
	}
	
	public override int SetMagenticMode(PlayBandMagenticMode mode,int deviceNo)
	{
		return OnUnityMagneticMode((int) mode);
	}
	
	public override string GetCountry()
	{
		//CN TW HK
		return OnUnityGetCountry ();
	}
	
	void OnApplicationPause(bool paused) {
		if(paused){
			OnUnityDisconnect();
		}else{
			Reconnect();
		}
	}
#elif UNITY_STANDALONE_WIN
    const int RESULT_CONNECT = 0;
	const int RESULT_BATTERY = 1;
	const int RESULT_BUTTON = 2;
	const int RESULT_CALIBRATION = 3;
	const int RESULT_SENSOR = 4;
	const int RESULT_WARNING = 5;
	const int RESULT_COUNTRY = 6;
	const int RESULT_NO_BRACELET = 100;
	const int RESULT_SELECT_BRACELET = 101;
	const int RESULT_DEBUG_LOG = 102;
	string countryString;
	public delegate void OnJniResult(int methodCode, string msg );

    void onJniResult(int methodCode, string msg)
    {
        Debug.Log("code: " + methodCode + ", msg: " + msg);
		//return;
        switch (methodCode)
        {
            case RESULT_CONNECT:
			OnJniConnectResult(msg);
				break;
			case RESULT_BATTERY:
                OnJniBatteryStatus(msg);
                break;
            case RESULT_BUTTON:
                OnJniButtonClicked(msg);
                break;
            case RESULT_CALIBRATION:
                OnJniMagnetCalibrated(msg);
                break;
            case RESULT_SENSOR:
                OnJniSensorStatus(msg);
                break;
            case RESULT_WARNING:
                OnJniWarningStatus(msg);
			break;
			case RESULT_COUNTRY:
				instance.countryString = ResultSplit(msg)[2];
			break;
            case RESULT_NO_BRACELET:
				OnNoBracelet(msg);
                break;
			case RESULT_SELECT_BRACELET:
				OnSelectDevice(msg);
                break;
            case RESULT_DEBUG_LOG:
				Debug.Log ("RESULT_DEBUG: " + msg);
                break;
        }
	}

	IEnumerator MyCoroutine()
	{	
		yield return 0;    //Wait one frame
		Debug.Log ("Wait 1 frame!!");
	}

	private void OnSelectDevice(string result)
	{
		string[] deviceList = ResultSplit(result)[2].Split(',');
		if (SelectDeviceList.Instance == null)
		{
			Debug.Log("You didn't set the deviceListCanvas prefab in running scene! Unable to show Band list.");
			return;
		}
		if (deviceList.Length == 0) 
		{
			SelectDeviceList.ShowNoBraceletDialog();
			return;
		}
		for(int i = 0; i < deviceList.Length; i++)
		{
			if(deviceList[i] == "")
			{
				continue;
			}
			Debug.Log("Unity: Device " + i +"'s name = " + (string)deviceList[i]);
			SelectDeviceList.AddDevice(deviceList[i]);
		}
		SelectDeviceList.ShowScrollView();
	}
	
	private void OnNoBracelet(string result)
	{
		if (SelectDeviceList.Instance == null)
		{
			Debug.Log("You didn't set the deviceListCanvas prefab in running scene! Unable to show Band list.");
			return;
		}
		SelectDeviceList.ShowNoBraceletDialog();
	}

	public string[] ResultSplit(string result)
	{
		return result.Split('@');
	}

	public void OnJniConnectResult (string result)
	{
		string deviceMac="";
		int deviceNo=-1;

		string[] stringList = ResultSplit(result);

		deviceNo = int.Parse(stringList[0]);
		deviceMac = stringList[1];
		result = stringList[2];
		
		PlayBandConnectData resultData = new PlayBandConnectData (result,deviceMac,deviceNo);
		ConnectAddress = resultData.address;
		
		Debug.Log("deviceNo: " + deviceNo + ",  [errorMessage] status: " + resultData.status + ",  msg: " + resultData.errorMessage);
		
		//edit by BlackSmith Jack, 20150717
		if( OnConnectResultEvent != null )
		{
			OnConnectResultEvent (resultData);
		}
		
		if(deviceNo==0){
			if (resultData.success) {
				//Invoke ("FixedNorth", 1.0f);
				StartReciveData ();
			} else {
				Debug.Log("error : " + resultData.errorMessage);
				Calibrated = false;
				StopReciveData ();
			}
		}
		Debug.Log("deviceList[deviceNo]: " + DeviceList[deviceNo]);
		DeviceList[deviceNo].CallOnConnectResultEvent(resultData);
		
	}
	string buttonClickedData = "";
	string batteryStatusData = "";
	string warningStatusData = "";
	string magneticCalibratedData = "";
	bool onButtonClicked = false, onBatteryStatus = false, onWarningStatus = false, onMagneticCalibreted = false;

	private void CallbackOnMainThread()
	{
		if (onButtonClicked) 
		{
			if(buttonClickedData != "")
			{
				ButtonClicked(buttonClickedData);
				onButtonClicked = false;
				buttonClickedData = "";
			}
		}

		if (onBatteryStatus) 
		{
			if(batteryStatusData != "")
			{
				BatteryStatus(batteryStatusData);
				onBatteryStatus = false;
				batteryStatusData = "";
			}
		}

		if (onWarningStatus) 
		{
			if(warningStatusData != "")
			{
				WarningStatus(warningStatusData);
				onWarningStatus = false;
				warningStatusData = "";
			}
		}
		
		if (onMagneticCalibreted) 
		{
			if(magneticCalibratedData != "")
			{
				MagneticCalibrated(magneticCalibratedData);
				onMagneticCalibreted = false;
				magneticCalibratedData = "";
			}
		}
	}

    public void OnJniButtonClicked(string result)
	{
		instance.buttonClickedData = result;
		instance.onButtonClicked = true;
    }
	private void ButtonClicked(string result)
	{
		string deviceMac="";
		int deviceNo=-1;
		
		string[] stringList = ResultSplit(result);
		deviceNo = int.Parse(stringList[0]);
		deviceMac = stringList[1];
		result = stringList[2];
		//		UpdateMouseClick (1);
		
		if (OnButtonClickedEvent != null)
		{
			OnButtonClickedEvent(new PlayBandID(deviceMac, deviceNo));
		}
		
		DeviceList[deviceNo].CallOnButtonClickedEvent();
		
		if (instance.useButtonCorrection)
		{
			instance.FixedNorth(deviceNo);
			//FixedNorth(deviceNo);
		}
		Debug.Log("TTT_OnJniButtonClicked In:" + deviceMac);
		Debug.Log("TTT_OnJniButtonClicked To:" + deviceNo + "  " + DeviceList[deviceNo].address);
	}
    //
    public void OnJniBatteryStatus(string result)
	{
		instance.batteryStatusData = result;
		instance.onBatteryStatus = true;
    }
	private void BatteryStatus(string result)
	{
		string deviceMac="";
		int deviceNo=-1;
		
		string[] stringList = ResultSplit(result);
		deviceNo = int.Parse(stringList[0]);
		deviceMac = stringList[1];
		result = stringList[2];
		
		PlayBandBatteryData batteryData = new PlayBandBatteryData(result, deviceMac, deviceNo);
		
		if (OnBatteryStatusEvent != null)
		{
			OnBatteryStatusEvent(batteryData);
		}
		
		DeviceList[deviceNo].CallOnBatteryStatusEvent(batteryData);
	}
    public void OnJniWarningStatus(string result)
	{
		instance.warningStatusData = result;
		instance.onWarningStatus = true;
    }
	private void WarningStatus(string result)
	{
		string deviceMac="";
		int deviceNo=-1;
		
		string[] stringList = ResultSplit(result);
		deviceNo = int.Parse(stringList[0]);
		deviceMac = stringList[1];
		result = stringList[2];
		
		PlayBandWarningData warningData = new PlayBandWarningData(result, deviceMac, deviceNo);
		
		if (OnWarningDataEvent != null)
		{
			OnWarningDataEvent(warningData);
		}
		
		DeviceList[deviceNo].CallOnWarningDataEvent(warningData);
	}

    public void OnJniMagnetCalibrated(string result)
    {
		instance.magneticCalibratedData = result;
		instance.onMagneticCalibreted = true;
	}
	private void MagneticCalibrated(string result)
	{
		string deviceMac="";
		int deviceNo=-1;
		
		string[] stringList = ResultSplit(result);
		Debug.Log("OnJniMagnetCalibrated result:" + result);
		
		deviceNo = int.Parse(stringList[0]);
		deviceMac = stringList[1];
		result = stringList[2];
		
		string[] separators = { "," };
		string[] dataList = result.Split(separators, 0);
		Calibrated = (result == "1");
		if (dataList[0] == "1") // done calibrate
		{
			PlayBand.IsCalibrating = false;
		}
		else if (dataList[0] == "0") // is calibrating
		{
			PlayBand.IsCalibrating = true;
		}
		else
		{
			PlayBand.IsCalibrating = false;
		}
		PlayBandID playBandID = new PlayBandID(deviceMac, deviceNo);
		
		if (OnCalibratedEvent != null)
		{
			OnCalibratedEvent(playBandID);
		}	
		Debug.Log("OnJniMagnetCalibrated:" + PlayBand.IsCalibrating);
	}
    //
    public void OnJniSensorStatus(string result)
	{
		string deviceMac="";
		int deviceNo=-1;
		
		string[] stringList = ResultSplit(result);
        deviceNo = int.Parse(stringList[0]);
        deviceMac = stringList[1];
        result = stringList[2];

        MobileSensor.Update(result);
    }

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern int OnUnityExit();

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnitySetCallback(OnJniResult fp);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern int OnUnityConnect(string deviceName, int powerMode, int sensorRate);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnityInquireBateryStatus();

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnityControlLED(int deviceNo, int iParm1, int iParm2, int iParm3, int iParm4, int iParm5);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnityDoVibrate(int deviceNo, int iParm1, int iParm2, int iParm3, int iParm4);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnitySetPowerMode(int deviceNo, int powerMode, int sensorRate);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnityDisconnect();

    //[DllImport("CavyPlayBandSDKWindowsNative")]
    //private static extern void OnUnitySaveMagneticCalibrationData();

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnitySetMagnticMode(int deviceNo, int mode);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern void OnUnityGetSensorData(float[] data);

    [DllImport("CavyPlayBandSDKWindowsNative")]
    private static extern int GetDeviceCount();

    private float[] cacheData = new float[14];
    public override void GetSensorData()
    {
        OnUnityGetSensorData(cacheData);
        bandDataList[0].Update(cacheData);
    }

    public override int InquireBatteryStatus(int deviceNo)
    {
        OnUnityInquireBateryStatus();
        return 0;
    }

    public override int LEDOff(PlayBandLEDColor ledColor, int deviceNo)
    {
        OnUnityControlLED(0, (int)ledColor, 0, 0, 0, 0);
        return 0;
    }

    public override int LEDOff(int ledColor, int deviceNo)
    {
        return LEDOff((PlayBandLEDColor)ledColor, deviceNo);
    }

    public override int LEDOn(PlayBandLEDColor ledColor, int brightness, int deviceNo)
    {
        OnUnityControlLED(0, (int)ledColor, 1, brightness, 0, 0);
        return 0;
    }

    public override int LEDOn(int ledColor, int brightness, int deviceNo)
    {
        return LEDOn((PlayBandLEDColor)ledColor, brightness, deviceNo);
    }

    public override int LEDFlash(PlayBandLEDColor ledColor, int brightness, int onPeriod, int offPeriod, int deviceNo)
    {
        OnUnityControlLED(0, (int)ledColor, 2, brightness, onPeriod, offPeriod);
        return 0;
    }

    public override int LEDFlash(int ledColor, int brightness, int onPeriod, int offPeriod, int deviceNo)
    {
        return LEDFlash((PlayBandLEDColor)ledColor, brightness, onPeriod, offPeriod, deviceNo);
    }

    public override int VibrateOnce(int power, int period, int deviceNo)
    {
        OnUnityDoVibrate(0, 1, power, period, 0);
        return 0;
    }

    public override int VibrateTwice(int power, int onPeriod, int offPeriod, int deviceNo)
    {
        OnUnityDoVibrate(0, 2, power, onPeriod, offPeriod);
        return 0;
    }
    //--------------------------------------------------------------------
    public override int Connect(PlayBandPowerMode mode, string deviceName, PlayBandSensorRate sensorRate)
    {
        PowerMode = mode;
        SensorDataRate = sensorRate;
        int _Ret = 0;
        OnUnityConnect(deviceName, (int)mode, (int)sensorRate);
        if (_Ret == 0)
        {
            Calibrated = false;
        }
        return _Ret;
    }
    //
    public override int Reconnect()
    {
        OnUnityConnect(deviceList[0].address, (int)PlayBandPowerMode.Performance, 25);
        return 0;
    }

    public override int SetPowerMode(PlayBandPowerMode mode, PlayBandSensorRate sensorRate)
    {
        OnUnitySetPowerMode(0, (int)mode, (int)sensorRate);
        return 0;
    }

    public override int Disconnect(bool result)
    {
        //Connected=false;
        //return OnUnityDisconnect();
        instance.StopReciveData();
        instance.deviceList[0].Disconnect();
        OnUnityDisconnect();
        return 0;
    }

    public override int SetMagenticMode(PlayBandMagenticMode mode, int deviceNo)
	{
		OnUnitySetMagnticMode(0, (int)PlayBandMagenticMode.NoMove);
        return 0;
    }

    public override string GetCountry()
	{
		return instance.countryString;
    }

    void OnApplicationQuit()
    {
        Debug.Log(OnUnityExit());
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

#elif UNITY_WSA || NETFX_CORE || UNITY_METRO
    private CavyBandSDK bandSDK = null;
	private string deviceMac="";
	private int deviceNo=-1;
	private CavyBandSDK WinSDK()
	{
		if(bandSDK == null)
		{
			bandSDK = new CavyBandSDK();
			bandSDK.OnConnectResult += OnJniConnectResult;
			bandSDK.OnButtonClicked += OnJniButtonClicked;
			bandSDK.OnBatteryStatus += OnJniBatteryStatus;
			bandSDK.OnMagneticCalibrated += OnJniMagnetCalibrated;
			bandSDK.OnSensorStatus += OnJniSensorStatus;
			bandSDK.OnWarningStatus += OnJniWarningStatus;
			bandSDK.OnSelectBracelet += OnSelectDevice;
			bandSDK.OnErrorResult += OnErrorResult;
			Debug.Log("Metro ver!!");
		}
		return bandSDK;
	}
	private void OnSelectDevice(List<string> deviceList)
	{
        if (SelectDeviceList.Instance == null)
        {
            Debug.Log("You didn't set the deviceListCanvas prefab in running scene! Unable to show Band list.");
            return;
        }
		for(int i = 0; i < deviceList.Count; i++)
		{
			Debug.Log("Unity: Device " + i +"'s name = " + (string)deviceList[i]);
			SelectDeviceList.AddDevice(deviceList[i]);
		}
		SelectDeviceList.ShowScrollView();
	}

	private void OnErrorResult(int errorCode)
	{
		if(errorCode == (int)PlayBandConnectStatus.NoBracelet)
        {
            Debug.Log("You didn't set the deviceListCanvas prefab in running scene! Unable to show Error dialog.");
            SelectDeviceList.ShowNoBraceletDialog();
		}
	}

	public void OnJniConnectResult(int deviceNumber, string deviceName, string resultString)
	{
		//gameObject.SendMessage("OnJniConnectResult", deviceNumber + "@" + deviceName + "@" + resultString);
		deviceNo = deviceNumber;
		deviceMac = deviceName;
		Debug.Log("result string: " + resultString);
		PlayBandConnectData resultData = new PlayBandConnectData (resultString,deviceMac,deviceNo);
		ConnectAddress = resultData.address;
		
		Debug.Log("deviceNo: " + deviceNo + ",  [errorMessage] status: " + resultData.status + ",  msg: " + resultData.errorMessage);
		
		//edit by BlackSmith Jack, 20150717
		if( OnConnectResultEvent != null )
		{
			OnConnectResultEvent (resultData);
		}
		
		if(deviceNo==0){
			if (resultData.success)
            {
                Invoke("FixedNorth", 1.0f);
                StartReciveData();
            } else
            {
                Debug.Log("error : " + resultData.errorMessage);
                Calibrated = false;
                StopReciveData();
            }
		}
		Debug.Log("deviceList[deviceNo]: " + deviceList[deviceNo]);
		deviceList[deviceNo].CallOnConnectResultEvent(resultData);
	}
//	private string[] stringList=new string[3];
	
	public void OnJniButtonClicked (int deviceNumber, string deviceName, string result)
	{

		deviceNo=deviceNumber;
		deviceMac=deviceName;
		
		if (OnButtonClickedEvent != null) {
			OnButtonClickedEvent (new PlayBandID (deviceMac,deviceNo));
		}
		Debug.Log("TTT_OnJniButtonClicked In:"+deviceMac);
		Debug.Log("TTT_OnJniButtonClicked To:"+deviceNo+"  "+deviceList[deviceNo].address);
		
		deviceList[deviceNo].CallOnButtonClickedEvent();
		
		if(useButtonCorrection){
			FixedNorth(deviceNo);
		}
		
	}
	//
	public void OnJniBatteryStatus (int deviceNumber, string deviceName, string result)
	{
		deviceNo= deviceNumber;
		deviceMac=deviceName;
		
		PlayBandBatteryData batteryData=new PlayBandBatteryData (result,deviceMac,deviceNo);
		
		if (OnBatteryStatusEvent != null) {
			OnBatteryStatusEvent (batteryData);
		}
		
		deviceList[deviceNo].CallOnBatteryStatusEvent(batteryData);
		
	}
	
	public void OnJniWarningStatus (int deviceNumber, string deviceName, string result)
	{
		deviceNo= deviceNumber;
		deviceMac=deviceName;
		
		PlayBandWarningData warningData=new PlayBandWarningData (result,deviceMac,deviceNo);
		
		if (OnWarningDataEvent != null) {
			OnWarningDataEvent (warningData);
		}
		
		deviceList[deviceNo].CallOnWarningDataEvent(warningData);
	}
	
	public void OnJniMagnetCalibrated (int deviceNumber, string deviceName, string result)
	{
		deviceNo= deviceNumber;
		deviceMac=deviceName;
		
		Calibrated = (result == "1");
		
		PlayBandID playBandID=new PlayBandID (deviceMac,deviceNo);
		
		if (OnCalibratedEvent != null) {
			OnCalibratedEvent (playBandID);
		}
		
	}
	//
	public void OnJniSensorStatus (int deviceNumber, string deviceName, string result)
	{
		deviceNo= deviceNumber;
		deviceMac=deviceName;
		
		MobileSensor.Update (result);
	}

	//====================================
	//Sender
	//====================================
	private float[] cacheData;
	public override void GetSensorData()
	{
		cacheData= WinSDK().OnUnityGetSensorData();
		bandDataList[0].Update(cacheData);
	}
	
	public override int InquireBatteryStatus(int deviceNo)
	{
		return WinSDK().OnUnityInquireBateryStatus();
	}
	
	public override int LEDOff(PlayBandLEDColor ledColor,int deviceNo)
	{
		return WinSDK().OnUnityControlLED(0, (int)ledColor, 0, 0, 0, 0);
	}
	
	public override int LEDOff(int ledColor,int deviceNo)
	{
		return LEDOff((PlayBandLEDColor)ledColor,deviceNo);
	}
	
	public override int LEDOn(PlayBandLEDColor ledColor, int brightness,int deviceNo)
	{
		return WinSDK().OnUnityControlLED(0, (int)ledColor, 1, brightness, 0, 0);
	}
	
	public override int LEDOn(int ledColor, int brightness,int deviceNo)
	{
		return LEDOn((PlayBandLEDColor)ledColor,brightness,deviceNo);
	}
	
	public override int LEDFlash(PlayBandLEDColor ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo)
	{
		return WinSDK().OnUnityControlLED(0, (int)ledColor, 2, brightness, onPeriod, offPeriod);
	}
	
	public override int LEDFlash(int ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo)
	{
		return LEDFlash((PlayBandLEDColor)ledColor,brightness,onPeriod,offPeriod,deviceNo);
	}
	
	public override int VibrateOnce(int power, int period,int deviceNo)
	{
		return WinSDK().OnUnityDoVibrate(0, 1, power, period, 0);
	}
	
	public override int VibrateTwice(int power, int onPeriod, int offPeriod,int deviceNo)
	{
		return WinSDK().OnUnityDoVibrate(0, 2, power, onPeriod, offPeriod);
	}
	//--------------------------------------------------------------------
	public override int Connect(PlayBandPowerMode mode,string deviceName,PlayBandSensorRate sensorRate )
	{
		PowerMode=mode;
		SensorDataRate=sensorRate;
		int _Ret = 0;
		WinSDK().OnUnityConnect(deviceName, (int) mode,(int)sensorRate);
		if(_Ret==0){
			Calibrated=false;
		}
		return _Ret;
	}
	//
	public override int Reconnect()
	{
		return WinSDK().OnUnityReconnect();
	}
	
	public override int SetPowerMode(PlayBandPowerMode mode,PlayBandSensorRate sensorRate)
	{
		return WinSDK().OnUnitySetPowerMode(0, (int) mode,(int)sensorRate);
	}
	
	public override int Disconnect(bool result)
	{
		//Connected=false;
		//return OnUnityDisconnect();
		instance.StopReciveData();
		instance.deviceList[0].Disconnect();
		WinSDK().OnUnityDisconnect();
		return 0;
	}
	
	public override int SetMagenticMode(PlayBandMagenticMode mode,int deviceNo)
	{
		return WinSDK().OnUnitySetMagnticMode(0, (int) mode);
	}
	
	public override string GetCountry()
	{
		//CN TW HK
		return WinSDK().OnUnityGetCountry();
	}
	
	void OnApplicationPause(bool paused) {
		//if(paused){
		//	WinSDK().OnUnityDisconnect();
		//}else{
		//	Reconnect();
		//}
	}

#elif UNITY_EDITOR
	public override int Connect(PlayBandPowerMode mode,string deviceName,PlayBandSensorRate sensorRate ){return 0;}
	public override int Reconnect(){return 0;}
	public override int SetPowerMode(PlayBandPowerMode mode,PlayBandSensorRate sensorRate){return 0;}
	public override int SetMagenticMode(PlayBandMagenticMode mode,int deviceNo){return 0;}
	public override int Disconnect(bool result){return 0;}
	public override void GetSensorData(){}
	public override string GetCountry(){return "";}
	public override int InquireBatteryStatus(int deviceNo){return 0;}
	public override int LEDOff(PlayBandLEDColor ledColor,int deviceNo){return 0;}
	public override int LEDOff(int ledColor,int deviceNo){return 0;}
	public override int LEDOn(PlayBandLEDColor ledColor, int brightness,int deviceNo){return 0;}
	public override int LEDOn(int ledColor, int brightness,int deviceNo){return 0;}
	public override int LEDFlash(PlayBandLEDColor ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo){return 0;}
	public override int LEDFlash(int ledColor, int brightness, int onPeriod, int offPeriod,int deviceNo){return 0;}
	public override int VibrateOnce(int power, int period,int deviceNo){return 0;}
	public override int VibrateTwice(int power, int onPeriod, int offPeriod,int deviceNo){return 0;}
#endif

    //--------------------------------------------------------------------
    //common
    //--------------------------------------------------------------------
    public static int Connect(PlayBandPowerMode mode,string deviceName)
	{
		return instance.Connect(mode,deviceName,SensorDataRate);
	}
    public static int Connect(string deviceName)
    {
        return instance.Connect(PlayBandPowerMode.Performance, deviceName, SensorDataRate);
    }
	//
	public static int Connect(PlayBandPowerMode mode)
	{
		return instance.Connect(mode,"",SensorDataRate);
	}
	//
	public static int Connect(PlayBandSensorRate sensorRate)
	{
		SensorDataRate=sensorRate;
		return instance.Connect(PowerMode,"",sensorRate);
	}
	
	public static int Connect()
	{
		return instance.Connect(PowerMode,"",SensorDataRate);
	}
	//
	
	public static int Disconnect()
	{
		return instance.Disconnect(true);
	}
	
	public static int SetSensorRate(PlayBandSensorRate sensorRate)
	{
		return instance.SetPowerMode(PlayBandPowerMode.Performance,sensorRate);
	}
	
	public static int SetPowerMode(PlayBandPowerMode mode)
	{
		return instance.SetPowerMode(mode,instance.sensorDataRate);
	}
	//
	public override void FixedNorth(int deviceNo)
	{
		instance.SetMagenticMode(PlayBandMagenticMode.NoMove, deviceNo);
		//Debug.Log("Fixed North!! set magnetic mode!!");
	}
	//--------------------------------------------------------------------
	//for v1.0 single band
	//--------------------------------------------------------------------
	public static int InquireBatteryStatus ()
	{
		return instance.InquireBatteryStatus (0);
	}
	
	public static int LEDOff (PlayBandLEDColor ledColor)
	{
		return  instance.LEDOff (ledColor, 0);
	}
	
	public static int LEDOn (PlayBandLEDColor ledColor, int brightness)
	{
		return instance.LEDOn (ledColor, brightness, 0);
	}
	
	public static int LEDFlash (PlayBandLEDColor ledColor, int brightness, int onPeriod, int offPeriod)
	{
		return instance.LEDFlash (ledColor, brightness, onPeriod, offPeriod, 0);
	}
	
	public static int VibrateOnce (int power, int period)
	{
		return instance.VibrateOnce (power, period, 0);
	}
	
	public static int VibrateTwice (int power, int onPeriod, int offPeriod)
	{
		return instance.VibrateTwice (power, onPeriod, offPeriod, 0);
	}
	
	public static void FixedNorth()
	{
		instance.FixedNorth (0);
	}
	//=====================================
	//Decide Way
	//=====================================
	private bool _exactAngleMode = false;
	//
	public static bool ExactAngleMode {
		get { return instance._exactAngleMode; }
		set {
			instance._exactAngleMode = value;
			instance.deviceList[0].mExactAngleMode = value;
			instance.deviceList[1].mExactAngleMode = value;
			instance.deviceList[2].mExactAngleMode = value;
			instance.deviceList[3].mExactAngleMode = value;
		}
	}
	
	[HideInInspector]
	public PlayBandMouse mouseComponent;
	public static void StartMouseListener ()
	{
		GameObject obj=GameObject.Find ("PlayBandMouse");
		if(obj!=null){
			instance.mouseComponent=obj.GetComponent<PlayBandMouse>();
			if(instance.mouseComponent!=null){
				instance.mouseComponent.StartMouseListener();
			}
		}
	}
	public static void StopMouseListener ()
	{
		if(instance.mouseComponent!=null){
			instance.mouseComponent.StopMouseListener();
		}
	}
}
