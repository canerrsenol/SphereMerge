using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class LevelManager : MonoBehaviour, ILevelService
{
    private const string CurrentLevelKey = "CurrentLevel";

    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private Transform levelParent;

    private int currentLevelIndex;
    private GameObject currentLevelInstance;
    private LifetimeScope currentLevelScope;
    private LifetimeScope sceneScope;

    public int CurrentLevelIndex => currentLevelIndex;
    public event Action<int> LevelLoaded;

    [Inject]
    public void Construct(LifetimeScope sceneScope)
    {
        this.sceneScope = sceneScope;
    }

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

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void OnDestroy()
    {
        DisposeCurrentLevel();
    }

    public void LoadCurrentLevel()
    {
        if (!CanLoadLevel())
        {
            return;
        }

        CreateLevel(currentLevelIndex);
    }

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
    }

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

        return true;
    }
}
