using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public List<CardData> allCards; //게임에 존재하는 모든 카드
    public List<CardData> deck = new List<CardData>(); // 현재 남은 덱
    public List<CardData> hand = new List<CardData>(); // 현재 내 손의 카드
    public List<CardData> grave = new List<CardData>(); //버려진 카드

    public int maxHandSize = 10;
    public int drawCards = 5;
    void Start()
    {
        PrepareInitDeck();
    }
    public void PrepareInitDeck()
    {
        if (allCards != null && allCards.Count > 0)
        {
            deck = new List<CardData>(allCards);
        }

        hand.Clear();
        grave.Clear();

        // 첫 셔플
        Shuffle();

        // [추가] 초기화가 끝났으니 설정해 둔 값(5장)만큼 첫 손패를 자동 드로우합니다!
        DrawMultipleCards(drawCards);
    }
    public void DrawCard()
    {
        // 만약 드로우 덱이 비어있다면 무덤에서 리필만 해줍니다.
        if (deck.Count == 0)
        {
            RefillDeckFromGrave();
        }

        // [중요] 리필을 시도했는데도 덱이 여전히 0장이라면(무덤도 비었다면), 
        // 더 이상 뽑을 카드가 아예 없는 것이므로 안전하게 함수를 종료(리턴)합니다.
        if (deck.Count == 0)
        {
            Debug.LogWarning("덱과 무덤이 모두 비어있어 더 이상 카드를 뽑을 수 없습니다.");
            return;
        }

        // 이제 안전하게 한 장을 뽑아 핸드에 넣습니다.
        CardData drawnCard = deck[0];
        deck.RemoveAt(0);

        if (hand.Count < maxHandSize)
        {
            hand.Add(drawnCard);
            // CardUI.CreateCardMotion(drawnCard);
        }
        else
        {
            grave.Add(drawnCard);
            Debug.Log("손패가 가득 차 카드가 무덤으로 버려졌습니다.");
        }
    }
    public void DrawMultipleCards(int count)
    {
        Debug.Log($"[시스템] 카드를 한 번에 {count}장 뽑습니다.");

        // 매개변수로 받은 count 횟수만큼 DrawCard()를 반복 실행
        for (int i = 0; i < count; i++)
        {
            DrawCard();
        }
    }
    public void RefillDeckFromGrave()
    {
        // 무덤에 있는 모든 데이터를 드로우 덱으로 이동
        deck.AddRange(grave);
        grave.Clear(); // 무덤 비우기

        // 다시 리필되었으니 반드시 섞어줌
        Shuffle();
        Debug.Log("무덤의 카드를 덱으로 되돌리고 셔플했습니다.");
    }
    public void Shuffle()
    {
        deck = deck.OrderBy(card => System.Guid.NewGuid()).ToList();
    }
    
    public void UseCard()
    {

    }
    
    public void TurnEnd()
    {

    }

    public void DiscardHand()
    {

    }

    public void OnNewTurnStart()
    {

    }


    void Update()
    {

    }
}
