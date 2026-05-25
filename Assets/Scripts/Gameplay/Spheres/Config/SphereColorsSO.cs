using System;
using UnityEngine;

// Stores editable visual colors for every sphere color type.
[CreateAssetMenu(fileName = "SphereColors", menuName = "Sphere Merge/Sphere Colors")]
public class SphereColorsSO : ScriptableObject
{
    [SerializeField] private SphereColorData[] colors = Array.Empty<SphereColorData>();

    public SphereColorData[] Colors => colors;

    // Gets liquid and glow colors for one sphere color.
    public bool TryGetColors(SphereColors sphereColor, out Color liquidColor, out Color glowColor)
    {
        return TryGetColors(sphereColor, out liquidColor, out glowColor, out _);
    }

    // Gets all visual colors or returns safe default colors.
    public bool TryGetColors(SphereColors sphereColor, out Color liquidColor, out Color glowColor, out Color outlineColor)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            SphereColorData colorData = colors[i];
            if (colorData.SphereColor != sphereColor)
            {
                continue;
            }

            liquidColor = colorData.LiquidColor;
            glowColor = colorData.GlowColor;
            outlineColor = colorData.OutlineColor;
            return true;
        }

        liquidColor = GetDefaultLiquidColor(sphereColor);
        glowColor = GetDefaultGlowColor(sphereColor);
        outlineColor = GetDefaultOutlineColor(sphereColor);
        return false;
    }

    // Gets the liquid color used for one sphere type.
    public Color GetLiquidColor(SphereColors sphereColor)
    {
        return TryGetColors(sphereColor, out Color liquidColor, out _) ? liquidColor : GetDefaultLiquidColor(sphereColor);
    }

    // Gets the outline color used for one sphere type.
    public Color GetOutlineColor(SphereColors sphereColor)
    {
        return TryGetColors(sphereColor, out _, out _, out Color outlineColor) ? outlineColor : GetDefaultOutlineColor(sphereColor);
    }

    // Returns the built-in liquid color for a sphere type.
    public static Color GetDefaultLiquidColor(SphereColors sphereColor)
    {
        switch (sphereColor)
        {
            case SphereColors.Blue:
                return new Color(0.05f, 0.22f, 0.55f, 0.87f);
            case SphereColors.Yellow:
                return new Color(0.55f, 0.38f, 0.05f, 0.87f);
            case SphereColors.Purple:
                return new Color(0.32f, 0.08f, 0.58f, 0.87f);
            case SphereColors.Pink:
                return new Color(0.52f, 0.08f, 0.32f, 0.87f);
            case SphereColors.Brown:
                return new Color(0.35f, 0.18f, 0.07f, 0.87f);
            default:
                return Color.white;
        }
    }

    // Returns the built-in glow color for a sphere type.
    public static Color GetDefaultGlowColor(SphereColors sphereColor)
    {
        switch (sphereColor)
        {
            case SphereColors.Blue:
                return new Color(0.1f, 0.38f, 0.85f, 1f);
            case SphereColors.Yellow:
                return new Color(0.85f, 0.62f, 0.12f, 1f);
            case SphereColors.Purple:
                return new Color(0.55f, 0.18f, 0.85f, 1f);
            case SphereColors.Pink:
                return new Color(0.82f, 0.18f, 0.52f, 1f);
            case SphereColors.Brown:
                return new Color(0.55f, 0.32f, 0.12f, 1f);
            default:
                return Color.white;
        }
    }

    // Returns the built-in outline color for a sphere type.
    public static Color GetDefaultOutlineColor(SphereColors sphereColor)
    {
        switch (sphereColor)
        {
            case SphereColors.Blue:
                return new Color(0.02f, 0.07f, 0.25f, 1f);
            case SphereColors.Yellow:
                return new Color(0.22f, 0.14f, 0.02f, 1f);
            case SphereColors.Purple:
                return new Color(0.12f, 0.03f, 0.25f, 1f);
            case SphereColors.Pink:
                return new Color(0.22f, 0.02f, 0.12f, 1f);
            case SphereColors.Brown:
                return new Color(0.13f, 0.06f, 0.02f, 1f);
            default:
                return Color.black;
        }
    }
}

// Stores the visual colors assigned to one sphere color type.
[Serializable]
public struct SphereColorData
{
    [SerializeField] private SphereColors sphereColor;
    [SerializeField] private Color liquidColor;
    [SerializeField] private Color glowColor;
    [SerializeField] private Color outlineColor;

    public SphereColors SphereColor => sphereColor;
    public Color LiquidColor => liquidColor;
    public Color GlowColor => glowColor;
    public Color OutlineColor => outlineColor;
}
