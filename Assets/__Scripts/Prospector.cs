using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckJSON;
    public TextAsset layoutJSON;
    public Vector3 layoutCenter;
    public float reloadDelay = 2f;
    public TMP_Text gameOverText, roundResultText;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    private void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    private void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckJSON.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutJSON.text);

        drawPile = ConvertListCardsToListCardProspector(deck.cards);
        LayoutGame();

        // Ensure the first target card is set properly
        SetFirstTarget();
        UpdateDrawPile();
    }

    private List<CardProspector> ConvertListCardsToListCardProspector(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        foreach (Card tCD in lCD)
        {
            lCP.Add(tCD as CardProspector);
        }
        return lCP;
    }

    private CardProspector Draw()
    {
        if (drawPile.Count == 0) return null;
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    private void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.position = layoutCenter;
        }

        foreach (SlotDef tSD in layout.slotDefs)
        {
            CardProspector cp = Draw();
            if (cp == null) continue;

            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID + 20f
            );

            cp.faceUp = true;
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }
    }

    private void SetFirstTarget()
    {
        CardProspector firstCard = Draw();
        if (firstCard != null)
        {
            MoveToTarget(firstCard);
        }
        else
        {
            Debug.LogError("Draw pile is empty at the start!");
        }
    }

    private void MoveToTarget(CardProspector cd)
    {
        if (target != null)
        {
            MoveToDiscard(target);
        }

        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * -8, // Discard pile position
            layout.multiplier.y * 5.5f,
            -layout.drawPile.layerID + 20f
        );
        cd.faceUp = true;
        cd.SetSortingLayerName("Discard");

        Debug.Log("Target card set to: " + cd.name);
    }

    private void MoveToDiscard(CardProspector cd)
    {
        discardPile.Add(cd);
        cd.state = eCardState.discard;
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * -8, // Discard pile position
            layout.multiplier.y * 5.5f,
            -layout.drawPile.layerID - discardPile.Count
        );

        cd.faceUp = true;
        cd.SetSortingLayerName("Discard");
        cd.SetSortOrder(discardPile.Count);
    }

    private void UpdateDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardProspector cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            float xOffset = layout.drawPile.x + (i * layout.drawPile.xStagger);
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * xOffset,
                layout.multiplier.y * layout.drawPile.y,
                -layout.drawPile.layerID + 20.1f * i
            );
            cd.faceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
        }
    }

    public void CardClicked(CardProspector cd)
    {
        if (cd == null) return;

        if (cd.state == eCardState.target) return;

        if (cd.state == eCardState.tableau && CanPlayCard(cd))
        {
            tableau.Remove(cd);
            MoveToTarget(cd);
            CheckForGameOver();
        }
        else if (cd.state == eCardState.drawpile)
        {
            CardProspector nextCard = Draw();
            if (nextCard != null)
            {
                MoveToTarget(nextCard);
                UpdateDrawPile();
            }
        }
    }

    private bool CanPlayCard(CardProspector cd)
    {
        if (!cd.faceUp) return false;

        foreach (int blockerID in cd.slotDef.hiddenBy)
        {
            CardProspector blocker = tableau.Find(c => c.layoutID == blockerID);
            if (blocker != null && blocker.state != eCardState.discard)
            {
                return false;
            }
        }

        return AdjacentRank(cd, target);
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        return Mathf.Abs(c0.rank - c1.rank) == 1 || (c0.rank == 1 && c1.rank == 13) || (c0.rank == 13 && c1.rank == 1);
    }

    private void CheckForGameOver()
    {
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }

        if (drawPile.Count == 0)
        {
            foreach (CardProspector cd in tableau)
            {
                if (CanPlayCard(cd)) return;
            }
            GameOver(false);
        }
    }

    private void GameOver(bool won)
    {
        gameOverText.gameObject.SetActive(true);
        roundResultText.gameObject.SetActive(true);
        roundResultText.text = won ? "You Won!" : "Game Over";
        Invoke("ReloadLevel", reloadDelay);
    }

    private void ReloadLevel()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    private void SetUpUITexts()
    {
        gameOverText.gameObject.SetActive(false);
        roundResultText.gameObject.SetActive(false);
    }
}
