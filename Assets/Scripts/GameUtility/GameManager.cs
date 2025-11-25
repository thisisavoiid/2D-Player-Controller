using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ---------------------------------------------------------
    //  FIELDS & SINGLETON
    // ---------------------------------------------------------

    /// <summary>
    /// Index of the currently loaded scene.
    /// Updated whenever a new scene is loaded.
    /// </summary>
    int _currentSceneIndex = 0;

    /// <summary>
    /// Global GameManager instance accessible from anywhere.
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Prefab reference for the player object.
    /// </summary>
    [SerializeField] public GameObject _playerPrefab;

    /// <summary>
    /// Utility objects to instantiate in each scene (optional).
    /// </summary>
    [SerializeField] public List<GameObject> _sceneGameUtilityObjects = new List<GameObject>();


    // ---------------------------------------------------------
    //  INITIALIZATION
    // ---------------------------------------------------------

    /// <summary>
    /// Establishes the singleton instance and ensures the manager persists between scenes.
    /// </summary>
    private void Awake()
    {
        // Destroy duplicate instances
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("test");

        // Keep the manager across scene loads
        DontDestroyOnLoad(gameObject);
    }


    // ---------------------------------------------------------
    //  SCENE UTILITY INSTANTIATION
    // ---------------------------------------------------------

    /// <summary>
    /// Instantiates all utility objects for the current scene.
    /// </summary>
    private void InstantiateGameManagers()
    {
        foreach (GameObject obj in _sceneGameUtilityObjects)
        {
            try
            {
                // Instantiate object inside the target scene
                Instantiate(obj, SceneManager.GetSceneByBuildIndex(_currentSceneIndex));
                Debug.Log($"Instance of {obj.name} has been created -");
            }
            catch
            {
                Debug.LogError($"Couldn't create instance of {obj.name} -");
            }
        }
    }


    // ---------------------------------------------------------
    //  SCENE LOADING
    // ---------------------------------------------------------

    /// <summary>
    /// Loads the next scene in the build index.
    /// Increments internal scene index and performs optional initialization.
    /// </summary>
    public void LoadNextScene()
    {
        try
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
            _currentSceneIndex++;

            // InstantiateGameManagers();   // intentionally unused

            Debug.Log($"Scene {_currentSceneIndex} ({SceneManager.GetSceneByBuildIndex(_currentSceneIndex).name}) loaded -");
        }
        catch (ArgumentException)
        {
            Debug.LogError("A non-existing scene index has been queued for loading -");
            return;
        }
        catch
        {
            Debug.LogError("Scene could not be loaded -");
            return;
        }
    }

    /// <summary>
    /// Loads a scene by a specific index.
    /// Updates internal scene index after load completes.
    /// </summary>
    public void LoadScene(int sceneToLoadIndex)
    {
        try
        {
            SceneManager.LoadScene(sceneToLoadIndex, LoadSceneMode.Single);

            // InstantiateGameManagers();   // intentionally unused

            Debug.Log($"Scene {_currentSceneIndex} ({SceneManager.GetSceneByBuildIndex(_currentSceneIndex).name}) loaded -");
        }
        catch (ArgumentException)
        {
            Debug.LogError("A non-existing scene index has been queued for loading -");
            return;
        }
        catch
        {
            Debug.LogError("Scene could not be loaded -");
            return;
        }

        // Update internal index to match newly loaded scene
        _currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }


    // ---------------------------------------------------------
    //  SCENE ANCHOR QUERIES
    // ---------------------------------------------------------

    /// <summary>
    /// Returns the spawn anchor position ("PlayerSpawnPosition") inside the current scene's Level object.
    /// </summary>
    public Vector3 GetLevelSpawnAnchorPosition()
    {
        try
        {
            GameObject[] gameObjectsInCurrentScene =
                GetAllGameObjectsInScene(SceneManager.GetActiveScene().buildIndex);

            // Look for root-level "Level" object
            GameObject levelObjectInCurrentScene =
                gameObjectsInCurrentScene.Where(obj => obj.name == "Level").First();

            // Find child transform containing spawn marker
            Transform playerSpawnerAnchor =
                levelObjectInCurrentScene.transform.Find("PlayerSpawnPosition");

            Vector3 playerSpawnerAnchorPosition = playerSpawnerAnchor.transform.position;

            Debug.Log($"Spawn anchor of level {SceneManager.GetActiveScene().buildIndex} ({SceneManager.GetActiveScene().name}) was located at {playerSpawnerAnchorPosition} -");
            return playerSpawnerAnchorPosition;
        }
        catch
        {
            // Fallback in case of missing scene structure
            Debug.LogError("Couldn't locate any spawn anchors in the current scene: Output vector has been defaulted to the zero vector -");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns the level end anchor position ("PlayerFinishPosition") of the current scene.
    /// </summary>
    public Vector3 GetLevelEndAnchorPosition()
    {
        try
        {
            GameObject[] gameObjectsInCurrentScene =
                GetAllGameObjectsInScene(SceneManager.GetActiveScene().buildIndex);

            GameObject levelObjectInCurrentScene =
                gameObjectsInCurrentScene.Where(obj => obj.name == "Level").First();

            Transform playerFinishAnchor =
                levelObjectInCurrentScene.transform.Find("PlayerFinishPosition");

            Vector3 playerFinishAnchorPosition = playerFinishAnchor.transform.position;

            Debug.Log($"Level end anchor of level {SceneManager.GetActiveScene().buildIndex} ({SceneManager.GetActiveScene().name}) was located at {playerFinishAnchorPosition} -");
            return playerFinishAnchorPosition;
        }
        catch
        {
            Debug.LogError("Couldn't locate any end anchors in the current scene: Output vector has been defaulted to the zero vector -");
            return Vector3.zero;
        }
    }


    // ---------------------------------------------------------
    //  UTILITY
    // ---------------------------------------------------------

    /// <summary>
    /// Returns all GameObjects that exist in the active scene.
    /// </summary>
    public GameObject[] GetAllGameObjectsInScene(int sceneIndex)
    {
        GameObject[] objects;

        try
        {
            // Get all objects in the scene
            objects = FindObjectsOfType<GameObject>().ToArray();
            Debug.Log("All game objects in the current scene have been fetched -");
        }
        catch
        {
            // Fallback if fetching fails
            objects = new GameObject[0];
            Debug.LogError("Fetching game objects raised an exception: Output array has been defaulted to an empty array -");
        }

        return objects;
    }
}
