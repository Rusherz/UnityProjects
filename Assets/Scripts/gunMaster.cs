using UnityEngine;
using System.Collections;

public class gunMaster : MonoBehaviour {

	private ArrayList guns;

	// Use this for initialization
	void Start () {
		guns = new ArrayList ();
		addGuns ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	// Custom methods start here
	private void addGuns(){
		gunClass ak47 = new gunClass ("AK-47", 850, 44, 1000);
	}
}
