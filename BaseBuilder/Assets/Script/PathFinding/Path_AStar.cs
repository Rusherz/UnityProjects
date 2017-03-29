using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Priority_Queue;

public class Path_AStar {

	Queue<Tile> path;

	public Path_AStar(World world, Tile tileStart, Tile tileEnd, string objectType=null, int desiredAmount=0, bool canTakeFromStockpile=false){
		

		if (world.tileGraph == null) {
			world.tileGraph = new Path_TileGraph (world);
		}

		Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;
		if (!nodes.ContainsKey (tileStart)) {
			Debug.Log ("Not in list of nodes");
			return;
		}
		Path_Node<Tile> start = nodes[tileStart];
		Path_Node<Tile> end = null;

		if (tileEnd != null) {
			if (!nodes.ContainsKey (tileEnd)) {
				Debug.Log ("Not in list of nodes");
				return;
			}
			end = nodes [tileEnd];
		}

		List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>> ();

		SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>> ();
		OpenSet.Enqueue (start, 0);

		Dictionary<Path_Node<Tile>, Path_Node<Tile>> came_from = new Dictionary<Path_Node<Tile>, Path_Node<Tile>> ();
		Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float> ();
		Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float> ();

		foreach (Path_Node<Tile> n  in nodes.Values) {
			g_score [n] = Mathf.Infinity;
			f_score [n] = Mathf.Infinity;
		}

		g_score [start] = 0;
		f_score [start] = cost_estimate(start, end);

		while (OpenSet.Count > 0) {
			Path_Node<Tile> current = OpenSet.Dequeue ();

			if (end != null && current == end) {
				reconstruct_path (came_from, current);
				return;
			} else {
				if(end == null && current.data.inventory != null 
					&& current.data.inventory.objectType == objectType){
					if (canTakeFromStockpile || current.data.furniture == null || current.data.furniture.IsStockPile () == false) {
						reconstruct_path (came_from, current);
						return;
					}
				}
			}

			ClosedSet.Add (current);

			foreach (Path_Edge<Tile> edge_neighbour in current.edges) {
				Path_Node<Tile> neighbour = edge_neighbour.node;
				if (ClosedSet.Contains (neighbour)) {
					continue;
				}

				float movementCost = neighbour.data.movementCost * dist_between(current, neighbour);

				float tentative_g_score = g_score [current] + movementCost;

				if (OpenSet.Contains (neighbour) && tentative_g_score >= g_score [neighbour]) {
					continue;
				}

				came_from [neighbour] = current;
				g_score [neighbour] = tentative_g_score;
				f_score [neighbour] = g_score [neighbour] + cost_estimate (neighbour, end);

				if (!OpenSet.Contains (neighbour)) {
					OpenSet.Enqueue (neighbour, f_score [neighbour]);
				} else {
					OpenSet.UpdatePriority (neighbour, f_score [neighbour]);
				}

			}
		}
	}

	float cost_estimate(Path_Node<Tile> start, Path_Node<Tile> end){
		if (end == null) {
			return 0;
		}
		return Mathf.Sqrt (Mathf.Pow (start.data.X - end.data.X, 2) + Mathf.Pow (start.data.Y - end.data.Y, 2));
	}

	float dist_between(Path_Node<Tile> start, Path_Node<Tile> end){
		if (Mathf.Abs (start.data.X - end.data.X) + Mathf.Abs (start.data.Y - end.data.Y) == 1) {
			return 1;
		}
		if(Mathf.Abs(start.data.X - end.data.X) == 1 && Mathf.Abs(start.data.Y - end.data.Y) == 1){
			return 1.41421356237f;
		}
		return Mathf.Sqrt (Mathf.Pow (start.data.X - end.data.X, 2) + Mathf.Pow (start.data.Y - end.data.Y, 2));
	}

	void reconstruct_path(Dictionary<Path_Node<Tile>, Path_Node<Tile>> came_from, Path_Node<Tile> current){
		Queue<Tile> total_path = new Queue<Tile> ();
		total_path.Enqueue (current.data);
		while(came_from.ContainsKey(current)){
			current = came_from [current];
			total_path.Enqueue (current.data);
		}

		path = new Queue<Tile>(total_path.Reverse());
	}

	public Tile DequeueNextTile(){
		return path.Dequeue ();
	}

	public int Length(){
		if (path == null) {
			return 0;
		}

		return path.Count;
	}

	public Tile EndTile(){
		if (path == null || path.Count == 0) {
			return null;
		}
		return path.Last ();
	}

}
