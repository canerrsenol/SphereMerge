using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelContext : MonoBehaviour
{
    public void RegisterDependencies(IContainerBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.RegisterComponent(this).AsSelf().AsImplementedInterfaces();
        RegisterLevelDependencies(builder);
    }

    protected virtual void RegisterLevelDependencies(IContainerBuilder builder)
    {
    }
}
