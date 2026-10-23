using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningVfx : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField]
    private Transform start;

    [SerializeField]
    private Transform end;

    [Header("Shape")]
    [SerializeField, Min(2)]
    private int points = 8;

    [SerializeField, Min(0f)]
    private float jitter = 0.1f;

    [SerializeField, Min(0.01f)]
    private float refreshRate = 0.05f;

    private LineRenderer lineRenderer;
    private float timer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Local space keeps the bolt glued to the weapon between refreshes.
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = points;
    }

    private void OnEnable()
    {
        timer = 0f;
        Regenerate();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < refreshRate)
            return;

        timer = 0f;
        Regenerate();
    }

    private void Regenerate()
    {
        Vector3 startPosition = transform.InverseTransformPoint(start.position);
        Vector3 endPosition = transform.InverseTransformPoint(end.position);

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            Vector3 point = Vector3.Lerp(startPosition, endPosition, t);

            if (i != 0 && i != points - 1)
                point += Random.insideUnitSphere * jitter;

            lineRenderer.SetPosition(i, point);
        }
    }
}
