using UnityEngine;
using System.Collections;

public class gunClass : MonoBehaviour {

	private string Gun;
	private float Rpm;
	private float Damage;
	private float Range;
	private bool Selected;

	public gunClass(string gun, float rpm, float damage, float range){
		this.Gun = gun;
		this.Rpm = rpm;
		this.Damage = damage;
		this.Range = range;
		this.Selected = false;
	}

	public void setGun(string gun){
		this.Gun = gun;
	}

	public string getGun(){
		return Gun;
	}

	public void setRpm(float rpm){
		this.Rpm = rpm;
	}
	
	public float getRpm(){
		return Rpm;
	}

	public void setDamage(float damage){
		this.Damage = damage;
	}
	
	public float getDamage(){
		return Damage;
	}

	public void setRange(float range){
		this.Range = range;
	}
	
	public float getRange(){
		return Range;
	}

	public void setSelected(bool selected){
		this.Selected = selected;
	}
	
	public bool getSelected(){
		return Selected;
	}

}
