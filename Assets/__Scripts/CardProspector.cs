using UnityEngine;
using System.Collections.Generic; // Required for List

// Enum to define the state of a card
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card
{
    [Header("Set Dynamically")]
    public eCardState state; // The current state of the card
    public SlotDef slotDef;  // Slot definition for layout
    public int layoutID;     // ID from the layout
    public List<CardProspector> hiddenBy = new List<CardProspector>(); // Cards blocking this card

    // Override the OnMouseUpAsButton method from the Card class
    public override void OnMouseUpAsButton()
    {
        // Prevent interaction if the card is blocked
        if (hiddenBy.Count > 0)
        {
            Debug.Log($"{name} is blocked by {hiddenBy.Count} card(s).");
            return;
        }

        // Notify the Prospector script about this card being clicked
        Debug.Log($"Card clicked: {name}, State: {state}");
        Prospector.S.CardClicked(this);

        // Optionally call the base class method
        base.OnMouseUpAsButton();
    }
}
