﻿using UnityEngine;
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
	static public List<Agent> musicAppreciators;

	const int numSongs = 4;
	int currentTrack;

	public AudioClip[] songs = new AudioClip[numSongs];
	public SongQuality_e[] songRatings = new SongQuality_e[numSongs];
	public double[] songEndTime = new double[numSongs];

	double startTime;

	void Awake(){
		musicAppreciators = new List<Agent>();
	}

	// Use this for initialization
	void Start () {
		currentTrack = Random.Range (0, numSongs-1);
		Camera.main.audio.clip =  songs [currentTrack];
		startTime = AudioSettings.dspTime + 1;
		Camera.main.audio.PlayScheduled (startTime);
		Camera.main.audio.SetScheduledEndTime (startTime + songEndTime [currentTrack]);
		Camera.main.audio.loop = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// check for fade-out
		if (AudioSettings.dspTime > startTime + songEndTime [currentTrack] - 10d) {
			if (Camera.main.audio.volume > 0.1){
				Camera.main.audio.volume -= 0.05f * Time.fixedDeltaTime;
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

	void NotifyTrackStatus(SongQuality_e currentSongGoodness){
//		foreach (Agent a in musicAppreciators) {
//			// tell them how good the song is		
//		}
	}
}