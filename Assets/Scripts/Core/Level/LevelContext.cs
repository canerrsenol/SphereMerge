using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelContext : MonoBehaviour
{
    [SerializeField] private SpheresManager spheresManager;

    public void RegisterDependencies(IContainerBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.RegisterComponent(this).AsSelf().AsImplementedInterfaces();
        RegisterSpheresService(builder);
        RegisterLevelDependencies(builder);
    }

    protected virtual void RegisterLevelDependencies(IContainerBuilder builder)
    {
    }

    private void RegisterSpheresService(IContainerBuilder builder)
    {
        if (spheresManager == null)
        {
            spheresManager = GetComponentInChildren<SpheresManager>(true);
        }

        if (spheresManager == null)
        {
            Debug.LogError("LevelContext requires a SpheresManager in the level hierarchy.", this);
            return;
        }

        builder.RegisterComponent(spheresManager)
            .As<ISpheresManagerService>()
            .AsSelf();
    }
}
