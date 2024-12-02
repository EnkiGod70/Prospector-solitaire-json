using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public List<GameObject> decoGOs = new List<GameObject>();
    public List<GameObject> pipGOs = new List<GameObject>();
    public GameObject back;
    public CardDefinition def;
    public SpriteRenderer[] spriteRenderers;

    public bool faceUp
    {
        get => !back.activeSelf;
        set => back.SetActive(!value);
    }

    virtual public void OnMouseUpAsButton()
    {
        Debug.Log(name);
    }

    private void Start()
    {
        SetSortOrder(0);
    }

    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            switch (tSR.gameObject.name)
            {
                case "Card_Front":
                    tSR.sortingOrder = sOrd;
                    break;
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    break;
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }
}

[System.Serializable]
public class Decorator
{
    public string type;
    public Vector3 loc;
    public bool flip = false;
    public float scale = 1f;
}

[System.Serializable]
public class CardDefinition
{
    public string face;
    public int rank;
    public List<Decorator> pips = new List<Decorator>();
}
