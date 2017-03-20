using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

	public Tile.TileType BuildModeTile = Tile.TileType.Floor;

	public GameObject cursorCirclePrefab;
	Vector3 currFramePos;
	Vector3 lastFramePos;
	Vector3 tileDragStartPos;
	List<GameObject> DragPreview;

	// Use this for initialization
	void Start () {
		DragPreview = new List<GameObject> ();
	}
	
	// Update is called once per frame
	void Update () {
		currFramePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		currFramePos.z = 0;

		//UpdateCursorPos ();
		DragSelect ();
		UpdateCameraMovement ();

		lastFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		lastFramePos.z = 0;

	}

	/*void UpdateCursorPos(){
		// Update mouse circle position
		Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(currFramePos);
		if (tileUnderMouse != null) {
			cursorCircle.SetActive (true);
			Vector3 cursorPosition = new Vector3 (tileUnderMouse.X, tileUnderMouse.Y, 0);
			cursorCircle.transform.position = cursorPosition;
		} else {
			cursorCircle.SetActive (false);
		}
	}*/

	void DragSelect(){
		// If over UI bail
		if(EventSystem.current.IsPointerOverGameObject()){
			return;
		}

		// Start Drag
		if (Input.GetMouseButtonDown (0)) {
			tileDragStartPos = currFramePos;
		}

		int start_x = Mathf.FloorToInt (tileDragStartPos.x);
		int end_x = Mathf.FloorToInt (currFramePos.x);

		if (end_x < start_x) {
			int tmp = end_x;
			end_x = start_x;
			start_x = tmp;
		}

		int start_y = Mathf.FloorToInt (tileDragStartPos.y);
		int end_y = Mathf.FloorToInt (currFramePos.y);

		if (end_y < start_y) {
			int tmp = end_y;
			end_y = start_y;
			start_y = tmp;
		}	

		while (DragPreview.Count > 0) {
			GameObject go = DragPreview [0];
			DragPreview.RemoveAt (0);
			SimplePool.Despawn (go);
		}

		if (Input.GetMouseButton (0)) {
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.world.getTileAt (x, y);
					if (t != null) {
						GameObject go = SimplePool.Spawn (cursorCirclePrefab, new Vector3 (x, y, 0), Quaternion.identity);
						go.transform.SetParent (this.transform, true);
						DragPreview.Add(go);
					}
				}
			}
		}

		// Handle left mouse clicks
		if (Input.GetMouseButtonUp (0)) {
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.world.getTileAt (x, y);
					if (t != null) {
						t.Type = BuildModeTile;
					}
				}
			}
		}
	}

	void UpdateCameraMovement(){
		// Handle Screen Dragging
		if (Input.GetMouseButton (2) || Input.GetMouseButton(1)) {
			Vector3 diff = lastFramePos - currFramePos;
			Camera.main.transform.Translate (diff);
		}

		Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 3, 25);
	}

	public void OnBuildFloorClick(){
		BuildModeTile = Tile.TileType.Floor;
	}

	public void OnBulldozeFloorClick(){
		BuildModeTile = Tile.TileType.Empty;
	}

}
