using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pyramid : MonoBehaviour
{
    static public Pyramid S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
   

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardPyramid> drawPile;
    public Transform layoutAnchor;
    public CardPyramid piletop;
    public List<CardPyramid> tableau;
    public List<CardPyramid> discardPile;

    CardPyramid firstCard;
    CardPyramid secondCard;
    CardPyramid unselect;

    void Awake()
    {
        S = this; 
    }
        void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);
        layout = GetComponent<Layout>(); 
        layout.ReadLayout(layoutXML.text); 
        drawPile = ConvertListCardsToListCardPyramid(deck.cards);
        LayoutGame();
    }

    void Update()
    {
        if (firstCard == true)
        {
            SpriteRenderer spriteColor;
            spriteColor = firstCard.GetComponentInChildren<SpriteRenderer>();
            spriteColor.color = Color.cyan;
            unselect = firstCard;
        }
        if (firstCard == false && unselect == true)
        {
            SpriteRenderer spriteColor;
            spriteColor = unselect.GetComponentInChildren<SpriteRenderer>();
            spriteColor.color = Color.white;
        }
    }

    List<CardPyramid> ConvertListCardsToListCardPyramid(List<Card> lCD) //Also a class
    {
        List<CardPyramid> lCP = new List<CardPyramid>();
        CardPyramid tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardPyramid;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    CardPyramid Draw()
    {
        CardPyramid cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)           
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;          
            layoutAnchor.transform.position = layoutCenter;     
        }
        CardPyramid cp; 

        foreach (SlotDef tSD in layout.slotDefs) 
        {
            cp = Draw();    
            cp.faceUp = tSD.faceUp; 
            cp.transform.parent = layoutAnchor;     
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
           
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eNewCardState.tableau;
            
            cp.SetSortingLayerName(tSD.layerName); 

            tableau.Add(cp);    
        }

        foreach (CardPyramid tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)   
            {
                cp = FindCardByLayoutID(hid);      
                tCP.hiddenBy.Add(cp);          
            }
        }
        MoveToPiletop(Draw()); 
        UpdateDrawPile();
    }

    CardPyramid FindCardByLayoutID(int layoutID)
    {
        foreach (CardPyramid tCP in tableau)
        {
           
            if (tCP.layoutID == layoutID)
            {
                return (tCP);
            }
        }
        return (null);
    }

    void MoveToDiscard(CardPyramid cd)   
    {
        cd.state = eNewCardState.discard;  
        discardPile.Add(cd);           
        cd.transform.parent = layoutAnchor;     

        
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true; 
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToPiletop(CardPyramid cd)  
    {
        if (piletop != null) MoveToDiscard(piletop);
        piletop = cd;        
        cd.state = eNewCardState.piletop;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true; 
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    
    void UpdateDrawPile()
    {
        CardPyramid cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * layout.drawPile.x,
                layout.multiplier.y * layout.drawPile.y,
                -layout.drawPile.layerID + 0.1f);
            cd.faceUp = false;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public int card_cleared_count;

    public void CardClicked(CardPyramid cd)
    {

        switch (cd.state)
        {
            case eNewCardState.piletop:
                print("This card is on the piletop");
                if (cd.rank == 13){
                    MoveToDiscard(piletop);
                    card_cleared_count += 1;
                    MoveToPiletop(Draw());
                    UpdateDrawPile();
                }
                if (firstCard == true){
                    if (cd == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = cd;
                    print("This card has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        card_cleared_count += 1;
                        firstCard.layoutID = 0;
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        card_cleared_count += 1;
                        secondCard = null;
                        MoveToPiletop(Draw());
                        UpdateDrawPile();
                    } else
                    {
                        firstCard = null;
                        secondCard = null;
                    }
                } else
                {
                    firstCard = cd;
                    print("This card has been selected as card one.");
                }
                

                break;

            case eNewCardState.drawpile:
                MoveToDiscard(piletop);
                MoveToPiletop(Draw());
                UpdateDrawPile();
                break;

            case eNewCardState.tableau:
                bool noGo = true;
                foreach (CardPyramid cover in cd.hiddenBy)
                {
                    if (cover.state == eNewCardState.tableau)
                    {
                        print("I am hidden by: " + cover);
                        noGo = false;
                    }
                }
                if (noGo == false)  
                {
                    return;
                }

                if (cd.rank == 13)
                {
                    MoveToDiscard(cd);
                    card_cleared_count += 1;
                    if (piletop == null)
                    {
                        MoveToPiletop(Draw());
                    }
                }


                if (firstCard == true)
                {
                    if (cd == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = cd;
                    print("This card has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        card_cleared_count += 1;
                        firstCard.SetSortOrder(-10 * firstCard.rank); 
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        card_cleared_count += 1;
                        secondCard.SetSortOrder(-10 * secondCard.rank);
                        secondCard = null;
                        if (piletop == null){
                            MoveToPiletop(Draw());
                        }
                    } else
                    {
                        firstCard = null;
                        secondCard = null;
                    }
                }
                else
                {
                    firstCard = cd;
                    print("This card has been selected as card one.");
                }
                break;
        }
    }
    public bool EqualsThirteen(CardPyramid c0, CardPyramid c1)
    {
        if (Mathf.Abs(c0.rank + c1.rank) == 13)
        {
            return (true);
        }
        return (false);
    }
}
