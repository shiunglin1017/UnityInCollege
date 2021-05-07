using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class SelectDeviceList : MonoBehaviour {
	private static SelectDeviceList mInstance = null;
	public GameObject scrollViewContent;
	public GameObject devicePrefab;
	private int ButtonCount = 0;
	private List<GameObject> buttonList;
	public GameObject DeviceListObj;
	public GameObject NoBraceletDialogObj;
    public static SelectDeviceList Instance
    {
        get
        {
            return mInstance;
        }
    }
	// Use this for initialization
	void Start () {
        if (mInstance == null)
        {
            mInstance = this;
            buttonList = new List<GameObject>();
            //gameObject.SetActive(false);
            DeviceListObj.SetActive(false);
            NoBraceletDialogObj.SetActive(false);
            DontDestroyOnLoad(this);
            Debug.Log("SelectDeviceList Init Success");
        }
        else
        {
            Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void ShowNoBraceletDialog()
	{
		mInstance.NoBraceletDialogObj.SetActive(true);
	}

	public static void HideNoBraceletDialog()
	{
		mInstance.hideNoBraceletDialog();
	}

	public void hideNoBraceletDialog()
	{
		NoBraceletDialogObj.SetActive(false);
	}

	public static void ShowScrollView()
	{
		mInstance.DeviceListObj.SetActive(true);
	}

	public static void HideScrollView()
	{
		mInstance.DeviceListObj.SetActive(false);
		mInstance.ButtonCount = 0;
		for(int i = 0; i < mInstance.buttonList.Count; i++)
		{
			Destroy(mInstance.buttonList[i]);
		}
		mInstance.buttonList.Clear();
		//mInstance.scrollViewContent.GetComponent<RectTransform>().rect.height = 200;
	}

	public static void AddDevice(string name)
	{
		mInstance.addDeviceButton(name);
	}

	private void addDeviceButton(string name)
	{
		GameObject newButton = GameObject.Instantiate(devicePrefab) as GameObject;
		SelectDeviceButton deviceButton = newButton.GetComponent<SelectDeviceButton>();
		deviceButton.setName(name);
		newButton.transform.SetParent(scrollViewContent.transform);
		newButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		buttonList.Add(newButton);
		ButtonCount++;
		if(ButtonCount > 5)
		{
			Rect temp = scrollViewContent.GetComponent<RectTransform>().rect;
			temp.height += 50;
			scrollViewContent.GetComponent<RectTransform>().rect.Set(temp.position.x, temp.position.y, temp.width, temp.height);
			//scrollViewContent.GetComponent<RectTransform>().rect.height += 50;
		}
	}
}
