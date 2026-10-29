using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SwordSlashVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform swordTip;

    [Header("Trail Settings")]
    [SerializeField, Min(0.05f)]
    private float trailLifetime = 0.18f;

    [SerializeField, Min(0.001f)]
    private float minPointDistance = 0.04f;

    private LineRenderer lineRenderer;
    private readonly List<TrailPoint> points = new();

    private bool isRecording;

    private struct TrailPoint
    {
        public Vector3 position;
        public float time;

        public TrailPoint(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (isRecording && swordTip != null)
        {
            Vector3 position = swordTip.position;

            if (
                points.Count == 0
                || Vector3.Distance(points[^1].position, position) >= minPointDistance
            )
            {
                points.Add(new TrailPoint(position, Time.time));
            }
        }

        RemoveOldPoints();
        UpdateTrail();
    }

    public void Show()
    {
        if (swordTip == null)
        {
            Debug.LogWarning($"{name}: Sword tip is not assigned.", this);
            return;
        }

        points.Clear();
        isRecording = true;
        lineRenderer.enabled = true;

        points.Add(new TrailPoint(swordTip.position, Time.time));
        UpdateTrail();
    }

    public void Hide()
    {
        isRecording = false;
    }

    private void RemoveOldPoints()
    {
        float cutoff = Time.time - trailLifetime;

        while (points.Count > 0 && points[0].time < cutoff)
        {
            points.RemoveAt(0);
        }

        if (!isRecording && points.Count == 0)
        {
            lineRenderer.enabled = false;
        }
    }

    private void UpdateTrail()
    {
        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i].position);
        }
    }

    private void OnDisable()
    {
        isRecording = false;
        points.Clear();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }
}
