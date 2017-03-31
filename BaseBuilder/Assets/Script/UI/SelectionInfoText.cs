using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionInfoText : MonoBehaviour {

    public CanvasGroup canvasGroup;

    MouseController mc;
    Text selectText;

	// Use this for initialization
	void Start () {
        mc = GameObject.FindObjectOfType<MouseController>();
        selectText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (mc.mySelection != null) {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            ISelectableInterface selection = mc.mySelection.objectArray[mc.mySelection.subSelection];
            string s = "Name: " + selection.GetName() + "\nDescription: " + selection.GetDescription() + "\nHealth: " + selection.GetHitPoints();
            selectText.text = s;
        } else {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
	}

}
