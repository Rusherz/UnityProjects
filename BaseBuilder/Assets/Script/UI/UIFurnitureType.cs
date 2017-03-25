using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFurnitureType : MonoBehaviour {

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

		string s = "NULL";

		if (t.furniture != null) {
			s = t.furniture.objectType;
		}

		myText.text = "Furniture Type: " + s;
	}
}
