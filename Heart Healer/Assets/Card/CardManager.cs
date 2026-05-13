using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardData> allCards; //게임에 존재하는 모든 카드
    public List<CardData> deck = new List<CardData>(); // 현재 남은 덱
    public List<CardData> hand = new List<CardData>(); // 현재 내 손의 카드
    public List<CardData> grave = new List<CardData>(); //버려진 카드

    public int maxHandSize = 10;
    void Start()
    {
        
    }
    public void PrepareInitDeck()
    {

    }
    public void DrawCard()
    {

    }
    public void RefillDeckFromGrave()
    {

    }
    public void Shuffle(List<CardData> list)
    {

    }


    void Update()
    {
        
    }
}
