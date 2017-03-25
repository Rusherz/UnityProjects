using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {

	public Dictionary<string, List<Inventory>> inventory;

	public InventoryManager(){
		inventory = new Dictionary<string, List<Inventory>> ();
	}

	void CleanupInventory(Inventory inv){
		if (inv.stackSize == 0) {
			if (inventory.ContainsKey (inv.objectType)) {
				inventory [inv.objectType].Remove (inv);
			}
			if (inv.tile != null) {
				inv.tile.inventory = null;
				inv.tile = null;
			}
			if (inv.character != null) {
				inv.character.inventory = null;
				inv.character = null;
			}
		}
	}

	public bool PlaceInventory(Tile tile, Inventory inv){

		bool tileWasEmpty = tile.inventory == null;

		if(tile.PlaceInvemtory (inv) == false){
			Debug.LogError ("Could not place inventory item.");
			return false;
		}

		CleanupInventory (inv);

		if (tileWasEmpty) {
			if(!inventory.ContainsKey(tile.inventory.objectType)){
				inventory [tile.inventory.objectType] = new List<Inventory> ();
			}
			inventory [tile.inventory.objectType].Add (tile.inventory);
		}
		return true;
	}

	public bool PlaceInventory(Job job, Inventory inv){

		if (!job.InventoryReq.ContainsKey (inv.objectType)) {
			Debug.LogError ("Trying to add inventory to a job that it doesnt want.");
			return false;
		}

		job.InventoryReq [inv.objectType].stackSize += inv.stackSize;

		if (job.InventoryReq [inv.objectType].maxStackSize < job.InventoryReq [inv.objectType].stackSize) {
			inv.stackSize = job.InventoryReq [inv.objectType].stackSize - job.InventoryReq [inv.objectType].maxStackSize;
			job.InventoryReq [inv.objectType].stackSize = job.InventoryReq [inv.objectType].maxStackSize;
		} else {
			inv.stackSize = 0;
		}

		CleanupInventory (inv);

		return true;
	}

	public bool PlaceInventory(Character character, Inventory inv, int amount = -1){
		if (amount < 0) {
			amount = inv.stackSize;
		} else {
			amount = Mathf.Min (amount, inv.stackSize);
		}
		if (character.inventory == null) {
			Debug.Log ("adding inventory");
			character.inventory =  inv.Clone();
			character.inventory.stackSize = 0;
			inventory[character.inventory.objectType].Add (character.inventory);
		} else if(character.inventory.objectType != inv.objectType) {
			Debug.LogError ("None matching inv types");
			return false;
		}
		character.inventory.stackSize += amount;
		if (character.inventory.maxStackSize < character.inventory.stackSize) {
			inv.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
			character.inventory.stackSize = character.inventory.maxStackSize;
		} else {
			inv.stackSize -= amount;
		}

		CleanupInventory (inv);

		return true;
	}

	public Inventory GetClosestInventoryOfType(string type, Tile t, int amount){
		if (!inventory.ContainsKey (type)) {
			Debug.LogError ("Trying to add inventory to a job that it doesnt want.");
			return null;
		}
		foreach (Inventory inv in inventory[type]) {
			if (inv.tile != null) {
				return inv;
			}
		}

		return null;
	}

}
