using UnityEngine;
using System.Collections.Generic;

public class MapGen : MonoBehaviour {

	public GameObject roomGFXPrefab;
	public GameObject hallGFXPrefab;
	public GameObject wallEPrefab;
	public GameObject wallEShortPrefab;
	public GameObject wallNPrefab;

	public GameObject floorParent;
	Dictionary<GameObject, GameObject> wallParents;	// Prefab -> Correct Parent
	Dictionary<Material, List<GameObject>> variantObjects;	// Material -> Instance

	List<Room> rooms;

	int numRooms = 10;
	int sizeMin = 2;
	int sizeMax = 10;

	int initSpacing;
	int numSpreadIters = 50;

	const float scale = 8f;

	const int IS_EMPTY = -1;
	const int IS_HALL  = -2;
	Tile[,] tileMap;	// index of the room, or -1 for empty, or -2 for hallway
	int tile_off_x = 0;
	int tile_off_y = 0;
	int tile_size_x = 0;
	int tile_size_y = 0;

	public GameObject devilBookPrefab;

	public static Vector3 playerSpawnPoint;

	//public GameObject victoryObjectPrefab;

	// Use this for initialization
	void Start () {

        rooms = new List<Room>();

		wallParents = new Dictionary<GameObject,GameObject>();

		for (int i = 0; i < numRooms; i++) {
			Room r = new Room( Instantiate(roomGFXPrefab) );
			r.roomGFX.transform.SetParent(floorParent.transform);

            initSpacing = Random.Range(5, 30);
            r.x = Random.Range(-initSpacing, initSpacing);
            initSpacing = Random.Range(5, 30);
            r.y = Random.Range(-initSpacing, initSpacing);

			bool wider = Random.Range(0,2) == 2;
			r.width = Random.Range(sizeMin, sizeMax) - (wider ? 0 : Random.Range(0,2));
			r.height = Random.Range(sizeMin, sizeMax) - (!wider ? 0 : Random.Range(0,2));
			r.UpdateGraphics();

			rooms.Add(r);
		}

		while(numSpreadIters > 0) {
			SpreadRooms();
			numSpreadIters--;
		}

		RemoveOverlap();

		for (int i = 0; i < rooms.Count; i++) {
			rooms[i].index = i;
		}

		// We need to create a connectivity graph
		CreateEdges();

		RoomsToTileMap();

		BuildHallways();

		BuildWalls();

		MeshCombine(floorParent);
		NormalizeUVs(floorParent);

		GameObject variantParent = new GameObject();
		variantParent.transform.SetParent( this.transform );
		variantObjects = new Dictionary<Material, List<GameObject>>();

		foreach(GameObject wallParent in wallParents.Values) {

			//wallParent.GetComponent<MeshRenderer>().sharedMaterial = wallParent.transform.GetChild(0).GetComponentInChildren<MeshRenderer>().sharedMaterial;

			int numWallsPerCombine = 10;

			List<GameObject> subParents = new List<GameObject>();


			while(wallParent.transform.childCount > 0) {
				//Debug.Log (wallParent.transform.GetChild(0).gameObject.name);
				Transform theWallSegment = wallParent.transform.GetChild(0).GetChild(0);

				// First, strip out the variant objects.
				int vi = 0;
				List<Transform> possibleVariants = new List<Transform>();
				while(vi < theWallSegment.childCount) {

					if(theWallSegment.GetChild(vi).name.Contains("Variant")) {
						possibleVariants.Add(theWallSegment.GetChild(vi));
						theWallSegment.GetChild(vi).SetParent(null);
					}
					else {
						vi++;
					}

				}

				// Now, pick one of the possible variants (or none) and add it to variantParents
				int rv = Random.Range(0, possibleVariants.Count + 1);
				if(rv < possibleVariants.Count) {
					if(variantObjects.ContainsKey(possibleVariants[rv].GetComponent<MeshRenderer>().sharedMaterial) == false) {
						variantObjects.Add( possibleVariants[rv].GetComponent<MeshRenderer>().sharedMaterial, new List<GameObject>() );
					}

					variantObjects[possibleVariants[rv].GetComponent<MeshRenderer>().sharedMaterial].Add(possibleVariants[rv].gameObject);
					possibleVariants[rv].SetParent(variantParent.transform);
					possibleVariants.RemoveAt(rv);
				}
				foreach(Transform v in possibleVariants) {
					Destroy(v.gameObject);
				}
				// At this point, all variants are simply standalone, uncombined objects.



				subParents.Add( new GameObject() );
				subParents[subParents.Count-1].layer = 9;
				subParents[subParents.Count-1].AddComponent<MeshFilter>();
				subParents[subParents.Count-1].AddComponent<MeshRenderer>();
				subParents[subParents.Count-1].AddComponent<MeshCollider>();
				subParents[subParents.Count-1].GetComponent<MeshRenderer>().sharedMaterial = theWallSegment.GetComponentInChildren<MeshRenderer>().sharedMaterial;


				for(int c=0; c < numWallsPerCombine; c++) {
					theWallSegment.parent.SetParent(subParents[subParents.Count-1].transform);

					if(wallParent.transform.childCount <= 0)
						break;
				}
			}

			foreach(GameObject sb in subParents) {
				sb.transform.SetParent(wallParent.transform);
				MeshCombine(sb);
			}
		}

		transform.localScale = Vector3.one * scale;

		//FillRooms();

		playerSpawnPoint = rooms[0].Center3d();
        playerSpawnPoint.y += 1.01f;
	}

