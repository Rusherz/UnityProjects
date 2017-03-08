using UnityEngine;
using System.Collections;

public class gunMaster : MonoBehaviour {

	private gunClass[] guns;
	private float switchCoolDown = 1f;
	private float timeWait = 0;

	// Use this for initialization
	void Start () {
		addGuns ();
	}
	
	// Update is called once per frame
	void Update () {
		timeWait -= Time.deltaTime;
		if(timeWait <= 0){
			if(Input.GetKeyDown(KeyCode.Alpha0)){
				guns[0].setSelected(true);
				timeWait = switchCoolDown;
			}else if(Input.GetKeyDown(KeyCode.Alpha1)){
				guns[1].setSelected(true);
				timeWait = switchCoolDown;
			}else if(Input.GetKeyDown(KeyCode.Alpha2)){
				guns[2].setSelected(true);
				timeWait = switchCoolDown;
			}else if(Input.GetKeyDown(KeyCode.Alpha3)){
				guns[3].setSelected(true);
				timeWait = switchCoolDown;
			}
		}
	}

	// Custom methods start here
	private void addGuns(){
		guns = new gunClass[]{
			new gunClass("AK-47", 850, 30, 6, 44, 1000, 10),
			new gunClass("G36C", 780, 30, 6, 38, 1000, 5),
			new gunClass("552 Commando", 690, 30, 6, 47, 1000, 5),
			new gunClass("Vector .45 APC", 1200, 25, 6, 23, 200, 2)};
	}
}
