using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public class Character : IXmlSerializable {
	public float X {
		get {
			return Mathf.Lerp( currTile.X, nextTile.X, movementPercentage );
		}
	}

	public float Y {
		get {
			return Mathf.Lerp( currTile.Y, nextTile.Y, movementPercentage );
		}
	}

	public Tile currTile {
		get; protected set;
	}

	public Inventory inventory;

	Tile _destTile;
	Tile destTile {
		get {
			return _destTile;
		}

		set {
			if (_destTile != value) {
				_destTile = value;
				pathAStart = null;
			}
		}
	}
	Tile nextTile;
	Path_AStar pathAStart;
	float movementPercentage;

	float speed = 4f;

	Action<Character> cbCharacterChanged;

	Job myJob;

	public Character(Tile tile) {
		currTile = destTile = nextTile = tile;
	}

	void GetNewJob(){
		myJob = currTile.world.jobQueue.Dequeue();
		if (myJob == null) {
			return;
		}
		destTile = myJob.tile;
		myJob.RegisterJobCompleteCallBack(OnJobEnded);
		myJob.RegisterJobCancelCallBack(OnJobEnded);

		pathAStart = new Path_AStar (currTile.world, currTile, destTile);
		if (pathAStart.Length () == 0) {
			Debug.LogError ("Path to dest does not exist.");
			AbandonJob ();
			destTile = currTile;
			return;
		}
	}

	void Update_DoJob(float deltaTime){
		if(myJob == null) {
			// Grab a new job.
			GetNewJob();
			if (myJob == null) {
				destTile = currTile;
				return;
			}
		}
		// We have a job!

		if (!myJob.HasMaterial()) {

			if (inventory != null) {
				if (myJob.NeededMaterial (inventory) > 0) {
					destTile = myJob.tile;
					if (currTile == myJob.tile) {
						currTile.world.inventoryManager.PlaceInventory (myJob, inventory);
						myJob.DoWork (0);
						if (inventory.stackSize == 0) {
							inventory = null;
						} else {
							Debug.LogError ("Character still has inventory?");
							inventory = null;
							return;
						}
					} else {
						destTile = myJob.tile;
						return;
					}
				} else {
					if (currTile.world.inventoryManager.PlaceInventory (currTile, inventory) == false) {
						Debug.LogError ("Cannot place inventory in current tile.");
						inventory = null;
					}
				}
			} else {

				if (currTile.inventory != null && myJob.NeededMaterial (currTile.inventory) > 0) {
					currTile.world.inventoryManager.PlaceInventory (this, currTile.inventory, myJob.NeededMaterial (currTile.inventory));
				} else {

					Inventory reqNotMet = myJob.GetFirstNeededMaterial ();

					Inventory supply = currTile.world.inventoryManager.GetClosestInventoryOfType (reqNotMet.objectType, currTile, 
						                  reqNotMet.maxStackSize - reqNotMet.stackSize);
					if (supply == null) {
						Debug.Log ("No tile contains objects of type " + reqNotMet.objectType);
						AbandonJob ();
						return;
					}
					destTile = supply.tile;
					return;

				}

			}
			return;
		}
		destTile = myJob.tile;

		// Are we there yet?
		if(currTile == myJob.tile) {
			myJob.DoWork(deltaTime);
		}
	}

	public void AbandonJob(){
		nextTile = destTile = currTile;
		currTile.world.jobQueue.Enqueue (myJob);
		myJob = null;
	}

	void Update_Movement(float deltaTime){
		if (currTile == destTile || myJob == null) {
			pathAStart = null;
			return;
		}

		if (nextTile == null || currTile == nextTile) {
			if (pathAStart == null || pathAStart.Length() == 0) {
				pathAStart = new Path_AStar (currTile.world, currTile, destTile);
				if (pathAStart.Length () == 0) {
					Debug.LogError ("Path to dest does not exist.");
					AbandonJob ();
					return;
				}
				nextTile = pathAStart.DequeueNextTile ();
			}


			nextTile = pathAStart.DequeueNextTile ();

			if (nextTile == currTile) {
				Debug.LogError ("Returned current tile.");
			}
		}

		float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X-nextTile.X, 2) + Mathf.Pow(currTile.Y-nextTile.Y, 2));

		if (nextTile.IsEnterable() == Enterability.Never) {
			Debug.LogError ("Cant pass next tile");
			nextTile = null;
			pathAStart = null;
			return;
		} else if (nextTile.IsEnterable() == Enterability.Soon) {
			return;
		} else {

		}

		float distThisFrame = (speed / nextTile.movementCost) * deltaTime;

		float percThisFrame = distThisFrame / distToTravel;

		movementPercentage += percThisFrame;

		if(movementPercentage >= 1) {
			currTile = nextTile;
			movementPercentage = 0;
		}


	}

	public void Update(float deltaTime) {
		
		Update_DoJob (deltaTime);
		Update_Movement (deltaTime);

		if(cbCharacterChanged != null)
			cbCharacterChanged(this);
		
	}

	public void SetDestination(Tile tile) {
		if(currTile.IsNeighbour(tile, true) == false) {
			Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour.");
		}

		destTile = tile;
	}

	public void RegisterOnChangedCallback(Action<Character> cb) {
		cbCharacterChanged += cb;
	}

	public void UnregisterOnChangedCallback(Action<Character> cb) {
		cbCharacterChanged -= cb;
	}

	void OnJobEnded(Job j) {
		// Job completed or was cancelled.

		if(j != myJob) {
			Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
			return;
		}
		pathAStart = null;
		nextTile = currTile;
		myJob = null;
	}

	/*
	 * 
	 * 
	 * SAVE AND LOADING
	 * 
	 * 
	 * 
	 */
	public Character(){

	}

	public XmlSchema GetSchema(){
		return null;
	}

	public void WriteXml(XmlWriter writer){
		writer.WriteAttributeString ("X", currTile.X.ToString());
		writer.WriteAttributeString ("Y", currTile.Y.ToString());
	}

	public void ReadXml(XmlReader reader){



	}
}
