using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Job {

	public Tile tile;
	public float jobTime{ get; protected set; }

	public string jobObjectType { get; protected set; }

	public bool acceptAny = false;

	Action<Job> cbJobComplete;
	Action<Job> cbJobCancel;
	Action<Job> cbJobWorked;
	public Dictionary<string, Inventory> InventoryReq;

	public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] InventoryReq){
		this.tile = tile;
		this.jobObjectType = jobObjectType;
		this.cbJobComplete += cbJobComplete;
		this.jobTime = jobTime;

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
		this.cbJobComplete = other.cbJobComplete;
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

	public void RegisterJobCompleteCallBack(Action<Job> cb){
		cbJobComplete += cb;
	}

	public void RegisterJobCancelCallBack(Action<Job> cb){
		cbJobCancel += cb;
	}

	public void RegisterJobWorkedCallBack(Action<Job> cb){
		cbJobWorked += cb;
	}

	public void UnregisterJobCompleteCallBack(Action<Job> cb){
		cbJobComplete -= cb;
	}

	public void UnregisterJobCancelCallBack(Action<Job> cb){
		cbJobCancel -= cb;
	}

	public void UnregisterJobWorkedCallBack(Action<Job> cb){
		cbJobWorked -= cb;
	}

	public void DoWork(float workTime){
		jobTime -= workTime;

		if (cbJobWorked != null) {
			cbJobWorked (this);
		}

		if(jobTime <= 0){
			if(cbJobComplete != null){
				cbJobComplete(this);
			}
		}
	}

	public void CancelJob(){
		if (cbJobCancel != null) {
			cbJobCancel (this);
		}
		tile.world.jobQueue.Remove (this);
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
