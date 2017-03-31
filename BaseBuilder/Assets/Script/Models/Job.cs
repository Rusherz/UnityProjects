using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Job {

	public Tile tile;
	public float jobTime{ get; protected set; }

	public string jobObjectType { get; protected set; }

	public Furniture furnitureprototype;

	public Furniture furniture;

	public bool acceptAny = false;

	protected float jobTimeRequired;

	protected bool jobrepeats = false;

	Action<Job> cbJobCompleted;
	Action<Job> cbJobStopped;
	Action<Job> cbJobWorked;
	public Dictionary<string, Inventory> InventoryReq;

	public bool canTakeFromStockPile = true;

	public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] InventoryReq, bool jobRepeats = false){
		this.tile = tile;
		this.jobObjectType = jobObjectType;
		this.cbJobCompleted += cbJobComplete;
		this.jobTimeRequired = this.jobTime = jobTime;
		this.jobrepeats = jobRepeats;

		this.InventoryReq = new Dictionary<string, Inventory> ();
		if (InventoryReq != null) {
			foreach (Inventory inv in InventoryReq) {
				this.InventoryReq [inv.objectType] = inv.Clone ();

			}
		}

		/*Inventory inv = new Inventory ();
		inv.objectType = "Steel Plate";
		inv.maxStackSize = 5;
		InventoryReq [inv.objectType] = inv;*/

	}

	protected Job(Job other){
		this.tile = other.tile;
		this.jobObjectType = other.jobObjectType;
		this.cbJobCompleted = other.cbJobCompleted;
		this.jobTime = other.jobTime;

		this.InventoryReq = new Dictionary<string, Inventory> ();
		if (InventoryReq != null) {
			foreach (Inventory inv in other.InventoryReq.Values) {
				this.InventoryReq [inv.objectType] = inv.Clone ();
			}
		}
	}

	virtual public Job Clone(){
		return new Job (this);
	}

	public void RegisterJobCompletedCallBack(Action<Job> cb){
		cbJobCompleted += cb;
	}

	public void RegisterJobStoppedCallBack(Action<Job> cb){
		cbJobStopped += cb;
	}

	public void RegisterJobWorkedCallBack(Action<Job> cb){
		cbJobWorked += cb;
	}

	public void UnregisterJobCompletedCallBack(Action<Job> cb){
		cbJobCompleted -= cb;
	}

	public void UnregisterJobCStoppedCallBack(Action<Job> cb){
		cbJobStopped -= cb;
	}

	public void UnregisterJobWorkedCallBack(Action<Job> cb){
		cbJobWorked -= cb;
	}

	public void DoWork(float workTime){
		if (!HasMaterial()) {
			Debug.LogError ("Tried to do job that doesnt have all the material");
			if (cbJobWorked != null) {
				cbJobWorked (this);
			}
			return;
		}
		jobTime -= workTime;

		if (cbJobWorked != null) {
			cbJobWorked (this);
		}

		if(jobTime <= 0) {
			// Do whatever is supposed to happen with a job cycle completes.
			if(cbJobCompleted != null)
				cbJobCompleted(this);

			if(jobrepeats == false) {
				// Let everyone know that the job is officially concluded
				if(cbJobStopped != null)
					cbJobStopped(this);
			}
			else {
				// This is a repeating job and must be reset.
				jobTime += jobTimeRequired;
			}
		}
	}

	public void CancelJob(){
		if (cbJobStopped != null) {
			cbJobStopped (this);
		}
		World.currentWorld.jobQueue.Remove (this);
	}

	public bool HasMaterial(){
		foreach (Inventory inv in InventoryReq.Values) {
			if (inv.maxStackSize > inv.stackSize) {
				return false;
			}
		}
		return true;
	}

	public int NeededMaterial(Inventory inv){
		if (acceptAny) {
			return inv.maxStackSize;
		}

		if (!InventoryReq.ContainsKey (inv.objectType)) {
			return 0;
		}
		if (InventoryReq[inv.objectType].stackSize >= InventoryReq[inv.objectType].maxStackSize) {
			return 0;
		}
		return InventoryReq[inv.objectType].maxStackSize - InventoryReq[inv.objectType].stackSize;
	}

	public Inventory GetFirstNeededMaterial(){
		foreach (Inventory inv in InventoryReq.Values) {
			if (inv.maxStackSize > inv.stackSize) {
				return inv;

			}
		}
		return null;
	}

}