	void MeshCombine(GameObject go) {
		List<MeshFilter> meshFilters = new List<MeshFilter>(go.GetComponentsInChildren<MeshFilter>());
		meshFilters.RemoveAt(0);
		CombineInstance[] combine = new CombineInstance[meshFilters.Count];
		int j = 0;
		while (j < meshFilters.Count) {
			combine[j].mesh = meshFilters[j].sharedMesh;
			combine[j].transform = meshFilters[j].transform.localToWorldMatrix;
			//meshFilters[j].gameObject.transform.parent.gameObject.SetActive(false);
			j++;
		}
		go.GetComponent<MeshFilter>().mesh = new Mesh();
		go.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
//		Debug.Log (go.name + " num verts: " + go.GetComponent<MeshFilter>().mesh.vertices.Length);
		go.GetComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().mesh;
		go.SetActive(true);
		go.isStatic = true;
		
		while(go.transform.childCount > 0) {
			Transform c = go.transform.GetChild(0);
			c.SetParent(null); // Become Batman
			Destroy (c.gameObject);
		}

	}

	void NormalizeUVs(GameObject go) {
		Mesh mesh = go.GetComponent<MeshFilter>().mesh;

		Vector2[] uvs = new Vector2[mesh.vertices.Length];

		for (int i = 0; i < mesh.uv.Length; i++) {
			//mesh.uv[i] = new Vector2( Truncate(mesh.vertices[i].x), Truncate(mesh.vertices[i].z) );
			//uvs[i] = new Vector2( Truncate(mesh.vertices[i].x), Truncate(mesh.vertices[i].z) );
			uvs[i] = new Vector2( (mesh.vertices[i].x), (mesh.vertices[i].z) );
		}

		mesh.uv = uvs;

		//go.GetComponent<MeshFilter>().mesh = mesh;
	}

	float Truncate(float f) {
		if(f > 0)
			return f - Mathf.Floor(f);

		return f - Mathf.Ceil(f);
	}

