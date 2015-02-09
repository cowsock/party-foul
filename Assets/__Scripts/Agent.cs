using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Activity_e {
	inactive,
	drinking,
	dancing,
	talking
}

public enum Facing_e{
	up,
	down,
	left,
	right
}



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

	//NavMeshAgent navAgent;
	//BoxCollider collide;
	Animator anim;

	public float defaultVel = 0.5f;

	public float nearDist = 5f;
	public float collisionDist = 0.5f;

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
		float startTime = Time.time;
		while (Time.time - startTime < moveDuration) {
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
			
			// newVelocity and newPosition ready, but wait until LateUpdate to set		
			yield return null;
		}
		StartCoroutine (Wait ());
	}

	IEnumerator Drinking(){
		float startTime = Time.time;
		anim.SetBool ("Drinking", true);
		while (drink != null && Time.time - startTime < drink.potency) {
			yield return null;		
		}
		currentActivity = Activity_e.inactive;
		anim.SetBool ("Drinking", false);
	}

	IEnumerator Wait(){
		moving = false;
		float startTime = Time.time;
		while (Time.time - startTime < waitDuration) {
			yield return null;		
		}
		StartCoroutine (Move ());
	}
	
	// Update is called once per frame
	void Update () {
		if (priorityActivity == Activity_e.drinking && drinkSource != null) {
			if ((transform.position - drinkSource.transform.position).magnitude < collisionDist){ // close enough to drinkSource
				// get drink (could be significant later)
				drink = drinkSource.GetDrink();
				currentActivity = Activity_e.drinking;
				//forget about drink source
				drinkSource = null;
				// coroutine could later be used to have specific messages play.
				StartCoroutine(Drinking ());
			}
		}
	}

	void LateUpdate(){
		if (moving) {
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
		}
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
}
