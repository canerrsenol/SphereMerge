using System;

// Provides the current level and commands for loading levels.
public interface ILevelService
{
    int CurrentLevelIndex { get; }
    event Action<int> LevelLoaded;
    // Reloads the active level.
    void LoadCurrentLevel();
    // Advances to and loads the next level.
    void LoadNextLevel();
}
