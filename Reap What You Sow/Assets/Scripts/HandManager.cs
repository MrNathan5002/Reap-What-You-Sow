using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public DeckManager deckManager;
    public GameObject cardPrefab;
    public Transform handTransform;

    public float fanSpread = -6f;
    public float cardSpacing = 12f;
    public float verticalSpacing = 5f;
    public float Handsize = 5f;

    public List<GameObject> cardsInHand = new List<GameObject>();

    void Start()
    {

    }

    public void AddCardToHand(CardEditor cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);

        newCard.GetComponent<CardDisplay>().cardData = cardData;

        UpdateHandVisuals();
    }

    void Update(){
        //UpdateHandVisuals();
    }
    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1){
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }

        for (int i=0; i < cardCount; i++){
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));

            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }

    public void DiscardAll()
    {
        // Destroy all card visuals and clear list
        for (int i = cardsInHand.Count - 1; i >= 0; i--)
        {
            if (cardsInHand[i]) Destroy(cardsInHand[i]);
        }
        cardsInHand.Clear();
    }

    public void RemoveCardGO(GameObject go)
    {
        int i = cardsInHand.IndexOf(go);
        if (i >= 0)
        {
            cardsInHand.RemoveAt(i);
            Destroy(go);
        }
    }

    public void AddCardToHand(CardInstance cardInstance)
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);

        // Set data on display AND instance link
        var disp = newCard.GetComponent<CardDisplay>();
        if (disp) disp.cardData = cardInstance.def;

        var hc = newCard.GetComponent<HandCard>() ?? newCard.AddComponent<HandCard>();
        hc.instance = cardInstance;

        UpdateHandVisuals();
    }
}