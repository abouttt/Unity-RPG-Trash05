using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class SpritePreviewAttribute : PropertyAttribute
{
    public readonly float Size;

    public SpritePreviewAttribute(float size = 50f)
    {
        Size = size;
    }
}
