using UnityEngine;

public class CommonNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string[] interactionTexts;

    public bool CanInteract(Player player)
    {
        return player != null && interactionTexts != null && interactionTexts.Length > 0;
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player))
            return;

        string text = interactionTexts[Random.Range(0, interactionTexts.Length)];

        Debug.Log($"{name}: {text}");
    }
}
