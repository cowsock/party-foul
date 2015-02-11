using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SongQuality_e{
	creed,
	dreadful,
	meh,
	p_good,
	amazing
}

public class Playlist : MonoBehaviour {
	public static Playlist S;
	static public List<Agent> musicAppreciators;

	const int numSongs = 3;
	public int currentTrack;

	public AudioClip[] songs = new AudioClip[numSongs];
	public AudioClip alertSong;
	bool alert;
	public SongQuality_e[] songRatings = new SongQuality_e[numSongs];
	public double[] songEndTime = new double[numSongs];

	double startTime;

	void Awake(){
		S = this;
		musicAppreciators = new List<Agent>();
		alert = false;
	}

	// Use this for initialization
	void Start () {
		currentTrack = Random.Range (0, numSongs);
		Camera.main.audio.clip =  songs [currentTrack];
		startTime = AudioSettings.dspTime + 1;
		Camera.main.audio.PlayScheduled (startTime);
		Camera.main.audio.SetScheduledEndTime (startTime + songEndTime [currentTrack]);
		Camera.main.audio.loop = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// check for fade-out
		if (!alert){
			if (AudioSettings.dspTime > startTime + songEndTime [currentTrack] - 10d) {
				if (Camera.main.audio.volume > 0.1){
					Camera.main.audio.volume -= 0.12f * Time.fixedDeltaTime;
				}
			}
			if (AudioSettings.dspTime > startTime + songEndTime[currentTrack]) {
				++currentTrack;
				if (currentTrack >= numSongs){
					currentTrack = 0;
				}
				Camera.main.audio.clip = songs[currentTrack];
				startTime = AudioSettings.dspTime;
				Camera.main.audio.PlayScheduled (startTime + 3);
				Camera.main.audio.volume = 1f;
				Camera.main.audio.SetScheduledEndTime(startTime + songEndTime[currentTrack]);
			}
		}
	}

	void NotifyTrackStatus(SongQuality_e currentSongGoodness){
//		foreach (Agent a in musicAppreciators) {
//			// tell them how good the song is		
//		}
	}

	public void Alert(){
		if (!alert){
			alert = true;
			Camera.main.audio.Stop();
			Camera.main.audio.clip = alertSong;
			Camera.main.audio.Play();
			Camera.main.audio.volume = 1f;
		}
	}
}
