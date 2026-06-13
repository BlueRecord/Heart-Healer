using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardManager : MonoBehaviour
{
    public List<CardData> GetRuntimeDeck => runtimeDeck;
    public List<CardData> GetGraveList => grave;

    [Header("Card Prefab & Spawn Point")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handSpawnParent;

    [Header("Card Layout Settings")]
    [SerializeField] private float cardGap = 2.0f;
    [SerializeField] private float maxHandWidth = 8.0f;
    [SerializeField] private float handYPosition = -3.5f;

    [Header("Arc Layout Settings")]
    [SerializeField] private float arcIntensity = 0.2f;
    [SerializeField] private float rotateIntensity = 4.0f;

    [Header("Card Databases")]
    public List<CardData> allCards;
    public List<CardData> deck = new List<CardData>();

    [HideInInspector] public List<CardData> runtimeDeck = new List<CardData>();
    [HideInInspector] public List<CardData> hand = new List<CardData>();
    [HideInInspector] public List<CardData> grave = new List<CardData>();
    [HideInInspector] public List<GameObject> activeCardObjects = new List<GameObject>();

    [Header("Combat Targets")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private MonsterStats monsterStats;

    [Header("Deck & Grave UI Components")]
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI graveCountText;

    private CardEffectProcessor effectProcessor;

    void Awake()
    {
        effectProcessor = gameObject.AddComponent<CardEffectProcessor>();
    }

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        monsterStats = FindFirstObjectByType<MonsterStats>();
        RefreshPileTexts();
    }

    public void PrepareInitDeck()
    {
        runtimeDeck.Clear(); hand.Clear(); grave.Clear();
        if (deck != null && deck.Count > 0)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                if (deck[i] != null) runtimeDeck.Add(deck[i]);
            }
        }
        Shuffle(runtimeDeck);
        RefreshPileTexts();
    }

    public void DrawCard()
    {
        int drawAmount = 5;
        if (playerStats != null)
        {
            drawAmount += playerStats.NextTurnBonusDraw;
            playerStats.ClearNextTurnDraw();
        }
        DrawSpecificAmount(drawAmount);
    }

    public void DrawSpecificAmount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (runtimeDeck.Count == 0)
            {
                RefillDeckFromGrave();
                if (runtimeDeck.Count == 0) break;
            }

            CardData drawnCard = runtimeDeck[0];
            runtimeDeck.RemoveAt(0);
            hand.Add(drawnCard);

            GameObject newCard = Instantiate(cardPrefab, handSpawnParent);
            activeCardObjects.Add(newCard);

            RectTransform rect = newCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition3D = Vector3.zero;
                rect.localScale = Vector3.one;
            }

            CardUI cardUI = newCard.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetCardData(drawnCard);
            }
        }
        AlignHandCards();
        RefreshPileTexts();
    }

    private void AlignHandCards()
    {
        activeCardObjects.RemoveAll(item => item == null);
        int totalCards = activeCardObjects.Count;

        for (int i = 0; i < totalCards; i++)
        {
            if (activeCardObjects[i] == null) continue;

            RectTransform rect = activeCardObjects[i].GetComponent<RectTransform>();
            if (rect != null)
            {
                float normalizeIndex = (i - (totalCards - 1) / 2f);
                float xPos = normalizeIndex * cardGap * 100f;
                float yOffset = -Mathf.Pow(normalizeIndex, 2) * arcIntensity * 50f;
                float yPos = (handYPosition * 50f) + yOffset;
                float zRotation = -normalizeIndex * rotateIntensity;

                rect.anchoredPosition = new Vector2(xPos, yPos);
                rect.localRotation = Quaternion.Euler(0, 0, zRotation);
            }
        }
    }

    public void RefillDeckFromGrave()
    {
        if (grave.Count == 0) return;
        runtimeDeck.AddRange(grave);
        grave.Clear();
        Shuffle(runtimeDeck);
        RefreshPileTexts();
    }

    public void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            CardData temp = list[i]; list[i] = list[rnd]; list[rnd] = temp;
        }
    }

    public void DiscardHand()
    {
        for (int i = activeCardObjects.Count - 1; i >= 0; i--)
        {
            if (activeCardObjects[i] != null) Destroy(activeCardObjects[i]);
        }
        activeCardObjects.Clear();
        grave.AddRange(hand);
        hand.Clear();
        RefreshPileTexts();
    }

    public void UseCard(GameObject cardObj)
    {
        int index = activeCardObjects.IndexOf(cardObj);
        if (index == -1) return;

        CardData usedCardData = hand[index];
        playerStats = FindFirstObjectByType<PlayerStats>();
        monsterStats = FindFirstObjectByType<MonsterStats>();

        if (playerStats == null || monsterStats == null) return;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SpendMana(usedCardData.cost);
        }

        // 이펙트 프로세서로 고유 효과 수행
        if (effectProcessor != null)
        {
            effectProcessor.ExecuteEffect(usedCardData, playerStats, monsterStats);
        }

        // 기존 레거시 하드코딩 호환용 분기 보존
        if (usedCardData.cardName != null && (usedCardData.cardName.Contains("벌크업") || usedCardData.cardName.Contains("영양제")))
        {
            playerStats.AddAttack(2);
        }

        hand.RemoveAt(index);
        activeCardObjects.RemoveAt(index);
        grave.Add(usedCardData);

        Destroy(cardObj);
        AlignHandCards();

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.RefreshBattleUI();
        }
        RefreshPileTexts();
    }

    public void RefreshPileTexts()
    {
        if (deckCountText != null) deckCountText.text = runtimeDeck.Count.ToString();
        if (graveCountText != null) graveCountText.text = grave.Count.ToString();
    }

    public void SetHandInteractable(bool isInteractable)
    {
        foreach (GameObject cardObj in activeCardObjects)
        {
            if (cardObj == null) continue;
            CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.blocksRaycasts = isInteractable;
        }
    }
}