
using UnityEngine;

public sealed class ObjectSpin : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 90f;

    private void Update()
    {
        transform.Rotate(
            rotationAxis,
            degreesPerSecond * Time.deltaTime,
            Space.Self
        );
    }
}
