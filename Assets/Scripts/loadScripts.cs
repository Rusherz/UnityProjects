﻿using UnityEngine;
using System.Collections;

public class loadScripts : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.AddComponent <PlayerController>();
		gameObject.AddComponent<shoot> ();
	}

}
