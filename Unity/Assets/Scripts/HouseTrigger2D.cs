using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HouseTrigger2D : MonoBehaviour, IInteractable
{
    [Tooltip("Where the POV camera should go (child on the house front).")]
    public Transform doorPOVAnchor;

    bool playerInside;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        var pc = other.GetComponent<PlayerController>();
        pc?.SetInteractable(this);
        GameController.I.ui.ShowApproachPrompt(true, () => Interact(pc));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        var pc = other.GetComponent<PlayerController>();
        pc?.SetInteractable(null);
        GameController.I.ui.ShowApproachPrompt(false, null);
    }

    public void Interact(PlayerController player)
    {
        if (!playerInside) return;
        if (doorPOVAnchor == null) { Debug.LogWarning("No doorPOVAnchor set."); return; }
        GameController.I.ui.ShowApproachPrompt(false, null);
        GameController.I.EnterDoorPOV(doorPOVAnchor);
    }
}
