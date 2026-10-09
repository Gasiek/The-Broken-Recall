using UnityEngine;

public static class VfxSpawner
{
    public static ParticleSystem SpawnOneShot(
    ParticleSystem prefab,
    Vector3 position,
    Quaternion rotation,
    Transform parent = null)
{
    if (prefab == null)
        return null;

    ParticleSystem instance = Object.Instantiate(
        prefab,
        position,
        rotation,
        parent);

    instance.Play();

    float lifetime =
        instance.main.duration +
        instance.main.startLifetime.constantMax;

    Object.Destroy(instance.gameObject, lifetime);

    return instance;
}

    public static ParticleSystem SpawnAttached(
        ParticleSystem prefab,
        Transform parent,
        Vector3 localPosition
    )
    {
        if (prefab == null || parent == null)
            return null;

        ParticleSystem instance = Object.Instantiate(prefab, parent);

        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;

        instance.Play();

        return instance;
    }

    public static void StopAndDestroy(ParticleSystem instance, float delay = 0f)
    {
        if (instance == null)
            return;

        instance.Stop();

        Object.Destroy(instance.gameObject, delay);
    }
}
