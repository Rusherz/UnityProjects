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
		return new Inventory[] { new Inventory ("Steel Plate", 50, 0), 
			new Inventory ("Stone Clump", 50, 0), 
			new Inventory ("Wood Plank", 50, 0)  };
	}

	public static void StockPile_UpdateAction(Furniture furn, float deltaTime){

		if(furn.tile.inventory != null && 
			furn.tile.inventory.stackSize >= furn.tile.inventory.maxStackSize){
			furn.CancelJob ();
			return;
		}

		if (furn.JobCount() > 0) {
			return;
		}

		if (furn.tile.inventory != null && furn.tile.inventory.stackSize == 0) {
			Debug.LogError ("This stack has an inventory of 0 wtf ERROR");
			furn.CancelJob ();
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
		j.CancelJob ();

		foreach(Inventory inv in j.InventoryReq.Values){
			if(inv.stackSize > 0){
				World.currentWorld.inventoryManager.PlaceInventory (j.tile, inv);
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

	public static void MiningDroneStation_UpdateAction(Furniture furn, float deltaTime){
		Tile spawnSpot = furn.GetSpawnSpotTile ();
		if (furn.JobCount() > 0) {

			if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
				furn.CancelJob ();
			}

			return;
		}

		if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
			return;
		}

		Tile jobSpot = furn.JobSpotOffset ();

		if (jobSpot.inventory != null && (jobSpot.inventory.stackSize >= jobSpot.inventory.maxStackSize)) {
			return;
		}
		Job j = new Job (jobSpot, null, MiningDroneStation_JobComplete, 1f, null, true);
		furn.AddJob (j);
	}

	public static void MiningDroneStation_JobComplete(Job j){
		
		World.currentWorld.inventoryManager.PlaceInventory (j.furniture.GetSpawnSpotTile(), new Inventory ("Steel Plate", 50, 20));

	}

	public static void Tree_UpdateAction(Furniture furn, float deltaTime){
		Tile spawnSpot = furn.GetSpawnSpotTile ();
		if (furn.JobCount () > 0) {
			if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
				furn.CancelJob ();
			}
			return;
		}
		if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
			return;
		}
		Tile jobSpot = furn.JobSpotOffset ();
		if (jobSpot.inventory != null && (jobSpot.inventory.stackSize >= jobSpot.inventory.maxStackSize)) {
			return;
		}
		Job j = new Job (jobSpot, null, Tree_JobComplete, 1f, null, false);
		furn.AddJob (j);
	}

	public static void Tree_JobComplete(Job j){
		World.currentWorld.inventoryManager.PlaceInventory(j.furniture.GetSpawnSpotTile(), new Inventory("Wood Plank", 64, Random.Range(1, 20)));
		j.furniture.UnregisterDeconstruct (Tree_UpdateAction);
		j.furniture.Deconstruct ();

	}

	public static void Stone_UpdateAction(Furniture furn, float deltaTime){
		Tile spawnSpot = furn.GetSpawnSpotTile ();
		if (furn.JobCount () > 0) {
			if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
				furn.CancelJob ();
			}
			return;
		}
		if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
			return;
		}
		Tile jobSpot = furn.JobSpotOffset ();
		if (jobSpot.inventory != null && (jobSpot.inventory.stackSize >= jobSpot.inventory.maxStackSize)) {
			return;
		}
		Job j = new Job (jobSpot, null, Stone_JobComplete, 1f, null, false);
		furn.AddJob (j);
	}

	public static void Stone_JobComplete(Job j){
		World.currentWorld.inventoryManager.PlaceInventory(j.furniture.GetSpawnSpotTile(), new Inventory("Stone Clump", 64, Random.Range(1, 20)));
		j.furniture.UnregisterDeconstruct (Stone_UpdateAction);
		j.furniture.Deconstruct ();

	}

	public static void Steel_UpdateAction(Furniture furn, float deltaTime){
		Tile spawnSpot = furn.GetSpawnSpotTile ();
		if (furn.JobCount () > 0) {
			if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
				furn.CancelJob ();
			}
			return;
		}
		if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
			return;
		}
		Tile jobSpot = furn.JobSpotOffset ();
		if (jobSpot.inventory != null && (jobSpot.inventory.stackSize >= jobSpot.inventory.maxStackSize)) {
			return;
		}
		Job j = new Job (jobSpot, null, Steel_JobComplete, 1f, null, false);
		furn.AddJob (j);
	}

	public static void Steel_JobComplete(Job j){
		World.currentWorld.inventoryManager.PlaceInventory(j.furniture.GetSpawnSpotTile(), new Inventory("Steel Plate", 64, Random.Range(1, 20)));
		j.furniture.UnregisterDeconstruct (Steel_UpdateAction);
		j.furniture.Deconstruct ();

	}

}