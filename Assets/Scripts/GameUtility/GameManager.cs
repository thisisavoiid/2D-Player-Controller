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

    int _currentSceneIndex = 0;
    public static GameManager Instance { get; private set; }
    [SerializeField] public GameObject _playerPrefab;
    [SerializeField] public List<GameObject> _sceneGameUtilityObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("test");
        DontDestroyOnLoad(gameObject);
    }

    private void InstantiateGameManagers() {
        foreach (GameObject obj in _sceneGameUtilityObjects)
        {
            try
            {
                Instantiate(obj, SceneManager.GetSceneByBuildIndex(_currentSceneIndex));
                Debug.Log($"Instance of {obj.name} has been created -");
            }
            catch
            {
                Debug.LogError($"Couldn't create instance of {obj.name} -");
            }
            
        }
    }

    public void LoadNextScene()
    {
        try
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
            _currentSceneIndex++;
             // InstantiateGameManagers();
            
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

    public void LoadScene(int sceneToLoadIndex)
    {
        try
        {
            SceneManager.LoadScene(sceneToLoadIndex, LoadSceneMode.Single);
            // InstantiateGameManagers();
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

        _currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

    }

    public Vector3 GetLevelSpawnAnchorPosition()
    {
        try
        {
            GameObject[] gameObjectsInCurrentScene = GetAllGameObjectsInScene(SceneManager.GetActiveScene().buildIndex);
            GameObject levelObjectInCurrentScene = gameObjectsInCurrentScene.Where(obj => obj.name == "Level").First();
            Transform playerSpawnerAnchor = levelObjectInCurrentScene.transform.Find("PlayerSpawnPosition");
            Vector3 playerSpawnerAnchorPosition = playerSpawnerAnchor.transform.position;

            Debug.Log($"Spawn anchor of level {SceneManager.GetActiveScene().buildIndex} ({SceneManager.GetActiveScene().name}) was located at {playerSpawnerAnchorPosition} -");
            return playerSpawnerAnchorPosition;
        }
        catch
        {
            Debug.LogError("Couldn't locate any spawn anchors in the current scene: Output vector has been defaulted to the zero vector -");
            return Vector3.zero;
        }

    }

    public Vector3 GetLevelEndAnchorPosition()
    {
        try
        {
            GameObject[] gameObjectsInCurrentScene = GetAllGameObjectsInScene(SceneManager.GetActiveScene().buildIndex);
            GameObject levelObjectInCurrentScene = gameObjectsInCurrentScene.Where(obj => obj.name == "Level").First();
            Transform playerFinishAnchor = levelObjectInCurrentScene.transform.Find("PlayerFinishPosition");
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
    public GameObject[] GetAllGameObjectsInScene(int sceneIndex)
    {
        GameObject[] objects;
        try
        {
            objects = FindObjectsOfType<GameObject>().ToArray();
            Debug.Log("All game objects in the current scene have been fetched -");
        }
        catch
        {
            objects = new GameObject[0];
            Debug.LogError("Fetching game objects raised an exception: Output array has been defaulted to an empty array -");
        }

        return objects;

    }
}
