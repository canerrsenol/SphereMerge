using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereColors", menuName = "Sphere Merge/Sphere Colors")]
public class SphereColorsSO : ScriptableObject
{
    [SerializeField] private Color[] _colors = Array.Empty<Color>();

    public Color[] Colors => _colors;
}
