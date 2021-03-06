﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Activity_e {
	inactive,
	drinking,
	dancing,
	talking,
	alert
}

//public enum Facing_e{
//	up,
//	down,
//	left,
//	right
//}



// heavily inspired by prof. Gibson's Boids example
public class Agent : MonoBehaviour {
	static public List<Agent> agents; // master list of Agents

	public List<Agent> neighbors; // all nearby agents
	public List<Agent> collisionRisks; // all agents that are too close
	public Agent closest; // the closest agent

	public Vector3 velocity;
	public Vector3 newVelocity;
	public Vector3 newPosition;

	public bool moving;

	public Activity_e currentActivity;
	public Activity_e priorityActivity;
	public Facing_e facing;


	public Drinkable drinkSource;
	public Drink drink;


	public Transform danceFloor;
	// dancing inclination is multiplied by song rating to determine
	// if it is time to boogie
	public float dancingInclination;

	//NavMeshAgent navAgent;
	//BoxCollider collide;
	Animator anim;

	float changedAnim = -1000f;

	public float defaultVel = 0.5f;
	public float alertVel;

	public float nearDist = 5f;
	public float collisionDist = 0.5f;

	public float raycastDist;

	public float moveDuration;
	public float waitDuration;

	void Awake(){
		if (agents == null) {
			agents = new List<Agent>();		
		}
		// add this agent to the master list
		agents.Add (this);

		neighbors = new List<Agent> ();
		collisionRisks = new List<Agent> ();

		//navAgent = GetComponent<NavMeshAgent> ();
		//collide = GetComponent<BoxCollider> ();
		anim = GetComponent<Animator> ();

		velocity = Random.onUnitSphere * defaultVel;
		velocity.z = 0f;

		moving = false;


	}

	// Use this for initialization
	void Start () {
		currentActivity = Activity_e.inactive;
		StartCoroutine (StartDelay ());
	}

	IEnumerator StartDelay(){
		yield return new WaitForSeconds (Random.Range (1f, 4f)); // to async movement schedule of agents
		StartCoroutine (Move ());
	}

	IEnumerator Move(){
		moving = true;
		float startTime = Time.timeSinceLevelLoad;
		while (Time.timeSinceLevelLoad - startTime < moveDuration) {
			neighbors = GetNeighbors (this);
			
			newVelocity = velocity;
			newPosition = this.transform.position;
			
			// flocking behavior (could be parameterized)
			Vector3 neighborCenterOffset = GetAveragePosition (neighbors) - this.transform.position;
			newVelocity += neighborCenterOffset * 0.05f;
			
			// collision avoidance 
			Vector3 dist;
			if (collisionRisks.Count > 0) {
				Vector3 collisionAveragePos = GetAveragePosition(collisionRisks);
				dist = collisionAveragePos - this.transform.position;
				newVelocity += dist * -0.5f; // collision avoidance amount
			}
			if (currentActivity == Activity_e.alert){
				dist = Player.S.transform.position - this.transform.position;
				newVelocity += dist * 0.5f;
			}
			// attraction to "fun" 
			if (priorityActivity == Activity_e.drinking && currentActivity == Activity_e.inactive){
				if (drinkSource == null){
					drinkSource = GetNearestDrinkable(this); 

				}

				if (drinkSource != null){
					dist = drinkSource.transform.position - this.transform.position;
					newVelocity += dist * 0.20f;
				}
			}

			if (priorityActivity == Activity_e.dancing && currentActivity == Activity_e.inactive ||
			    	currentActivity == Activity_e.dancing){
				if (danceFloor == null){
					danceFloor = GameObject.FindGameObjectWithTag("DanceFloor").transform;
				}
				if (danceFloor != null){
					if ((int)Playlist.S.songRatings[Playlist.S.currentTrack] * dancingInclination > 3f){
						dist = danceFloor.position - this.transform.position;
						newVelocity += dist * 0.20f;
					}
					else{
						//priorityActivity = PickNewActivity();
					}
				}
			}
			
			// newVelocity and newPosition ready, but wait until LateUpdate to set		
			yield return null;
		}
		StartCoroutine (Wait ());
	}

