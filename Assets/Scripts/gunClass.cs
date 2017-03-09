using UnityEngine;
using System.Collections;

public class gunClass {

	private string Gun;
	private float Rpm;
	private float MagSize;
	private float Mags;
	private float Damage;
	private float Range;
	private float Kick;

	public gunClass(string gun, float rpm, float magSize, float mags, float damage, float range, float kick){
		this.Gun = gun;
		this.Rpm = rpm;
		this.MagSize = magSize;
		this.Mags = mags;
		this.Damage = damage;
		this.Range = range;
		this.Kick = kick;
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

	public void setMagSize(float magSize){
		this.MagSize = magSize;
	}
	
	public float getMagSize(){
		return MagSize;
	}

	public void setMags(float mags){
		this.Mags = mags;
	}
	
	public float getMags(){
		return Mags;
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

	public void setKick(float kick){
		this.Kick = kick;
	}
	
	public float getKick(){
		return Kick;
	}

}
