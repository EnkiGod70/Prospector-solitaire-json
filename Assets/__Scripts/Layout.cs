using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JSONMultiplier
{
    public float x;
    public float y;
}

[System.Serializable]
public class JSONSlot
{
    public int id;
    public float x;
    public float y;
    public bool faceUp = false;
    public string layer = "Default";
    public float xStagger; // For draw pile stagger
    public int[] hiddenBy; // Array of integers for IDs of cards blocking this slot
}

[System.Serializable]
public class JSONSlotDef
{
    public JSONMultiplier multiplier;
    public JSONSlot[] slots;
    public JSONSlot drawPile;
    public JSONSlot discardPile;
}

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID;
    public int id;
    public float xStagger; // Optional for stagger
    public List<int> hiddenBy; // List of IDs of cards blocking this slot
}

public class Layout : MonoBehaviour
{
    [Header("Set Dynamically")]
    public JSONSlotDef jsonr;
    public Vector2 multiplier;
    public List<SlotDef> slotDefs;
    public SlotDef drawPile;
    public SlotDef discardPile;

    public void ReadLayout(string jsonText)
    {
        jsonr = JsonUtility.FromJson<JSONSlotDef>(jsonText);

        // Multiplier
        multiplier.x = jsonr.multiplier.x;
        multiplier.y = jsonr.multiplier.y;

        // Parse slots
        slotDefs = new List<SlotDef>();
        foreach (JSONSlot slot in jsonr.slots)
        {
            SlotDef tSD = new SlotDef
            {
                x = slot.x,
                y = slot.y,
                faceUp = slot.faceUp,
                layerName = slot.layer,
                id = slot.id,
                hiddenBy = new List<int>(slot.hiddenBy) // Convert array to List<int>
            };
            slotDefs.Add(tSD);
        }

        // Parse drawPile
        drawPile = new SlotDef
        {
            x = jsonr.drawPile.x,
            y = jsonr.drawPile.y,
            layerName = jsonr.drawPile.layer,
            xStagger = jsonr.drawPile.xStagger
        };

        // Parse discardPile
        discardPile = new SlotDef
        {
            x = jsonr.discardPile.x,
            y = jsonr.discardPile.y,
            layerName = jsonr.discardPile.layer
        };
    }
}