	void Update() {
		foreach(Room r1 in rooms) {
			foreach(Room r2 in r1.edges) {
				Debug.DrawLine(r1.Center3d(), r2.Center3d());
			}
		}
	}

/*	void OnGUI() {
		if( GUI.Button( new Rect(0, 0, 100, 25), "Spread")) 
			SpreadRooms();
		if( GUI.Button( new Rect(0, 30, 100, 25), "Remove")) 
			RemoveOverlap();
	}
*/
	void RoomsToTileMap() {
		// Find the min and max x/y
		int min_x = rooms[0].x;
		int max_x = rooms[0].x+rooms[0].width-1;
		int min_y = rooms[0].y;
		int max_y = rooms[0].y+rooms[0].height-1;

		for(int i=0; i < rooms.Count; i++) {
			if(rooms[i].x < min_x)
				min_x = rooms[i].x;
			if(rooms[i].x+rooms[i].width-1 > max_x)
				max_x = rooms[i].x+rooms[i].width-1;
			if(rooms[i].y < min_y)
				min_y = rooms[i].y;
			if(rooms[i].y+rooms[i].height-1 > max_y)
				max_y = rooms[i].y+rooms[i].height-1;
		}

		tile_size_x = max_x - min_x + 1;
		tile_size_y = max_y - min_y + 1 ;

		tileMap = new Tile[ tile_size_x, tile_size_y ];
		tile_off_x = -min_x;
		tile_off_y = -min_y;

		for(int x = 0; x < tile_size_x; x++) {
			for (int y = 0; y < tile_size_y; y++) {
				tileMap[x,y] = new Tile();
			}
		}

		Debug.Log ("Tilemap: " +(max_x - min_x) + " " + (max_y-min_y));

		for(int i=0; i < rooms.Count; i++) {
			for (int x = 0; x < rooms[i].width; x++) {
				for(int y=0; y < rooms[i].height; y++) {
					tileMap[ tile_off_x + rooms[i].x + x, tile_off_y + rooms[i].y + y ].roomIndex = i;

					if(x != 0)
						tileMap[ tile_off_x + rooms[i].x + x, tile_off_y + rooms[i].y + y ].wallW=false;
					if(x != rooms[i].width-1)
						tileMap[ tile_off_x + rooms[i].x + x, tile_off_y + rooms[i].y + y ].wallE=false;
					if(y != 0)
						tileMap[ tile_off_x + rooms[i].x + x, tile_off_y + rooms[i].y + y ].wallS=false;
					if(y != rooms[i].height-1)
						tileMap[ tile_off_x + rooms[i].x + x, tile_off_y + rooms[i].y + y ].wallN=false;
				}
			}
		}
	}

	void BuildWalls() {
		for (int x = 0; x < tile_size_x; x++) {
			for (int y = 0; y < tile_size_y; y++) {
				if(tileMap[x,y].wallN) {
					if( tileMap[x,y].roomIndex==IS_EMPTY
					   && ((y == tile_size_y-1) || tileMap[x,y+1].roomIndex==IS_EMPTY)) {
						// Dont' build wall
					}
					else {
						((GameObject)Instantiate(wallNPrefab, new Vector3(x-tile_off_x, 0, y-tile_off_y), Quaternion.identity)).transform.SetParent(GetWallParent(wallNPrefab));
					}
				}
				if(tileMap[x,y].wallE) {
					if( tileMap[x,y].roomIndex==IS_EMPTY
					   && ((x == tile_size_x-1) || tileMap[x+1,y].roomIndex==IS_EMPTY)) {
						// No wall
					}
					else {
						GameObject theWall = wallEPrefab;

						if(tileMap[x,y].wallN && tileMap[x,y].roomIndex!=IS_EMPTY) {
							theWall = wallEShortPrefab;
						}

						if(y != tile_size_y-1 && tileMap[x,y].roomIndex==IS_EMPTY && tileMap[x,y+1].roomIndex!=IS_EMPTY) {
							theWall = wallEShortPrefab;

						}

						((GameObject)Instantiate(theWall, new Vector3(x-tile_off_x, 0, y-tile_off_y), Quaternion.identity)).transform.SetParent(GetWallParent(theWall));
					}
				}

				if(x==0 && tileMap[x,y].roomIndex != IS_EMPTY) {
					// We need a western wall
					GameObject theWall = wallEPrefab;
					float y_offset = 0;
					if(y == tile_size_y-1 || (tileMap[x,y+1].roomIndex==IS_EMPTY)) {
						theWall = wallEShortPrefab;
						y_offset = -0.25f;
					}

					((GameObject)Instantiate(theWall, new Vector3(x-tile_off_x+1, 0, y-tile_off_y+1  + y_offset), Quaternion.Euler(0, 180f, 0))).transform.SetParent(GetWallParent(wallEPrefab));
				}
				if(y==0 && tileMap[x,y].roomIndex != IS_EMPTY) {
					// We need a south wall
					((GameObject)Instantiate(wallNPrefab, new Vector3(x-tile_off_x+1, 0, y-tile_off_y+1), Quaternion.Euler(0, 180f, 0))).transform.SetParent(GetWallParent(wallNPrefab));
				}
			}
		}
	}

