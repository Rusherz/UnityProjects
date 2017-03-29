using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomIndex : MonoBehaviour {

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
        string s = "N/A";
        if(t.room != null) {
            s = t.room.ID.ToString();
        }
		myText.text = "Room Index: " + s;
	}
}
