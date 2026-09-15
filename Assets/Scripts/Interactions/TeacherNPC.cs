using UnityEngine;

public class TeacherNPC : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("You can learn a new skill.");
    }
}