	Transform GetWallParent(GameObject prefab) {
		if(wallParents.ContainsKey(prefab))
			return wallParents[prefab].transform;

		// We need to create a parent for this wall prefab type.

		GameObject go = new GameObject();
		go.name = "Walls-" + prefab.name;
		go.layer = 9;
		go.transform.SetParent(this.transform);
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		go.AddComponent<MeshCollider>();

		wallParents[prefab] = go;
		return go.transform;
	}

	void BuildHallways() {
		for(int i = 0; i < rooms.Count; i++) {
			foreach(Room e in rooms[i].edges) {
				if(e.index < i) 
					continue;

				BuildHallway(
					Mathf.FloorToInt(rooms[i].Center().x) + tile_off_x,
					Mathf.FloorToInt(rooms[i].Center().y) + tile_off_y,
					Mathf.FloorToInt(e.Center().x) + tile_off_x,
					Mathf.FloorToInt(e.Center().y) + tile_off_y
					);
			}
		}
	}

	void BuildHallway(int x1, int y1, int x2, int y2) {
		bool doXFirst = true;

		int dirX = x2 > x1 ? 1 : -1;
		int dirY = y2 > y1 ? 1 : -1;

		while(x1 != x2 || y1 != y2) {
			if( x1==x2 || (doXFirst==false && y1!=y2) ) {
				if(dirY > 0)
					tileMap[x1,y1].wallN=false;
				else
					tileMap[x1,y1].wallS=false;

				y1 += dirY;

				if(dirY > 0)
					tileMap[x1,y1].wallS=false;
				else
					tileMap[x1,y1].wallN=false;
			}
			else {
				if(dirX > 0)
					tileMap[x1,y1].wallE=false;
				else
					tileMap[x1,y1].wallW=false;

				x1 += dirX;

				if(dirX > 0)
					tileMap[x1,y1].wallW=false;
				else
					tileMap[x1,y1].wallE=false;
			}

			if(tileMap[x1,y1].roomIndex == IS_EMPTY) {
				tileMap[x1,y1].roomIndex = IS_HALL;
				GameObject go = (GameObject)Instantiate(hallGFXPrefab, new Vector3(x1-tile_off_x, 0, y1-tile_off_y), Quaternion.identity);
				go.transform.SetParent(floorParent.transform);
			}
		}
	}

	void CreateEdges() {
		for (int i = 0; i < rooms.Count-1; i++) {
			for (int j = i+1; j < rooms.Count; j++) {
				float dist = Vector2.Distance( rooms[i].Center(), rooms[j].Center());

				bool doNotConnect = false;
				for (int k = 0; k < rooms.Count; k++) {
					if(k==i || k==j)
						continue;

					if(  Vector2.Distance(rooms[i].Center(), rooms[k].Center()) < dist && Vector2.Distance(rooms[j].Center(), rooms[k].Center()) < dist ) {
						doNotConnect = true;
						break;
					}
				}

				if(doNotConnect)
					continue;

				rooms[i].edges.Add(rooms[j]);
				rooms[j].edges.Add(rooms[i]);
			}
		}
	}

