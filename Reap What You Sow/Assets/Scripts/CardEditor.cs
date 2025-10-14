using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardEditor : ScriptableObject
{
    public string cardTreat;

    public string cardTrick;

    public int cardCandy;

    public int neighborCandy;
}