using UnityEngine;
using System.Collections;

public class CameraCentricARManager : MonoBehaviour 
{
	public GameObject[] rootObjects;
	public bool fullscreen = false;
	public float alignment = 0.5f;
	public bool reorientBranding = true;
	float[] lastSpottedTimes;
	
	StringWrapper stringWrapper;
	
	void Awake() 
	{
		// Initialize String
		stringWrapper = new StringWrapper(null, camera, reorientBranding, fullscreen, alignment);

		// Load some image targets
		for (uint i = 0; i < rootObjects.Length; i++)
		{
			stringWrapper.LoadImageMarker("Marker " + (i + 1), "png");
		}

		// Hide all rootObjects
		// Also, set them as children of the camera;
		// This is to more easily position them relative to the camera later
		for (uint i = 0; i < rootObjects.Length; i++)
		{
			rootObjects[i].SetActiveRecursively(false);
			rootObjects[i].transform.parent = transform;
		}
		
		// Allocate array to track the last time each marker was spotted
		lastSpottedTimes = new float[rootObjects.Length];
		
		// Prevent the iOS keyboard from introducing an unwanted
		// black frame when rotating
		iPhoneKeyboard.autorotateToLandscapeLeft = false;
		iPhoneKeyboard.autorotateToLandscapeRight = false;
		iPhoneKeyboard.autorotateToPortrait = false;
		iPhoneKeyboard.autorotateToPortraitUpsideDown = false;
	}
	
	void Update() 
	{
		// Perform marker image tracking
		uint markerCount = stringWrapper.Update();
		
		// Handle detected markers
		for (uint i = 0; i < markerCount; i++)
		{
			// Fetch tracker data for this marker
			StringWrapper.MarkerInfo markerInfo = stringWrapper.GetDetectedMarkerInfo(i);
			
			if (markerInfo.imageID < rootObjects.Length && rootObjects[markerInfo.imageID] != null)
			{
				rootObjects[markerInfo.imageID].transform.localPosition = markerInfo.position;
				rootObjects[markerInfo.imageID].transform.localRotation = markerInfo.rotation;
				lastSpottedTimes[markerInfo.imageID] = Time.time;
				
				if (!rootObjects[markerInfo.imageID].active)
				{
					rootObjects[markerInfo.imageID].SetActiveRecursively(true);
				}
			}
		}
		
		// Deactivate rootObjects for lost markers
		for (uint i = 0; i < rootObjects.Length; i++)
		{
			if (rootObjects[i].active)
			{
				if (Time.time - lastSpottedTimes[i] > 2)
				{
					// Marker has been out of view for a while; Deactivate it
					rootObjects[i].SetActiveRecursively(false);
				}
				else if (Time.time != lastSpottedTimes[i])
				{
					// Marker wasn't spotted this frame; Hide it
					rootObjects[i].transform.localPosition = new Vector3(1000000, 0, 0);
				}
			}
		}
	}
}
