using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FurnitureActions {

	public static void Door_UpdateAction(Furniture furn, float deltaTime) {
		if(furn.GetParam("is_opening") >= 1) {
			furn.ChangeParam("openess", deltaTime * 2);
			if (furn.GetParam("openess") >= 1.5f) {
				furn.SetParam("is_opening", 0);
			}
		}
		else {
			furn.ChangeParam("openess", deltaTime * -2);
		}

		furn.SetParam("openess", Mathf.Clamp(furn.GetParam("openess"), 0f, 1.5f));

		furn.cbOnChanged (furn);
	}

	public static Enterability Door_IsEnterable(Furniture furn) {
		furn.SetParam("is_opening", 1);

		if(furn.GetParam("openess") >= 1) {
			return Enterability.Yes;
		}

		return Enterability.Soon;
	}

	public static void JobComplete_Building(Job theJob){
		WorldController.Instance.world.PlaceFurniture(theJob.jobObjectType, theJob.tile);
		theJob.tile.JobPending = null;
	}

	public static Inventory[] GetItemsFromFilter(){
		return new Inventory[1] { new Inventory ("Steel Plate", 50, 0) };
	}

	public static void StockPile_UpdateAction(Furniture furn, float deltaTime){

		if(furn.tile.inventory != null && 
			furn.tile.inventory.stackSize >= furn.tile.inventory.maxStackSize){
			furn.ClearJobs ();
			return;
		}

		if (furn.JobCount() > 0) {
			return;
		}

		if (furn.tile.inventory != null && furn.tile.inventory.stackSize == 0) {
			Debug.LogError ("This stack has an inventory of 0 wtf ERROR");
			furn.ClearJobs ();
			return;
		}

		Inventory[] itemsDesired;

		if (furn.tile.inventory == null) {
			itemsDesired = GetItemsFromFilter ();
		}else{
			Inventory desInv = furn.tile.inventory.Clone ();
			desInv.maxStackSize -= desInv.stackSize;
			desInv.stackSize = 0;

			itemsDesired = new Inventory[] { desInv };
		}

		Job j = new Job (
			furn.tile,
			null,
			null,
			0,
			itemsDesired);
		j.canTakeFromStockPile = false;
		j.RegisterJobWorkedCallBack (StockPile_JobWorked);
		furn.AddJob (j);
	}

	static void StockPile_JobWorked(Job j){
		j.tile.furniture.RemoveJob (j);

		foreach(Inventory inv in j.InventoryReq.Values){
			if(inv.stackSize > 0){
				j.tile.world.inventoryManager.PlaceInventory (j.tile, inv);
				return;
			}
		}

	}

	public static void OxygenGenerator_UpdateAction(Furniture furn, float deltaTime){
		if (furn.tile.room.GetGasAmount ("O2") < 0.2f) {
			Debug.Log ("Adding: " + string.Format("{0:0.00}", 0.01f * deltaTime)  + " to air.");
			furn.tile.room.ChangeGas ("O2", 0.01f * deltaTime);
		}
	}

}