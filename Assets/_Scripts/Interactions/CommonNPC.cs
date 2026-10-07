using UnityEngine;

public class CommonNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string[] interactionTexts;

    public bool CanInteract
    {
        get { return interactionTexts != null && interactionTexts.Length > 0; }
    }

    public void Interact()
    {
        if (!CanInteract)
        {
            return;
        }

        string text = interactionTexts[Random.Range(0, interactionTexts.Length)];

        Debug.Log($"{name}: {text}");
    }
}
