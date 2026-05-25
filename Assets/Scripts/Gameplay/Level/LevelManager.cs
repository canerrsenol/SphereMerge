using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

// Creates level instances and tracks the selected level index.
public sealed class LevelManager : MonoBehaviour, ILevelService
{
    private const string CurrentLevelKey = "CurrentLevel";

    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private Transform levelParent;

    private int currentLevelIndex;
    private GameObject currentLevelInstance;
    private LifetimeScope currentLevelScope;
    private LifetimeScope sceneScope;
    private IGameStateService gameStateService;

    public int CurrentLevelIndex => currentLevelIndex;
    public event Action<int> LevelLoaded;

    [Inject]
    // Receives scene services needed to create playable levels.
    public void Construct(LifetimeScope sceneScope, IGameStateService gameStateService)
    {
        this.sceneScope = sceneScope;
        this.gameStateService = gameStateService;
    }

    // Restores the saved level index and prepares a level parent.
    private void Awake()
    {
        if (levelParent == null)
        {
            levelParent = transform;
        }

        currentLevelIndex = PlayerPrefs.GetInt(CurrentLevelKey, 0);
        if (levelPrefabs == null || currentLevelIndex < 0 || currentLevelIndex >= levelPrefabs.Length)
        {
            currentLevelIndex = 0;
        }
    }

    // Loads the first current level after scene setup.
    private void Start()
    {
        LoadCurrentLevel();
    }

    // Cleans up the current level when this manager is destroyed.
    private void OnDestroy()
    {
        DisposeCurrentLevel();
    }

    // Recreates the currently selected level.
    public void LoadCurrentLevel()
    {
        if (!CanLoadLevel())
        {
            return;
        }

        CreateLevel(currentLevelIndex);
    }

    // Saves and creates the next level in the list.
    public void LoadNextLevel()
    {
        if (!CanLoadLevel())
        {
            return;
        }

        currentLevelIndex++;
        if (currentLevelIndex >= levelPrefabs.Length)
        {
            currentLevelIndex = 0;
        }

        PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        PlayerPrefs.Save();

        CreateLevel(currentLevelIndex);
    }

    // Builds a level instance and injects its level dependencies.
    private void CreateLevel(int levelIndex)
    {
        DisposeCurrentLevel();

        GameObject prefab = levelPrefabs[levelIndex];
        if (prefab == null)
        {
            Debug.LogError($"Level prefab at index {levelIndex} is null.", this);
            return;
        }

        currentLevelInstance = Instantiate(prefab, levelParent);
        LevelContext levelContext = currentLevelInstance.GetComponentInChildren<LevelContext>(true);
        if (levelContext == null)
        {
            Debug.LogError($"Level prefab '{prefab.name}' must include a LevelContext component.", this);
            Destroy(currentLevelInstance);
            currentLevelInstance = null;
            return;
        }

        currentLevelScope = sceneScope.CreateChild(builder =>
        {
            levelContext.RegisterDependencies(builder);
        });

        currentLevelScope.Container.InjectGameObject(currentLevelInstance);
        LevelLoaded?.Invoke(currentLevelIndex);
        gameStateService.ChangeState(GameState.Playing);
    }

    // Destroys the level instance and disposes its dependency scope.
    private void DisposeCurrentLevel()
    {
        if (currentLevelScope != null)
        {
            currentLevelScope.Dispose();
            currentLevelScope = null;
        }

        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
            currentLevelInstance = null;
        }
    }

    // Checks that required data and services exist before loading.
    private bool CanLoadLevel()
    {
        if (levelPrefabs == null || levelPrefabs.Length == 0)
        {
            Debug.LogError("Level prefabs list is empty.", this);
            return false;
        }

        if (sceneScope == null)
        {
            Debug.LogError("Scene LifetimeScope is not injected into LevelManager.", this);
            return false;
        }

        if (gameStateService == null)
        {
            Debug.LogError("IGameStateService is not injected into LevelManager.", this);
            return false;
        }

        return true;
    }
}
