using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Success : MonoBehaviour {

	public Text txt;

	// Use this for initialization
	void Start () {
		int amt = PlayerPrefs.GetInt ("CREAM");
		txt.text = "Nicely Done!\nYou escaped with $" + amt;
	}
	

}