	IEnumerator Drinking(){
		currentActivity = Activity_e.drinking;
		float startTime = Time.timeSinceLevelLoad;
		anim.SetBool ("Drinking", true);
		while (drink != null && Time.timeSinceLevelLoad - startTime < drink.potency) {
			yield return null;		
		}
		currentActivity = Activity_e.inactive;
		anim.SetBool ("Drinking", false);
	}

	IEnumerator Dancing(){
		currentActivity = Activity_e.dancing;
		int track = Playlist.S.currentTrack;
		anim.SetBool ("Dancing", true);
		while (track == Playlist.S.currentTrack) {
			yield return null;		
		}
		currentActivity = Activity_e.inactive;
		anim.SetBool ("Dancing", false);

	}

	IEnumerator Wait(){
		moving = false;
		float startTime = Time.timeSinceLevelLoad;
		while (Time.timeSinceLevelLoad - startTime < waitDuration) {
			yield return null;		
		}
		StartCoroutine (Move ());
	}

	Activity_e PickNewActivity(){
		int choice = Random.Range (0, 2);
		if (choice == 0)
			return Activity_e.drinking;
		else if (choice == 1)
			return Activity_e.dancing;
		return Activity_e.talking;
	}

	void FixedUpdate(){
		if ((currentActivity == Activity_e.inactive || currentActivity == Activity_e.alert)
		    	&& Time.timeSinceLevelLoad - changedAnim > 0.3f) {
			changedAnim = Time.timeSinceLevelLoad;
			// update facing
			bool foundPlayer;
			Ray ray = new Ray(transform.position, transform.position);
			ray.origin = transform.position;
			Vector3 direction = transform.position;
			if (Mathf.Abs (velocity.x) > Mathf.Abs (velocity.y)) { // x greater than y
					if (velocity.x > 0) {
						facing = Facing_e.right;
					// update ray for raycast!
						direction.x += 1f;
						
						anim.SetBool("FaceLeft", true);
						anim.SetBool ("FaceUp", false);
						anim.SetBool ("FaceDown", false);
					} else {
						facing = Facing_e.left;
						direction.x -= 1f;
						anim.SetBool ("FaceUp", false);
						anim.SetBool ("FaceDown", false);
						anim.SetBool("FaceLeft", true);
					}
			} else { // y greater than x
					if (velocity.y > 0) {
						facing = Facing_e.up;
						direction.y += 1f;
						anim.SetBool ("FaceUp", true);
						anim.SetBool ("FaceDown", false);
						anim.SetBool("FaceLeft", false);
					} else {
						facing = Facing_e.down;
						direction.y -= 1f;
						anim.SetBool("FaceDown", true);
						anim.SetBool("FaceUp", false);
						anim.SetBool("FaceLeft", false);
					}
			}
			ray.direction = direction;
			foundPlayer = Physics.Raycast(ray, raycastDist, 1 << LayerMask.NameToLayer("Player"));
			if (foundPlayer){
				//print ("Raycast at player");
				if (Player.S.stealing || Player.S.itemInHand){
					Alert ();
				}
			}
			// if any collision-risk agent has an alert, it spreads to this agent.
			if (ContainsAlert(collisionRisks)){
				Alert ();
			}
		}
	}

	void Alert(){
		PartyFoul.S.StartAlert();
		currentActivity = Activity_e.alert;
		defaultVel = alertVel;
		Playlist.S.Alert();
		waitDuration = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (priorityActivity == Activity_e.drinking && drinkSource != null) {
						if ((transform.position - drinkSource.transform.position).magnitude < collisionDist) { // close enough to drinkSource
								// get drink (could be significant later)
								drink = drinkSource.GetDrink ();
								//forget about drink source
								drinkSource = null;
								// coroutine could later be used to have specific messages play.
								StartCoroutine (Drinking ());
						}
		} else if (priorityActivity == Activity_e.dancing && currentActivity == Activity_e.inactive
		           && danceFloor != null) {
				if ((transform.position - danceFloor.transform.position).magnitude < nearDist){
					StartCoroutine("Dancing");
				}	
		}
	}

