using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

// Registers dependencies that belong to one level instance.
public class LevelContext : MonoBehaviour
{
    [SerializeField] private SpheresManager spheresManager;
    [SerializeField] private SpheresMergeManager spheresMergeManager;

    // Registers level components in a child dependency scope.
    public void RegisterDependencies(IContainerBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.RegisterComponent(this).AsSelf().AsImplementedInterfaces();
        builder.RegisterComponent(spheresManager)
            .AsSelf();
        builder.RegisterComponent(spheresMergeManager)
            .As<ISpheresMergeManagerService>()
            .AsSelf();
        RegisterLevelDependencies(builder);
    }

    // Lets derived level contexts register extra dependencies.
    protected virtual void RegisterLevelDependencies(IContainerBuilder builder)
    {
    }
}
