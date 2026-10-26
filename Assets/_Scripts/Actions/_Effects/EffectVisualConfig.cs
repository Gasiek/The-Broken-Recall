using UnityEngine;

public enum EffectVisualAttachment
{
    Character,
    Weapon,
}

[System.Serializable]
public class EffectVisualConfig
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private EffectVisualAttachment attachment = EffectVisualAttachment.Character;

    [Header("Local Transform")]
    [SerializeField]
    private Vector3 positionOffset;

    [SerializeField]
    private Vector3 rotationOffset;

    [SerializeField]
    private Vector3 scale = Vector3.one;

    public GameObject Prefab => prefab;
    public EffectVisualAttachment Attachment => attachment;
    public Vector3 PositionOffset => positionOffset;
    public Vector3 RotationOffset => rotationOffset;
    public Vector3 Scale => scale;
}
