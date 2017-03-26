using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomDetails : MonoBehaviour {

	Text myText;
	MouseController mouseController;

	// Use this for initialization
	void Start () {
		myText = GetComponent<Text> ();
		mouseController = GameObject.FindObjectOfType<MouseController> ();
	}
	
	// Update is called once per frame
	void Update () {
		Tile t = mouseController.GetMouseOverTile ();
		if (t == null || t.room == null) {
			myText.text = "No Gases.";
			return;
		}
		string s = "";
		foreach (string g in t.room.GetGases()) {
			s += g + ": " + string.Format("{0:0.00}", t.room.GetGasAmount(g)) + "( " + string.Format("{0:0.00}", (t.room.GetGasPercentage(g) * 100)) + "%)";
		}
		myText.text = s;
	}
}
