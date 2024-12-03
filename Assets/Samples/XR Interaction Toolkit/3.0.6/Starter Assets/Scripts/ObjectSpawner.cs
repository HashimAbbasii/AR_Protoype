using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
        Camera m_CameraToFace;

        public Camera cameraToFace
        {
            get
            {
                EnsureFacingCamera();
                return m_CameraToFace;
            }
            set => m_CameraToFace = value;
        }

        [SerializeField]
        [Tooltip("The list of prefabs available to spawn.")]
        List<GameObject> m_ObjectPrefabs = new List<GameObject>();

        public List<GameObject> objectPrefabs
        {
            get => m_ObjectPrefabs;
            set => m_ObjectPrefabs = value;
        }

        [SerializeField]
        [Tooltip("Optional prefab to spawn for each spawned object.")]
        GameObject m_SpawnVisualizationPrefab;

        public GameObject spawnVisualizationPrefab
        {
            get => m_SpawnVisualizationPrefab;
            set => m_SpawnVisualizationPrefab = value;
        }

        [SerializeField]
        [Tooltip("The index of the prefab to spawn. If outside the range of the list, this behavior will select a random object each time it spawns.")]
        int m_SpawnOptionIndex = -1;

        public int spawnOptionIndex
        {
            get => m_SpawnOptionIndex;
            set => m_SpawnOptionIndex = value;
        }

        public bool isSpawnOptionRandomized => m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count;

        [SerializeField]
        [Tooltip("Whether to only spawn an object if the spawn point is within view of the camera.")]
        bool m_OnlySpawnInView = true;

        public bool onlySpawnInView
        {
            get => m_OnlySpawnInView;
            set => m_OnlySpawnInView = value;
        }

        [SerializeField]
        [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
        float m_ViewportPeriphery = 0.15f;

        public float viewportPeriphery
        {
            get => m_ViewportPeriphery;
            set => m_ViewportPeriphery = value;
        }

        [SerializeField]
        [Tooltip("Whether to spawn each object as a child of this object.")]
        bool m_SpawnAsChildren;

        public bool spawnAsChildren
        {
            get => m_SpawnAsChildren;
            set => m_SpawnAsChildren = value;
        }

        private GameObject currentSpawnedObject; // Tracks the currently spawned object
        private bool isSpawnEnabled = false;    // Tracks whether spawning is enabled

        public event Action<GameObject> objectSpawned;

        void Awake()
        {
            EnsureFacingCamera();
        }

        void EnsureFacingCamera()
        {
            if (m_CameraToFace == null)
                m_CameraToFace = Camera.main;
        }

        /// <summary>
        /// Enables spawning for a new object. Resets the current spawned object if one exists.
        /// </summary>
        public void EnableSpawning()
        {
            isSpawnEnabled = true; // Allow spawning
            Debug.Log("Spawning enabled. Tap to spawn an object.");
        }

        /// <summary>
        /// Tries to spawn the selected object at the given position and normal.
        /// </summary>
        public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
        {
            // Ensure that spawning is enabled
            if (!isSpawnEnabled)
            {
                Debug.LogWarning("Spawning is not enabled.");
                return false;
            }

            // Select the object index to spawn (random or selected)
            var objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;

            // Instantiate the selected object
            GameObject newObject = Instantiate(m_ObjectPrefabs[objectIndex]);

            // Position and rotate the object based on spawn location and camera
            newObject.transform.position = spawnPoint;
            EnsureFacingCamera();

            var facePosition = m_CameraToFace.transform.position;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);

            // Optional: Show spawn visualization if set
            if (m_SpawnVisualizationPrefab != null)
            {
                var visualizationTrans = Instantiate(m_SpawnVisualizationPrefab).transform;
                visualizationTrans.position = spawnPoint;
                visualizationTrans.rotation = newObject.transform.rotation;
            }

            // Trigger object spawned event
            objectSpawned?.Invoke(newObject);

            // Allow spawning of the next object on the next tap
            isSpawnEnabled = false;

            Debug.Log("Object spawned successfully.");
            return true;
        }

        /// <summary>
        /// Sets the object index to be spawned when tapped.
        /// </summary>
        public void SetObjectToSpawn(int objectIndex)
        {
            if (objectIndex >= 0 && objectIndex < m_ObjectPrefabs.Count)
            {
                m_SpawnOptionIndex = objectIndex; // Update the selected object index
                EnableSpawning(); // Enable spawning for the selected object
            }
            else
            {
                Debug.LogWarning("Invalid object index.");
            }
        }
    }
}