	void LateUpdate(){
		if (!moving) return;
						
		velocity = (1 - 0.25f) * velocity + (0.25f * newVelocity); // lerp amount could be parameterized

		// make sure velocity is within min and max (though these could be the same for constant vel
		if (velocity.magnitude > defaultVel) {
			velocity = velocity.normalized * defaultVel;
		}
		if (velocity.magnitude < defaultVel) {
			velocity = velocity.normalized * defaultVel;		
		}

		// decide on new position
		newPosition = this.transform.position + velocity * Time.deltaTime;
		// keep everything in the XY plane
		newPosition.z = 0;
		// Look from the old position at the newPosition to orient the model???
		this.transform.position = newPosition;


		if (facing == Facing_e.right && transform.localScale.x > 0)
						FlipX ();
				else if (facing == Facing_e.left && transform.localScale.x < 0)
						FlipX ();
	}

	// returns Agents near enough to ag to be considered neighbors
	public List<Agent> GetNeighbors(Agent ag){
		float closestDist = float.MaxValue;
		Vector3 delta;
		float dist;
		neighbors.Clear ();
		collisionRisks.Clear ();

		foreach (Agent a in agents) {
			if (a == ag) continue;
			delta = a.transform.position - ag.transform.position;
			dist = delta.magnitude;
			if (dist < closestDist){
				closestDist = dist;
				closest = a;
			}
			if (dist < nearDist){
				neighbors.Add (a);
			}
			if (dist < collisionDist){
				collisionRisks.Add (a);
			}
		}
		if (neighbors.Count == 0) {
			neighbors.Add (closest);		
		}
		return neighbors;
	}

	// returns true if any agent in list is in alert state
	public bool ContainsAlert(List<Agent> someAgents){
		foreach (Agent a in someAgents) {
			if (a.currentActivity == Activity_e.alert)
				return true;
		}
		return false;
	}

	public Vector3 GetAveragePosition(List<Agent> someAgents){
		Vector3 sum = Vector3.zero;
		foreach (Agent a in someAgents) {
			sum += a.transform.position;		
		}
		Vector3 center = sum / someAgents.Count;
		return center;
	}

	public Vector3 GetAverageVelocity(List<Agent> someAgents){
		Vector3 sum = Vector3.zero;
		foreach (Agent a in someAgents) {
			sum += a.velocity;		
		}
		Vector3 avg = sum / someAgents.Count;
		return avg;
	}

	public Drinkable GetNearestDrinkable(Agent ag){
		Drinkable drinkSrc = null;
		float nearestDist = float.MaxValue;
		if (Environment.drinkSources.Count == 0)
			return null;

		Vector3 delta;
		float dist;
		foreach (Drinkable dr in Environment.drinkSources) {
			delta = dr.transform.position - ag.transform.position;
			dist = delta.magnitude;
			if (dist < nearestDist){
				nearestDist = dist;
				drinkSrc = dr;
			}
		}
		return drinkSrc;
	}

	void OnTriggerStay(Collider coll){
		if (coll.tag == "Player") {
			if (Player.S.stealing || Player.S.itemInHand){
				Alert();
			}		
		}
		if (coll.tag == "BoundaryUp") {
			velocity.y -= defaultVel;
			
		} else if (coll.tag == "BoundaryDown") {
			velocity.y += defaultVel;
			
		} else if (coll.tag == "BoundaryLeft") {
			velocity.x += defaultVel;
			
		} else if (coll.tag == "BoundaryRight") {
			velocity.x -= defaultVel;
			
		}
	}

	void OnTriggerExit(Collider coll){
		if (coll.tag == "DanceFloor" && currentActivity == Activity_e.dancing) {
			currentActivity = Activity_e.inactive;
			anim.SetBool("Dancing", false);
			StopCoroutine("Dancing");
		}
	}

	public void FlipX(){
		Vector3 scale = transform.localScale;
		scale.x *= -1f;
		transform.localScale = scale;
	}

}
