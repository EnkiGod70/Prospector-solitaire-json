using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckReader
{
    public List<Decorator> decorators; // Matches the "decorators" array in the JSON
    public List<CardDefinition> cards; // Matches the "cards" array in the JSON
}

public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool startFaceUp = false;
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabCard;
    public GameObject prefabSprite;

    [Header("Set Dynamically")]
    public DeckReader jsonr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    public void InitDeck(string deckJSONText)
    {
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        dictSuits = new Dictionary<string, Sprite>()
        {
            { "C", suitClub },
            { "D", suitDiamond },
            { "H", suitHeart },
            { "S", suitSpade }
        };

        ReadDeck(deckJSONText);
        MakeCards();
    }

    public void ReadDeck(string deckJSONText)
    {
        jsonr = JsonUtility.FromJson<DeckReader>(deckJSONText);
        decorators = new List<Decorator>(jsonr.decorators);
        cardDefs = new List<CardDefinition>(jsonr.cards);
    }

    public CardDefinition GetCardDefinitionByRank(int rank)
    {
        foreach (CardDefinition cd in cardDefs)
        {
            if (cd.rank == rank)
            {
                return cd;
            }
        }
        return null;
    }

    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        cgo.AddComponent<BoxCollider>();
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();

        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if (card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    private void AddDecorators(Card card)
    {
        foreach (Decorator deco in decorators)
        {
            GameObject tGO = Instantiate(prefabSprite);
            SpriteRenderer tSR = tGO.GetComponent<SpriteRenderer>();

            if (deco.type == "suit")
            {
                tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                tSR.sprite = rankSprites[card.rank];
                tSR.color = card.color;
            }

            tSR.sortingOrder = 1;
            tGO.transform.SetParent(card.transform);
            tGO.transform.localPosition = deco.loc;

            if (deco.flip)
            {
                tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (deco.scale != 1)
            {
                tGO.transform.localScale = Vector3.one * deco.scale;
            }

            tGO.name = deco.type;
            card.decoGOs.Add(tGO);
        }
    }

    private void AddPips(Card card)
    {
        foreach (Decorator pip in card.def.pips)
        {
            GameObject tGO = Instantiate(prefabSprite);
            tGO.transform.SetParent(card.transform);
            tGO.transform.localPosition = pip.loc;

            if (pip.flip)
            {
                tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (pip.scale != 1)
            {
                tGO.transform.localScale = Vector3.one * pip.scale;
            }

            tGO.name = "pip";
            SpriteRenderer tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = dictSuits[card.suit];
            tSR.sortingOrder = 1;
            card.pipGOs.Add(tGO);
        }
    }

    private void AddFace(Card card)
    {
        if (card.def.face == "")
        {
            return;
        }

        GameObject tGO = Instantiate(prefabSprite);
        SpriteRenderer tSR = tGO.GetComponent<SpriteRenderer>();
        tSR.sprite = GetFace(card.def.face + card.suit);
        tSR.sortingOrder = 1;
        tGO.transform.SetParent(card.transform);
        tGO.transform.localPosition = Vector3.zero;
        tGO.name = "face";
    }

    private Sprite GetFace(string faceS)
    {
        foreach (Sprite tSP in faceSprites)
        {
            if (tSP.name == faceS)
            {
                return tSP;
            }
        }
        return null;
    }

    private void AddBack(Card card)
    {
        GameObject tGO = Instantiate(prefabSprite);
        SpriteRenderer tSR = tGO.GetComponent<SpriteRenderer>();
        tSR.sprite = cardBack;
        tGO.transform.SetParent(card.transform);
        tGO.transform.localPosition = Vector3.zero;
        tSR.sortingOrder = 2;
        tGO.name = "back";
        card.back = tGO;

        card.faceUp = startFaceUp;
    }

    public static void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();
        while (oCards.Count > 0)
        {
            int ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }
}
