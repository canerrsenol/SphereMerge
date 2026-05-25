using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereObstacleCatalog", menuName = "Sphere Merge/Sphere Obstacle Catalog")]
public sealed class SphereObstacleCatalogSO : ScriptableObject
{
    [SerializeField] private ObstacleBaseAbstract[] obstacles;

    public IReadOnlyList<ObstacleBaseAbstract> Obstacles => obstacles;
}
