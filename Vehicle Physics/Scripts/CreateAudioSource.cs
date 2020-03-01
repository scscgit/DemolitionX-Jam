using UnityEngine;
using System.Collections;

///<summary>
///Class that Creates an audio source
///</summary>
public class CreateAudioSource : MonoBehaviour {


	//This may be useful in multiple parts of game..... So I just seperated from the main CarController
	///<summary>
	///The function that creates an Audio source
	///</summary>
	///<param name="go">Parent of the audio source</param>
	///<param name="audioName">Name of the audio source</param>
	///<param name="minDistance">Minimum distance of the audio source</param>
	///<param name="maxDistance">Maximum distance of the audio source</param>
	///<param name="volume">Volume of the audio source</param>
	///<param name="audioClip">Audio Clip needed to be assigned to the source</param>
	///<param name="loop">Must the clip be looped?</param>
	///<param name="playNow">Must the clip be played now?</param>
	///<param name="destroyAfterFinished">Must the clip be destroyed after playing?</param>
	public static AudioSource NewAudioSource(GameObject go, string audioName, float minDistance, float maxDistance, float volume, AudioClip audioClip, bool loop, bool playNow, bool destroyAfterFinished){
		
		GameObject audioSourceObject = new GameObject(audioName);
		audioSourceObject.AddComponent<AudioSource>();
		AudioSource source = audioSourceObject.GetComponent<AudioSource> ();

		source.transform.position = go.transform.position;
		source.transform.rotation = go.transform.rotation;
		source.transform.parent = go.transform;

		//audioSource.GetComponent<AudioSource>().priority =1;
		source.minDistance = minDistance;
		source.maxDistance = maxDistance;
		source.volume = volume;
		source.clip = audioClip;
		source.loop = loop;
		source.dopplerLevel = .5f;

		if(minDistance == 0 && maxDistance == 0)
			source.spatialBlend = 0f;
		else
			source.spatialBlend = 1f;

		if (playNow) {
			source.playOnAwake = true;
			source.Play ();
		} else {
			source.playOnAwake = false;
		}

		if(destroyAfterFinished){
			if(audioClip)
				Destroy(audioSourceObject, audioClip.length);
			else
				Destroy(audioSourceObject);
		}

		if (go.transform.Find ("All Audio Sources")) {
			audioSourceObject.transform.SetParent (go.transform.Find ("All Audio Sources"));
		} else {
			GameObject allAudioSources = new GameObject ("All Audio Sources");
			allAudioSources.transform.SetParent (go.transform, false);
			audioSourceObject.transform.SetParent (allAudioSources.transform, false);
		}

		return source;

	}

	///<summary>
	///Creates a New Low Pass Filter for the given audio source
	///</summary>
	///<param name="source">Audio Source for which the high pass filter needs to be created</param>
	///<param name="freq">Cut off frequency</param>
	///<param name="level">High Pass Resonance level</param>
	public static void NewHighPassFilter(AudioSource source, float freq, int level){

		if(source == null)
			return;

		AudioHighPassFilter highFilter = source.gameObject.AddComponent<AudioHighPassFilter>();
		highFilter.cutoffFrequency = freq;
		highFilter.highpassResonanceQ = level;

	}

}
