using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventorySpriteController : MonoBehaviour {

	public GameObject stackSizePrefab;

	Dictionary<Inventory, GameObject> inventoryGameObjectMap;

	Dictionary<string, Sprite> inventorySprites;

	World world {
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {
		
		LoadSprites();
		inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();
		world.RegisterInventoryCreated(OnInventoryCreated);
		foreach (string objectType in world.inventoryManager.inventory.Keys) {
			foreach( Inventory inv in world.inventoryManager.inventory[objectType]){
				OnInventoryCreated (inv);
			}
		}
	}

	void LoadSprites() {
		inventorySprites = new Dictionary<string, Sprite>();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Objects/");

		foreach(Sprite s in sprites) {
			inventorySprites[s.name] = s;
			Debug.Log (s);
		}
	}

	public void OnInventoryCreated( Inventory inv ) {
		Debug.Log("OnCharacterCreated");

		GameObject inv_go = new GameObject();

		inventoryGameObjectMap.Add( inv, inv_go );

		inv_go.name = inv.objectType;
		inv_go.transform.position = new Vector3( inv.tile.X, inv.tile.Y, 0);
		inv_go.transform.SetParent(this.transform, true);

		SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
		sr.sprite = inventorySprites[inv.objectType];
		sr.sortingLayerName = "Inventory";

		if (inv.maxStackSize > 1) {
			GameObject ui_go = Instantiate (stackSizePrefab);
			ui_go.transform.SetParent (inv_go.transform);
			ui_go.transform.localPosition = Vector3.zero;
			ui_go.GetComponentInChildren<Text> ().text = inv.stackSize.ToString ();
		}

		inv.RegisterInventoryChangedCallback( OnInventoryChanged );

	}

	void OnInventoryChanged( Inventory inv ) {

		if(inventoryGameObjectMap.ContainsKey(inv) == false) {
			Debug.LogError("OnInventoryChanged -- trying to change visuals for Inventory not in our map.");
			return;
		}

		GameObject inv_go = inventoryGameObjectMap[inv];
		if(inv.stackSize == 0){
			inventoryGameObjectMap.Remove (inv);
			inv.UnregisterInventoryChangedCallback (OnInventoryChanged);
			Destroy (inv_go);
			return;
		}
		Text text = inv_go.GetComponentInChildren<Text> ();
		if (inv.maxStackSize > 1 && text != null) {
			text.text = inv.stackSize.ToString ();
		} 

	}



}
