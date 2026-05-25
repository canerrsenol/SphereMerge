using System.Collections.Generic;
using UnityEngine;

// Stores obstacle prefabs available to the level editor.
[CreateAssetMenu(fileName = "SphereObstacleCatalog", menuName = "Sphere Merge/Sphere Obstacle Catalog")]
public sealed class SphereObstacleCatalogSO : ScriptableObject
{
    [SerializeField] private ObstacleBaseAbstract[] obstacles;

    public IReadOnlyList<ObstacleBaseAbstract> Obstacles => obstacles;
}
