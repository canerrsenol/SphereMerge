using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereColors", menuName = "Sphere Merge/Sphere Colors")]
public class SphereColorsSO : ScriptableObject
{
    [SerializeField] private SphereColorData[] colors = Array.Empty<SphereColorData>();

    public SphereColorData[] Colors => colors;

    public bool TryGetColors(SphereColors sphereColor, out Color liquidColor, out Color glowColor)
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
            return true;
        }

        liquidColor = GetDefaultLiquidColor(sphereColor);
        glowColor = GetDefaultGlowColor(sphereColor);
        return false;
    }

    public Color GetLiquidColor(SphereColors sphereColor)
    {
        return TryGetColors(sphereColor, out Color liquidColor, out _) ? liquidColor : GetDefaultLiquidColor(sphereColor);
    }

    public static Color GetDefaultLiquidColor(SphereColors sphereColor)
    {
        switch (sphereColor)
        {
            case SphereColors.Blue:
                return new Color(0.08f, 0.55f, 1f, 0.87f);
            case SphereColors.Yellow:
                return new Color(1f, 0.78f, 0.12f, 0.87f);
            case SphereColors.Purple:
                return new Color(0.56f, 0.2f, 0.95f, 0.87f);
            case SphereColors.Pink:
                return new Color(1f, 0.22f, 0.66f, 0.87f);
            case SphereColors.Brown:
                return new Color(0.68f, 0.32f, 0.14f, 0.87f);
            default:
                return Color.white;
        }
    }

    public static Color GetDefaultGlowColor(SphereColors sphereColor)
    {
        Color glowColor = GetDefaultLiquidColor(sphereColor);
        glowColor.a = 1f;
        return glowColor;
    }
}

[Serializable]
public struct SphereColorData
{
    [SerializeField] private SphereColors sphereColor;
    [SerializeField] private Color liquidColor;
    [SerializeField] private Color glowColor;

    public SphereColors SphereColor => sphereColor;
    public Color LiquidColor => liquidColor;
    public Color GlowColor => glowColor;
}
