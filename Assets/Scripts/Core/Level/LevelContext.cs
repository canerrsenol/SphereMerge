using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelContext : MonoBehaviour
{
    [SerializeField] private SpheresManager spheresManager;
    [SerializeField] private SpheresMergeManager spheresMergeManager;

    public void RegisterDependencies(IContainerBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.RegisterComponent(this).AsSelf().AsImplementedInterfaces();
        builder.RegisterComponent(spheresManager)
            .As<ISpheresManagerService>()
            .AsSelf();
        builder.RegisterComponent(spheresMergeManager)
            .As<ISpheresMergeManagerService>()
            .AsSelf();
        RegisterLevelDependencies(builder);
    }

    protected virtual void RegisterLevelDependencies(IContainerBuilder builder)
    {
    }
}
