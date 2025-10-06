using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HouseTrigger2D : MonoBehaviour, IInteractable
{
    public Transform doorPOVAnchor;             // the anchor at the door
    public DoorController door;                 // assign door (has HouseCandyAvailability)
    public UIHud ui;                            // assign (or find UIHud.I)

    private bool playerInside;
    private PlayerController currentPlayer;

    void Awake()
    {
        if (!ui) ui = UIHud.I;
        if (!door && doorPOVAnchor) door = doorPOVAnchor.GetComponentInParent<DoorController>();
        if (door && door.house)
            door.house.OnAvailabilityChanged += HandleAvailabilityChanged;
    }

    void OnDestroy()
    {
        if (door && door.house)
            door.house.OnAvailabilityChanged -= HandleAvailabilityChanged;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (!pc) return;

        playerInside = true;
        currentPlayer = pc;
        pc.SetInteractable(this);

        RefreshApproachUI();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (!pc || pc != currentPlayer) return;

        playerInside = false;
        if (ui) ui.ShowApproachPrompt(false, null);
        pc.SetInteractable(null);
        currentPlayer = null;
    }

    void HandleAvailabilityChanged(bool isAvailable)
    {
        if (!playerInside) return;
        RefreshApproachUI();
    }

    void RefreshApproachUI()
    {
        bool available = door && door.house && door.house.IsAvailable;
        if (!ui) ui = UIHud.I;

        if (available)
        {
            ui.ShowApproachPrompt(true, () =>
            {
                // guard: if it flips off in the split second before click
                if (door && door.house && door.house.IsAvailable)
                    GameController.I.EnterDoorPOV(doorPOVAnchor);
                else
                    ui.ShowApproachPrompt(false, null);
            });
        }
        else
        {
            ui.ShowApproachPrompt(false, null);
        }
    }

    // IInteractable
    public void Interact(PlayerController player)
    {
        if (door && door.house && door.house.IsAvailable)
            GameController.I.EnterDoorPOV(doorPOVAnchor);
    }
}