	void RemoveOverlap() {
		List<Room> deleteMe = new List<Room>();

		for(int i=0; i < rooms.Count-1; i++) {
			for (int j = i+1; j < rooms.Count; j++) {
				if(rooms[i].Overlaps(rooms[j])) {
					deleteMe.Add(rooms[i]);
					break;
				}
			}
		}

		foreach(Room r in deleteMe) {
			r.roomGFX.transform.SetParent(null); // I'M BATMAN
			Destroy(r.roomGFX);
			rooms.Remove(r);
		}
	}

	void SpreadRooms() {
		for(int i=0; i < rooms.Count; i++) {
			for (int j = 0; j < rooms.Count; j++) {
				if(i==j)
					continue;

				if(rooms[i].Overlaps(rooms[j])) {
					Vector2 ci = rooms[i].Center();
					Vector2 cj = rooms[j].Center();

					Vector2 dir = ci-cj;
					if(dir.x > 0)
						rooms[i].x += 1;
					if(dir.y > 0)
						rooms[i].y += 1;
					if(dir.x < 0)
						rooms[i].x += -1;
					if(dir.y < 0)
						rooms[i].y += -1;

					rooms[i].UpdateGraphics();
				}
			}
		}
	}








	class Room {
		public int index=0;

		public int x, y;
		public int width, height;

		public GameObject roomGFX;

		public List<Room> edges;

		public Room(GameObject roomGFX) {
			this.roomGFX = roomGFX;
			edges = new List<Room>();
		}

		public void UpdateGraphics() {
			Vector3 pos = roomGFX.transform.position;
			pos.x = x;
			pos.z = y;
			roomGFX.transform.position = pos;

			Vector3 scale = roomGFX.transform.localScale;
			scale.x = width;
			scale.z = height;
			roomGFX.transform.localScale= scale;
		}

		public bool Overlaps(Room other) {
			if( x < other.x+other.width && x+width > other.x &&
			   	y < other.y+other.height && y+height > other.y
			   ) {
				return true;
			}

			return false;
		}

		public Vector2 Center() {
			return new Vector2((float)x + (float)width/2.0f, (float)y + (float)height/2.0f);
		}

		public Vector3 Center3d() {
			return new Vector3((float)x + (float)width/2.0f, 0, (float)y + (float)height/2.0f) * MapGen.scale;
		}

		public Vector3 Min3d() {
			return new Vector3(x, 0, y) * MapGen.scale;
		}

		public Vector3 Max3d() {
			return new Vector3(x + width, 0, y+height) * MapGen.scale;
			
		}
	}

	/*void FillRooms() {
		allSpawner = new List<Spawner>();
		// Skip room zero, because that's where the player starts.
		for (int i = 1; i < rooms.Count; i++) {
			FillRoom(i);
		}

		// Add victory objects to 5 spawners (even though we only need 3 to win).

		for (int i = 0; i < 5; i++) {
			int s = Random.Range(0, allSpawner.Count);
			allSpawner[s].alsoSpawn = victoryObjectPrefab;
			allSpawner.RemoveAt(s);
		}
	}

	void FillRoom(int index) {
		allSpawner.Add( SimplePool.Spawn( spawnBooks[Random.Range (0, spawnBooks.Length)].prefab,  rooms[index].Center3d(), Quaternion.Euler(0, Random.Range(0,360f), 0), true ).GetComponent<Spawner>() );

		int numBooks = Random.Range(-5, 10);

		for (int i = 0; i < numBooks; i++) {
			Vector3 min = rooms[index].Min3d();
			Vector3 max = rooms[index].Max3d();

			Vector3 pos = new Vector3( Random.Range(min.x+0.25f, max.x-2), 0, Random.Range(min.z+0.25f, max.z-2) );
			pos.y = 0;
			SimplePool.Spawn(devilBookPrefab, pos, Quaternion.Euler(0, Random.Range(0,360), 0), true);
		}
	}*/


	class Tile {
		public int roomIndex=IS_EMPTY;
		public bool wallN = true;
		public bool wallS = true;
		public bool wallE = true;
		public bool wallW = true;
	}
}
