using System;

public interface ILevelService
{
    int CurrentLevelIndex { get; }
    event Action<int> LevelLoaded;
    void LoadCurrentLevel();
    void LoadNextLevel();
}
