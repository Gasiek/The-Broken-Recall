using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealAction", menuName = "RPG/Actions/Heal")]
public class HealAction : ActionDefinition
{
    [SerializeField]
    private int healAmount = 25;

    [SerializeField]
    private AudioClip healSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float healSoundVolume = 0.8f;

    public override IEnumerator Execute(ActionContext context)
    {
        if (healSound != null && context.Player != null)
        {
            GameObject audioObj = new GameObject("HealAudio");
            audioObj.transform.SetParent(context.Player);
            audioObj.transform.localPosition = Vector3.zero;

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = healSound;
            source.volume = healSoundVolume;
            source.spatialBlend = 0f; // 2D so it's always crystal clear
            source.Play();

            Destroy(audioObj, healSound.length);
        }

        if (context.Health != null)
        {
            context.Health.Heal(healAmount);
        }

        yield return new WaitForSeconds(Duration);
    }
}
