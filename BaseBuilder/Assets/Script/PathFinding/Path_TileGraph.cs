using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph {

	public Dictionary<Tile, Path_Node<Tile>> nodes;

	public Path_TileGraph(World world){

		nodes = new Dictionary<Tile, Path_Node<Tile>> ();

		foreach (Tile t in world.tiles) {
			//if (t.movementCost > 0) {
				Path_Node<Tile> n = new Path_Node<Tile> ();
				n.data = t;
				nodes.Add (t, n);
			//}
		}

		foreach (Tile t in nodes.Keys) {

			List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>> ();

			Path_Node<Tile> pn = nodes [t];

			Tile[] n = t.GetNeighbours (true);

			for (int i = 0; i < n.Length; i++) {
				if (n [i] != null && n [i].movementCost > 0) {

					if(IsClippingCorner(t, n[i])){
						continue;
					}
					Path_Edge<Tile> pe = new Path_Edge<Tile> ();
					pe.cost = n [i].movementCost;
					pe.node = nodes [n [i]];
					edges.Add (pe);
				}
			}

			pn.edges = edges.ToArray ();

		}

	}

	bool IsClippingCorner(Tile cur, Tile neigh){
		int dx = cur.X - neigh.X;
		int dy = cur.Y - neigh.Y;
		if (Mathf.Abs(dx) + Mathf.Abs(dy) == 2) {

			if(cur.world.GetTileAt(cur.X - dx, cur.Y).movementCost == 0){
				return true;
			}
			if(cur.world.GetTileAt(cur.X, cur.Y - dx).movementCost == 0){
				return true;
			}
		}

		return false;
	}

}
