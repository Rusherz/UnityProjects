using UnityEngine;
using System.Collections;

public class AmmoDisplay : MonoBehaviour {

    private GUIText ammo;

	// Use this for initialization
	void Start () {
        ammo = GetComponentInChildren<GUIText>();
	}
	
	// Update is called once per frame
	void Update () {
        ammo.transform.position = GetComponent<Camera>().WorldToViewportPoint(transform.position);
    }
}
