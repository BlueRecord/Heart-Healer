using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("최초 시작 시 기본 제공할 카드 목록")]
    [SerializeField] private List<CardData> startingCards = new List<CardData>();

    // 플레이어가 현재 게임 세션 동안 영구적으로 소지하는 전체 덱
    private List<CardData> playerDeck = new List<CardData>();
    public List<CardData> PlayerDeck => playerDeck;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 다른 씬으로 넘어가도 파괴되지 않음
            InitStartingDeck();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 최초 시작 시 기본 덱 생성
    public void InitStartingDeck()
    {
        playerDeck.Clear();
        foreach (var card in startingCards)
        {
            if (card != null)
            {
                playerDeck.Add(card);
            }
        }
        Debug.Log($"<color=green><b>[DeckManager] 기본 덱 {playerDeck.Count}장 생성 완료!</b></color>");
    }

    // [추후 확장용] 상점 구매나 전투 보상으로 카드를 얻으면 이 함수를 호출해 덱에 추가합니다.
    public void AddCardToDeck(CardData newCard)
    {
        if (newCard != null)
        {
            playerDeck.Add(newCard);
            Debug.Log($"<color=cyan><b>[DeckManager] 덱에 카드 추가: {newCard.cardName} (총 {playerDeck.Count}장)</b></color>");
        }
    }

    // DeckManager.cs에 추가
    public void LoadDeckFromIDs(List<string> cardIDs)
    {
        playerDeck.Clear();
        foreach (string id in cardIDs)
        {
            // 카드 ID(이름)로 카드를 찾아서 다시 덱에 추가
            // 예: Resources.Load를 쓰거나, 미리 정의된 카드 데이터베이스에서 찾기
            CardData loadedCard = Resources.Load<CardData>("Cards/" + id);
            if (loadedCard != null)
            {
                playerDeck.Add(loadedCard);
            }
        }
        Debug.Log($"덱 복구 완료! 총 {playerDeck.Count}장");
    }
}