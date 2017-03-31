using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

	public GameObject circleCursorPrefab;
	bool IsDragging = false;

	BuildModeController bmc;

	Vector3 lastFramePosition;
	Vector3 currFramePosition;

	Vector3 dragStartPosition;
	List<GameObject> dragPreviewGameObjects;
	FurnitureSpriteController fsc;

	enum MouseMode{
		Select,
		Build
	}

	MouseMode CurMode = MouseMode.Select;

	// Use this for initialization
	void Start () {
		dragPreviewGameObjects = new List<GameObject>();
		bmc = GameObject.FindObjectOfType<BuildModeController> ();
		fsc = GameObject.FindObjectOfType<FurnitureSpriteController> ();
	}

	public Vector3 GetMousePosition(){
		return currFramePosition;
	}

	public Tile GetMouseOverTile(){
		return WorldController.Instance.GetTileAtWorldCoord(currFramePosition);
	}

	// Update is called once per frame
	void Update () {
		currFramePosition = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		currFramePosition.z = 0;

		if (Input.GetKeyUp (KeyCode.Escape)) {
			if (CurMode == MouseMode.Build) {
				CurMode = MouseMode.Select;
			} else if (CurMode == MouseMode.Select) {
				Debug.Log ("Brining up pause menu");
			}
		}

		UpdateSelection ();
		UpdateDragging ();
		UpdateCameraMovement();

		lastFramePosition = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		lastFramePosition.z = 0;
	}

	public class SelectionInfo{
		public Tile tile;
		public ISelectableInterface[] objectArray;
		public int subSelection = 0;
	}

	public SelectionInfo mySelection;

	void UpdateSelection(){

		if (Input.GetKeyUp (KeyCode.Escape)) {
			mySelection = null;
		}

		if (CurMode != MouseMode.Select) {
			return;
		}

		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}

		if (Input.GetMouseButtonUp (0)) {
			Tile tileUnderMouse = GetMouseOverTile ();

			if (tileUnderMouse == null) {
				return;
			}
			if (mySelection == null || mySelection.tile != tileUnderMouse) {			
				mySelection = new SelectionInfo ();
				mySelection.tile = tileUnderMouse;
				RebuildSelectionInfo ();
				for (int i = 0; i < mySelection.objectArray.Length; i++) {
					if (mySelection.objectArray [i] != null) {
						mySelection.subSelection = i;
						break;
					}
				}
			} else {
				RebuildSelectionInfo ();
				for (int i = 0; i < mySelection.objectArray.Length; i++) {
					if (mySelection.objectArray [i] != null && i != mySelection.subSelection) {
						mySelection.subSelection = i;
						break;
					}
				}
			}
		}
	}

	void RebuildSelectionInfo(){
		mySelection.objectArray = new ISelectableInterface[mySelection.tile.characters.Count + 3];
		for (int i = 0; i < mySelection.tile.characters.Count; i++) {
			mySelection.objectArray [i] = mySelection.tile.characters [i];
		}
		mySelection.objectArray [mySelection.objectArray.Length - 3] = mySelection.tile.furniture;
		mySelection.objectArray [mySelection.objectArray.Length - 2] = mySelection.tile.inventory;
		mySelection.objectArray [mySelection.objectArray.Length - 1] = mySelection.tile;

	}

	void UpdateDragging() {
		
		if( EventSystem.current.IsPointerOverGameObject() ) {
			return;
		}
		// Clean up old drag previews
		while(dragPreviewGameObjects.Count > 0) {
			GameObject go = dragPreviewGameObjects[0];
			dragPreviewGameObjects.RemoveAt(0);
			SimplePool.Despawn (go);
		}
		if (CurMode != MouseMode.Build) {
			return;
		}
		// Start Drag
		if (Input.GetMouseButtonDown (0)) {
			dragStartPosition = currFramePosition;
			IsDragging = true;
		} else if (!IsDragging) {
			dragStartPosition = currFramePosition;
		}

		if (Input.GetMouseButtonUp (1) || Input.GetKeyUp(KeyCode.Escape)) {
			IsDragging = false;
		}

		if (!bmc.IsObjectDraggable()) {
			dragStartPosition = currFramePosition;
		}

		int start_x = Mathf.RoundToInt( dragStartPosition.x );
		int end_x =   Mathf.RoundToInt( currFramePosition.x );
		int start_y = Mathf.RoundToInt( dragStartPosition.y );
		int end_y =   Mathf.RoundToInt( currFramePosition.y );
		
		// We may be dragging in the "wrong" direction, so flip things if needed.
		if(end_x < start_x) {
			int tmp = end_x;
			end_x = start_x;
			start_x = tmp;
		}
		if(end_y < start_y) {
			int tmp = end_y;
			end_y = start_y;
			start_y = tmp;
		}

		//if( IsDragging ) {
			// Display a preview of the drag area
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.world.GetTileAt (x, y);
					if(t != null) {
					if (bmc.buildMode == BuildMode.Furniture) {
							ShowFurnitureSpriteAtCord (bmc.buildModeObjectType, t);
						} else {
							// Display the building hint on top of this tile position
							GameObject go = SimplePool.Spawn( circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity );
							go.transform.SetParent(this.transform, true);
							dragPreviewGameObjects.Add(go);
						}

					}
				}
			}
		//}

		if( IsDragging && Input.GetMouseButtonUp(0) ) {
			IsDragging = false;
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.world.GetTileAt (x, y);
					if(t != null) {
						bmc.DoBuild (t);
					}
				}
			}
		}
	}

	void UpdateCameraMovement() {
		if( Input.GetMouseButton(1) || Input.GetMouseButton(2) ) {
			
			Vector3 diff = lastFramePosition - currFramePosition;
			Camera.main.transform.Translate( diff );
			
		}

		Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
	}		

	void ShowFurnitureSpriteAtCord(string furnitureType, Tile t){

		GameObject go = new GameObject ();
		go.transform.SetParent(this.transform, true);
		dragPreviewGameObjects.Add(go);

		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = fsc.GetSpriteForFurniture (furnitureType);
		sr.sortingLayerName = "InstalledObject";
		if (WorldController.Instance.world.IsFurniturePLacementValid (furnitureType, t)) {
			sr.color = new Color (0.5f, 1f, 0.5f, 0.25f);
		} else {
			sr.color = new Color (1f, 0.5f, 0.5f, 0.25f);
		}
		Furniture proto = World.currentWorld.furniturePrototypes [furnitureType];
		go.transform.position = new Vector3 (t.X  + ((proto.height - 1) / 2f), t.Y  + ((proto.height - 1) / 2f), 0);
	}

	public void StartBuildMode(){
		CurMode = MouseMode.Build;
	}

}