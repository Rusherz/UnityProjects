using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public class Character : IXmlSerializable {
	public float X {
		get {
			if (nextTile == null)
				return currTile.X;
			return Mathf.Lerp( currTile.X, nextTile.X, movementPercentage );
		}
	}

	public float Y {
		get {
			if (nextTile == null)
				return currTile.Y;
			return Mathf.Lerp( currTile.Y, nextTile.Y, movementPercentage );
		}
	}
	private Tile _currTile;
	public Tile currTile {
		get{
			return _currTile;
		}
		protected set{
			if (_currTile != null) {
				_currTile.characters.Remove (this);
			}
			_currTile = value;
			_currTile.characters.Add (this);
		}
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

	float jobSearchCooldown = 0;

	public Character(Tile tile) {
		currTile = destTile = nextTile = tile;
	}

	void GetNewJob(){
		myJob = World.currentWorld.jobQueue.Dequeue();
		if (myJob == null) {
			return;
		}
		destTile = myJob.tile;
		myJob.RegisterJobCompletedCallBack(OnJobStopped);
		myJob.RegisterJobStoppedCallBack(OnJobStopped);

		pathAStart = new Path_AStar (World.currentWorld, currTile, destTile);
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
			jobSearchCooldown -= deltaTime;
			if (jobSearchCooldown > 0) {
				return;
			}

			GetNewJob();
			if (myJob == null) {
				//Debug.Log ("Searching for a job");
				jobSearchCooldown = UnityEngine.Random.Range (0.5f, 1f);
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
						World.currentWorld.inventoryManager.PlaceInventory (myJob, inventory);
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
					if (World.currentWorld.inventoryManager.PlaceInventory (currTile, inventory) == false) {
						Debug.LogError ("Cannot place inventory in current tile.");
						inventory = null;
					}
				}
			} else {

				if (currTile.inventory != null 
					&& (currTile.furniture == null || !currTile.furniture.IsStockPile() || myJob.canTakeFromStockPile)
					&& myJob.NeededMaterial (currTile.inventory) > 0) {
					World.currentWorld.inventoryManager.PlaceInventory (this, currTile.inventory, myJob.NeededMaterial (currTile.inventory));
				} else {

					Inventory reqNotMet = myJob.GetFirstNeededMaterial ();

					if (currTile != nextTile) {
						// We are still moving dont need to check for a new path yet.
						return;
					}

					if (pathAStart != null && pathAStart.EndTile() != null && pathAStart.EndTile ().inventory != null && pathAStart.EndTile ().inventory.objectType == reqNotMet.objectType) {

					} else {
						
						Path_AStar newPath = World.currentWorld.inventoryManager.GetPathToInventoryOfType (reqNotMet.objectType, currTile, 
							reqNotMet.maxStackSize - reqNotMet.stackSize, myJob.canTakeFromStockPile);
						if (pathAStart == null || pathAStart.Length () == 0) {
							Debug.Log ("No tile contains objects of type " + reqNotMet.objectType);
							AbandonJob ();
							return;
						}
						destTile = newPath.EndTile ();

						pathAStart = newPath;
						// Remove the tile we are already standing on.
						nextTile = pathAStart.DequeueNextTile ();
					}

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
		World.currentWorld.jobQueue.Enqueue (myJob);
		myJob = null;
	}

	void Update_Movement(float deltaTime){
		if (currTile == destTile || myJob == null) {
			pathAStart = null;
			return;
		}

		if (nextTile == null || currTile == nextTile) {
			if (pathAStart == null || pathAStart.Length() == 0) {
				pathAStart = new Path_AStar (World.currentWorld, currTile, destTile);
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

	void OnJobStopped(Job j) {
		// Job completed or was cancelled.

		j.UnregisterJobCStoppedCallBack (OnJobStopped);

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
