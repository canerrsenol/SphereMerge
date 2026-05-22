using VContainer;
using VContainer.Unity;

public class GameSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(this).As<LifetimeScope>();

        builder.RegisterComponentInHierarchy<GameManager>()
            .As<IGameStateService>();

        builder.RegisterComponentInHierarchy<LevelManager>()
            .As<ILevelService>();

        builder.RegisterComponentInHierarchy<UIManager>();
    }
}
