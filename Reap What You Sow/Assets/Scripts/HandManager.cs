using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;

    public Transform handTransform;

    public float fanSpread = 0f;

    public float cardSpacing = 8f;

    public float verticalSpacing = 10f;

    public List<GameObject> cardsInHand = new List<GameObject>();

    void Start()
    {
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
    }

    public void AddCardToHand()
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);

        UpdateHandVisuals();
    }

    void Update(){
        UpdateHandVisuals();
    }
    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        for (int i=0; i < cardCount; i++){
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));

            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
}