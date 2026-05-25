using VContainer;
using VContainer.Unity;

// Registers the services and UI components used in the game scene.
public class GameSceneLifetimeScope : LifetimeScope
{
    // Adds scene dependencies to the VContainer builder.
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<GameManager>()
            .As<IGameStateService>();

        builder.RegisterComponentInHierarchy<LevelManager>()
            .As<ILevelService>();

        builder.RegisterComponentInHierarchy<UIManager>();
        builder.RegisterComponentInHierarchy<UILevelText>();
    }
}
