using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardEditor cardData;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        UpdateCardDisplay();
    }

    public void UpdateCardDisplay()
    {
        spriteRenderer.sprite = cardData.cardSprite;
    }
}
