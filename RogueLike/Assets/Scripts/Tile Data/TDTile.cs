public enum TileType { Void, Floor, Wall, Trap }
public enum TrapType { Posion, Fire, Slowness, None }
public class TDTile {

    public TileType type = TileType.Void;
    public TrapType trap = TrapType.None;
    public int x { get; protected set; }
    public int y { get; protected set; }

    float _MoveSpeed = 0;
    public float moveSpeed {
        get {
            return _MoveSpeed;
        }
        set {
            _MoveSpeed = value;
        }
    }

    public TDTile(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void TrapEffect() {

    }
    
}
