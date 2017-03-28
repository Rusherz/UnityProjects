using UnityEngine;
using System.Collections.Generic;

public class DTileMap {

    /*protected class DTile {
		bool isWalkable = false;
		int tileGraphicId = 0;
		string name = "Unknown";
	}
	
	List<DTile> tileTypes;
	
	void InitTiles() {
		tileType[1].name = "Floor";
		tileType[1].isWalkable = true;
		tileType[1].tileGraphicId = 1;
		tileType[1].damagePerTurn = 0;
	}*/

    protected class DRoom {
        public int left;
        public int top;
        public int width;
        public int height;

        public bool isConnected = false;

        public int right {
            get { return left + width - 1; }
        }

        public int bottom {
            get { return top + height - 1; }
        }

        public int center_x {
            get { return left + width / 2; }
        }

        public int center_y {
            get { return top + height / 2; }
        }

        public bool CollidesWith(DRoom other) {
            if (left > other.right - 1)
                return false;

            if (top > other.bottom - 1)
                return false;

            if (right < other.left + 1)
                return false;

            if (bottom < other.top + 1)
                return false;

            return true;
        }


    }

    int size_x;
    int size_y;

    public Vector2 playerSpawn;
    bool playerSpawnSet = false;

    TDTile[,] map_data;

    List<DRoom> rooms;

    public DTileMap(int size_x, int size_y, Vector2 playerSpawn) {
        DRoom r;
        this.size_x = size_x;
        this.size_y = size_y;
        this.playerSpawn = playerSpawn;

        map_data = new TDTile[size_x, size_y];

        for (int x = 0; x < size_x; x++) {
            for (int y = 0; y < size_y; y++) {
                TDTile tile = new TDTile(x, y);
                tile.type = TileType.Void;
                map_data[x, y] = tile;
            }
        }

        rooms = new List<DRoom>();

        if (!playerSpawnSet && playerSpawn != null) {
            r = new DRoom();
            r.left = (int)playerSpawn.x;
            r.top = (int)playerSpawn.y;
            r.width = 6;
            r.height = 6;
            rooms.Add(r);
        }

        int maxFails = 10;

        while (rooms.Count < 10) {
            int rsx = Random.Range(4, 14);
            int rsy = Random.Range(4, 10);

            r = new DRoom();
            r.left = Random.Range(1, (size_x - 1) - rsx);
            r.top = Random.Range(1, (size_y - 1) - rsy);
            r.width = rsx;
            r.height = rsy;

            if (!RoomCollides(r)) {
                rooms.Add(r);
            } else {
                maxFails--;
                if (maxFails <= 0)
                    break;
            }

        }

        foreach (DRoom r2 in rooms) {
            MakeRoom(r2);
        }


        for (int i = 0; i < rooms.Count; i++) {
            if (!rooms[i].isConnected) {
                int j = Random.Range(1, rooms.Count);
                MakeCorridor(rooms[i], rooms[(i + j) % rooms.Count]);
            }
        }

        MakeWalls();

    }

    bool RoomCollides(DRoom r) {
        foreach (DRoom r2 in rooms) {
            if (r.CollidesWith(r2)) {
                return true;
            }
        }

        return false;
    }

    public TDTile GetTileAt(int x, int y) {
        return map_data[x, y];
    }

    void MakeRoom(DRoom r) {

        for (int x = 0; x < r.width; x++) {
            for (int y = 0; y < r.height; y++) {
                TDTile tile = map_data[r.left + x, r.top + y];
                TrapType trap = IsTrap();
                Debug.Log(trap);
                if (trap != TrapType.None) {
                    tile.type = TileType.Trap;
                    tile.trap = trap;
                } else {
                    tile.type = TileType.Floor;
                }
                tile.moveSpeed = 1;
            }
        }

    }

    void MakeCorridor(DRoom r1, DRoom r2) {
        int x = r1.center_x;
        int y = r1.center_y;

        while (x != r2.center_x) {
            TDTile tile = map_data[x, y];
            TrapType trap = IsTrap();
            Debug.Log(trap);
            if (trap != TrapType.None) {
                tile.type = TileType.Trap;
                tile.trap = trap;
            } else {
                tile.type = TileType.Floor;
            }
            tile.moveSpeed = 1;
            x += x < r2.center_x ? 1 : -1;
        }

        while (y != r2.center_y) {
            TDTile tile = map_data[x, y];
            TrapType trap = IsTrap();
            Debug.Log(trap);
            if (trap != TrapType.None) {
                tile.type = TileType.Trap;
                tile.trap = trap;
            } else {
                tile.type = TileType.Floor;
            }
            tile.moveSpeed = 1;
            y += y < r2.center_y ? 1 : -1;
        }

        r1.isConnected = true;
        r2.isConnected = true;

    }

    void MakeWalls() {
        for (int x = 0; x < size_x; x++) {
            for (int y = 0; y < size_y; y++) {
                if (map_data[x, y].type == TileType.Void && HasAdjacentFloor(x, y)) {
                    TDTile tile = map_data[x, y];
                    tile.type = TileType.Wall;
                }
            }
        }
    }

    bool HasAdjacentFloor(int x, int y) {
        if (x > 0 && map_data[x - 1, y].type == TileType.Floor)
            return true;
        if (x < size_x - 1 && map_data[x + 1, y].type == TileType.Floor)
            return true;
        if (y > 0 && map_data[x, y - 1].type == TileType.Floor)
            return true;
        if (y < size_y - 1 && map_data[x, y + 1].type == TileType.Floor)
            return true;

        if (x > 0 && y > 0 && map_data[x - 1, y - 1].type == TileType.Floor)
            return true;
        if (x < size_x - 1 && y > 0 && map_data[x + 1, y - 1].type == TileType.Floor)
            return true;

        if (x > 0 && y < size_y - 1 && map_data[x - 1, y + 1].type == TileType.Floor)
            return true;
        if (x < size_x - 1 && y < size_y - 1 && map_data[x + 1, y + 1].type == TileType.Floor)
            return true;


        return false;
    }

    TrapType IsTrap() {
        if (Random.Range(0f, 1f) > 0.01f) {
            return TrapType.None;
        } else {
            return (TrapType)Random.Range(0, 3);
        }
    }
}
