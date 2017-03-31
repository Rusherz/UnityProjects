using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureCreate {
    
    public static void CreateFurniturePrototypes(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes) {
        CreateWallProtoType(furniturePrototypes, furnitureJobPrototypes);
        CreateDoorPrototype(furniturePrototypes, furnitureJobPrototypes);
        //CreateStockPilePrototype(furniturePrototypes, furnitureJobPrototypes);
        CreateOxygenPrototype(furniturePrototypes, furnitureJobPrototypes);
        CreateMiningDroneStation(furniturePrototypes, furnitureJobPrototypes);
		CreateTree (furniturePrototypes, furnitureJobPrototypes);
		CreateStone(furniturePrototypes, furnitureJobPrototypes);
		CreateSteel(furniturePrototypes, furnitureJobPrototypes);
    }

    static void CreateWallProtoType(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes) {
        furniturePrototypes.Add("Wall", new Furniture("Wall", 0, 1, 1, true, true));
        furnitureJobPrototypes.Add("Wall", new Job(null, "Wall", FurnitureActions.JobComplete_Building,
            1f, new Inventory[] { new Inventory("Steel Plate", 5, 0) }));
    }

    static void CreateDoorPrototype(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes) {
        furniturePrototypes.Add("Door", new Furniture("Door", 2, 1, 1, false, true));
        furniturePrototypes["Door"].SetParam("openess", 0);
        furniturePrototypes["Door"].SetParam("is_opening", 0);
        furniturePrototypes["Door"].RegisterAction(FurnitureActions.Door_UpdateAction);
        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;
    }

    static void CreateStockPilePrototype(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes) {
        furniturePrototypes.Add("Stock Pile", new Furniture("Stock Pile", 1, 1, 1, true, false));
        furnitureJobPrototypes.Add("Stock Pile", new Job(null, "Stock Pile", FurnitureActions.JobComplete_Building, -1f, null));
        furniturePrototypes["Stock Pile"].RegisterAction(FurnitureActions.StockPile_UpdateAction);
        furniturePrototypes["Stock Pile"].tint = new Color32(186, 31, 31, 255);
    }

    static void CreateOxygenPrototype(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes) {
        furniturePrototypes.Add("Oxygen Generator", new Furniture("Oxygen Generator", 10, 2, 2, false, false));
        furniturePrototypes["Oxygen Generator"].RegisterAction(FurnitureActions.OxygenGenerator_UpdateAction);
    }

    static void CreateMiningDroneStation(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes) {
        furniturePrototypes.Add("Mining Drone Station", new Furniture("Mining Drone Station", 1, 3, 3, false, false));
        furniturePrototypes["Mining Drone Station"].RegisterAction(FurnitureActions.MiningDroneStation_UpdateAction);
        furniturePrototypes["Mining Drone Station"].jobSpotOffset = new Vector2(1, 0);
        furniturePrototypes["Mining Drone Station"].jobSpawnSpot = new Vector2(0, 0);
    }

	#region World Objects
	static void CreateTree(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes){
		furniturePrototypes.Add("Tree", new Furniture("Tree", 1, 1, 2, false, false, 1));
		furniturePrototypes ["Tree"].RegisterDeconstruct (FurnitureActions.Tree_UpdateAction);
		furniturePrototypes["Tree"].jobSpotOffset = new Vector2(0, -1);
		furniturePrototypes["Tree"].jobSpawnSpot = new Vector2(0, 0);

	}

	static void CreateStone(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes){
		furniturePrototypes.Add("Stone", new Furniture("Stone", 1, 1, 1, false, false, 1));
		furniturePrototypes ["Stone"].RegisterDeconstruct (FurnitureActions.Stone_UpdateAction);
		furniturePrototypes["Stone"].jobSpotOffset = new Vector2(0, -1);
		furniturePrototypes["Stone"].jobSpawnSpot = new Vector2(0, 0);

	}

	static void CreateSteel(Dictionary<string, Furniture> furniturePrototypes, Dictionary<string, Job> furnitureJobPrototypes){
		furniturePrototypes.Add("Steel", new Furniture("Steel", 1, 1, 1, false, false, 1));
		furniturePrototypes ["Steel"].RegisterDeconstruct (FurnitureActions.Steel_UpdateAction);
		furniturePrototypes["Steel"].jobSpotOffset = new Vector2(0, -1);
		furniturePrototypes["Steel"].jobSpawnSpot = new Vector2(0, 0);

	}
	#endregion

}
