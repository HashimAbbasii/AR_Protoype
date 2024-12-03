using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARInputHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ObjectSpawner responsible for spawning objects.")]
    private ObjectSpawner objectSpawner;

    [SerializeField]
    [Tooltip("The ARRaycastManager to detect taps on AR planes.")]
    private ARRaycastManager arRaycastManager;

    private Camera mainCamera;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Start()
    {
        mainCamera = Camera.main;

        if (objectSpawner == null)
            Debug.LogError("ObjectSpawner is not assigned! Please assign it in the inspector.");

        if (arRaycastManager == null)
            Debug.LogError("ARRaycastManager is not assigned! Please assign it in the inspector.");
    }

    private void Update()
    {
        // Check for a tap
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Detect where the user tapped
            var touchPosition = Input.GetTouch(0).position;

            // Raycast to detect AR planes
            if (arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinBounds))
            {
                // Get the point where the tap hit the AR plane
                var hitPose = hits[0].pose;

                // Try spawning the object at the detected point
                if (objectSpawner != null)
                {
                    objectSpawner.TrySpawnObject(hitPose.position, hitPose.rotation * Vector3.up);
                }
            }
        }
    }
}
