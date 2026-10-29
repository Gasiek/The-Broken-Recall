using UnityEngine;

[CreateAssetMenu(fileName = "AttackVFX", menuName = "Combat/Attack VFX")]
public class AttackVFXDefinition : ScriptableObject
{
    [Header("Particle Effect")]
    [SerializeField]
    private ParticleSystem prefab;

    [Header("Player-Relative Spawn")]
    [SerializeField]
    private Vector3 localPosition;

    [SerializeField]
    private Vector3 localRotation;

    [SerializeField, Min(0.01f)]
    private Vector3 scale = Vector3.one;

    public ParticleSystem Prefab => prefab;
    public Vector3 LocalPosition => localPosition;
    public Vector3 LocalRotation => localRotation;
    public Vector3 Scale => scale;
}
