using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectDeviceButton : MonoBehaviour {
	private string name;
	public Text buttonText = null;
	// Use this for initialization
	void Start () {
	}

	public void setName(string str)
	{
		if(buttonText == null)
			buttonText = gameObject.GetComponentInChildren<Text>();
		name = str;
		buttonText.text = name;
	}

	public void onClickDeviceButton()
	{
		PlayBand.Connect(name);
		SelectDeviceList.HideScrollView();
	}
}
