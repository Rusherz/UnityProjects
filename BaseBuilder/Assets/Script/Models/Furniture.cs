using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public class Furniture : IXmlSerializable{

	public List<Job> jobs;
	protected Dictionary<string, float> furnParam;
	protected Action<Furniture, float> updateActions;

	public Func<Furniture, Enterability> IsEnterable;

    public void Update(float deltaTime)
    {
		if (updateActions != null) {
			updateActions (this, deltaTime);
		}
    }

	public float GetParam(string s, float def = 0){
		if (furnParam.ContainsKey (s) == false) {
			return def;
		}
		return furnParam [s];
	}

	public void SetParam(string s, float def){
		furnParam [s] = def;
	}

	public void ChangeParam(string s, float def){
		if (furnParam.ContainsKey (s) == false) {
			furnParam [s] = def;
		}
		furnParam [s] += def;
	}

	public void RegisterAction(Action<Furniture, float> a){
		updateActions += a;
	}
	public void UnregisterAction(Action<Furniture, float> a){
		updateActions -= a;
	}

	public Tile tile {
		get; protected set;
	}

	public string objectType {
		get; protected set;
	}

	public float movementCost{ get; protected set; }

	public bool roomEnclosure{ get; protected set; }

	public Color32 tint = Color.white;

	public int width;
	public int height;

	public bool linksToNeighbour{
		get; protected set;
	}

	public Action<Furniture> cbOnChanged;
	public Action<Furniture> cbOnRemoved;

	Func<Tile, bool> funcPositionValidation;


	public Furniture() {
		furnParam = new Dictionary<string, float>();
		jobs = new List<Job> ();
	}

    public Furniture(Furniture other)
    {
		this.objectType = other.objectType;
		this.movementCost = other.movementCost;
		this.roomEnclosure = other.roomEnclosure;
        this.width = other.width;
        this.height = other.height;
		this.tint = other.tint;
        this.linksToNeighbour = other.linksToNeighbour;

		this.furnParam = new Dictionary<string, float>(other.furnParam);

		jobs = new List<Job> ();

		if (other.updateActions != null) {
			this.updateActions = (Action<Furniture, float>)other.updateActions.Clone ();
		}
		if (other.funcPositionValidation != null) {
			this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone ();
		}
		this.IsEnterable = other.IsEnterable;
    }

	virtual public Furniture Clone(){
		return new Furniture(this);
	}

	public Furniture( string objectType, float movementCost = 1f, int width=1, int height=1, bool linksToNeighbour=false, bool roomEnclosure = false) {
        this.objectType = objectType;
        this.movementCost = movementCost;
		this.roomEnclosure = roomEnclosure;
        this.width = width;
        this.height = height;
        this.linksToNeighbour = linksToNeighbour;
		this.furnParam = new Dictionary<string, float>();

        this.funcPositionValidation = this.IsValidPosition;
    }

	static public Furniture PlaceInstance( Furniture proto, Tile tile ) {
		if( proto.funcPositionValidation(tile) == false ) {
			Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
			return null;
		}

		Furniture obj = proto.Clone();
        obj.tile = tile;

        if ( tile.PlaceFurniture(obj) == false ) {
			return null;
		}

		if(obj.linksToNeighbour) {

			Tile t;
			int x = tile.X;
			int y = tile.Y;

			t = tile.world.GetTileAt(x, y+1);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}
			t = tile.world.GetTileAt(x+1, y);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}
			t = tile.world.GetTileAt(x, y-1);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}
			t = tile.world.GetTileAt(x-1, y);
			if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
				t.furniture.cbOnChanged(t.furniture);
			}

		}

		return obj;
	}

	public void RegisterOnChangedCallback(Action<Furniture> callbackFunc) {
		cbOnChanged += callbackFunc;
	}

	public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc) {
		cbOnChanged -= callbackFunc;
	}

	public void RegisterOnRemovedCallback(Action<Furniture> callbackFunc) {
		cbOnRemoved += callbackFunc;
	}

	public void UnregisterOnRemovedCallback(Action<Furniture> callbackFunc) {
		cbOnRemoved -= callbackFunc;
	}

	public bool IsValidPlacement(Tile t){
		return funcPositionValidation (t);
	}

	public bool IsValidPosition(Tile t) {
		for (int x_off = t.X; x_off < (t.X + width); x_off++) {
			for (int y_off = t.Y; y_off < (t.Y + height); y_off++) {
				Tile t2 = t.world.GetTileAt (x_off, y_off);
				if( t2.Type != TileType.Floor ) {
					return false;
				}

				if( t2.furniture != null ) {
					return false;
				}
			}
		}
		return true;
	}

	public bool IsValidPosition_Door(Tile t) {
		if(IsValidPosition(t) == false)
			return false;

		return true;
	}

	public int JobCount(){
		return jobs.Count;
	}

	public void AddJob(Job j){
		jobs.Add (j);
		tile.world.jobQueue.Enqueue (j);
	}

	public void RemoveJob(Job j){
		jobs.Remove (j);
		j.CancelJob ();
		tile.world.jobQueue.Remove (j);
	}

	public void ClearJobs(){
		foreach (Job j in jobs) {
			RemoveJob (j);
		}
	}

	public bool IsStockPile(){
		return objectType == "Stock Pile";
	}

	public void Deconstruct(){
		Debug.Log ("Deconstructing");
		tile.UnplaceFurniture ();

		tile.world.furniture.Remove (this);

		if (cbOnRemoved != null) {
			cbOnRemoved (this);
		}

		if (roomEnclosure) {
			Room.DoRoomFloodFill (this.tile);
		}

		tile.world.InvalidateTileGraph ();

	}



	/*
	 * 
	 * 
	 * SAVE AND LOADING
	 * 
	 * 
	 * 
	 */

	public XmlSchema GetSchema(){
		return null;
	}

	public void WriteXml(XmlWriter writer){
		writer.WriteAttributeString ("X", tile.X.ToString());
		writer.WriteAttributeString ("Y", tile.Y.ToString());
		writer.WriteAttributeString ("ObjectType", objectType);
		//writer.WriteAttributeString ("MovementCost", movementCost.ToString ());
		foreach (string s in furnParam.Keys) {
			writer.WriteStartElement ("Param");
			writer.WriteAttributeString ("Name", s);
			writer.WriteAttributeString ("value", furnParam [s].ToString ());
			writer.WriteEndElement ();
		}
	}

	public void ReadXml(XmlReader reader){
		//movementCost = int.Parse(reader.GetAttribute ("MovementCost"));
		if(reader.ReadToDescendant("Param")){
			do{
				string s = reader.GetAttribute("Name");
				float v = float.Parse(reader.GetAttribute("value"));
				furnParam[s] = v;
			}while(reader.ReadToNextSibling("Param"));
		}
	}
}