using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class RadialCooldownElement : VisualElement
{
    private float progress;

    public float Progress
    {
        get => progress;
        set
        {
            float clamped = Mathf.Clamp01(value);

            if (Mathf.Approximately(progress, clamped))
                return;

            progress = clamped;
            MarkDirtyRepaint();
        }
    }

    public RadialCooldownElement()
    {
        pickingMode = PickingMode.Ignore;
        generateVisualContent += DrawCooldown;
    }

    private void DrawCooldown(MeshGenerationContext context)
    {
        if (progress <= 0f)
            return;

        Rect rect = contentRect;

        if (rect.width <= 0f || rect.height <= 0f)
            return;

        Vector2 center = rect.center;

        // Find where a ray from the center intersects the rectangle.
        Vector2 GetEdgePoint(float angleDegrees)
        {
            float angle = angleDegrees * Mathf.Deg2Rad;
            float dx = Mathf.Cos(angle);
            float dy = Mathf.Sin(angle);

            float tx = dx == 0f ? float.PositiveInfinity : (rect.width * 0.5f) / Mathf.Abs(dx);

            float ty = dy == 0f ? float.PositiveInfinity : (rect.height * 0.5f) / Mathf.Abs(dy);

            float distance = Mathf.Min(tx, ty);

            return new Vector2(center.x + dx * distance, center.y + dy * distance);
        }

        var painter = context.painter2D;
        painter.fillColor = new Color(0f, 0f, 0f, 0.7f);

        painter.BeginPath();
        painter.MoveTo(center);
        painter.LineTo(GetEdgePoint(-90f));

        int segments = Mathf.Max(1, Mathf.CeilToInt(180f * progress));

        for (int i = 1; i <= segments; i++)
        {
            float angle = -90f - 360f * progress * i / segments;
            painter.LineTo(GetEdgePoint(angle));
        }

        painter.ClosePath();
        painter.Fill();
    }
}
