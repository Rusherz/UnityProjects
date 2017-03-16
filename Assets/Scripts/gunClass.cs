using UnityEngine;
using System.Collections;

public class gunClass {

	private string Gun;
	private float Rpm;
	private int MagSize;
	private int TotalBullets;
	private float Damage;
	private float Range;
	private float Kick;

	public gunClass(string gun, float rpm, int magSize, float damage, float range, float kick){
		this.Gun = gun;
		this.Rpm = rpm;
		this.MagSize = magSize;
		this.TotalBullets = magSize * 6;
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

	public void setMagSize(int magSize){
		this.MagSize = magSize;
	}
	
	public int getMagSize(){
		return MagSize;
	}

	public void setTotalBullets(int totalBullets)
    {
		this.TotalBullets = totalBullets;
	}
	
	public int getTotalBullets(){
		return TotalBullets;
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
