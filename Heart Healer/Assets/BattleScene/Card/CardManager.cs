using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
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
    public List<CardData> hand = new List<CardData>();
    public List<CardData> grave = new List<CardData>();

    [HideInInspector] public List<GameObject> activeCardObjects = new List<GameObject>();

    [Header("Combat Targets")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private MonsterStats monsterStats;

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        monsterStats = FindFirstObjectByType<MonsterStats>();

        // 덱을 생성하고 셔플합니다.
        PrepareInitDeck();

        // =======================================================
        // ★ [복구 완료] 전투 시작 시 최초 5장 드로우를 실행합니다!
        // =======================================================
        DrawCard();
        // =======================================================
    }

    public void PrepareInitDeck()
    {
        deck.Clear(); hand.Clear(); grave.Clear();
        // 덱에 기본 샘플 3장씩 카드 세팅
        foreach (CardData card in allCards)
        {
            deck.Add(card); deck.Add(card); deck.Add(card);
        }
        Shuffle(deck);
    }

    public void DrawCard()
    {
        int amount = 5; // 기본 5장 드로우 기믹
        for (int i = 0; i < amount; i++)
        {
            if (deck.Count == 0)
            {
                RefillDeckFromGrave();
                if (deck.Count == 0) break;
            }

            CardData drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);

            GameObject newCardObj = Instantiate(cardPrefab, handSpawnParent);
            activeCardObjects.Add(newCardObj);

            // =======================================================
            // ★ [교정 완료] Setup 대신 원본에 있는 RefreshUI를 정확히 호출합니다.
            // =======================================================
            CardUI uiComponent = newCardObj.GetComponent<CardUI>();
            if (uiComponent != null)
            {
                uiComponent.RefreshUI(drawnCard, this);
            }
            // =======================================================
        }
        AlignHandCards();
    }
    public void UseCard(GameObject cardObj)
    {
        int index = activeCardObjects.IndexOf(cardObj);
        if (index == -1) return;

        CardData usedCardData = hand[index];

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SpendMana(usedCardData.cost);
        }

        if (playerStats != null && monsterStats != null)
        {
            if (usedCardData.damage > 0)
            {
                int finalDmg = playerStats.CalculateOutputDamage(usedCardData.damage);
                monsterStats.TakeDamage(finalDmg);
            }
            if (usedCardData.puredamage > 0)
            {
                monsterStats.TakePureDamage(usedCardData.puredamage);
            }
            if (usedCardData.armorAmount > 0)
            {
                playerStats.GetArmor(usedCardData.armorAmount);
            }
            if (usedCardData.cardName != null && (usedCardData.cardName.Contains("벌크업") || usedCardData.cardName.Contains("영양제")))
            {
                playerStats.AddAttack(2);
            }
        }

        hand.RemoveAt(index);
        activeCardObjects.RemoveAt(index);
        grave.Add(usedCardData);

        Destroy(cardObj);
        AlignHandCards();
    }

    public void AlignHandCards()
    {
        int cardCount = activeCardObjects.Count;
        if (cardCount == 0) return;

        float totalWidth = cardGap * (cardCount - 1);
        if (totalWidth > maxHandWidth) totalWidth = maxHandWidth;

        float currentCardGap = cardCount > 1 ? totalWidth / (cardCount - 1) : 0f;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            if (activeCardObjects[i] == null) continue;

            float targetX = startX + (i * currentCardGap);
            float normalizedPos = cardCount > 1 ? (-1f + (2f * i / (cardCount - 1))) : 0f;
            float targetY = handYPosition - (Mathf.Abs(normalizedPos) * arcIntensity);

            activeCardObjects[i].transform.localPosition = new Vector3(targetX, targetY, -i * 0.1f);
            float targetRotationZ = -normalizedPos * rotateIntensity;
            activeCardObjects[i].transform.rotation = Quaternion.Euler(0f, 0f, targetRotationZ);
        }
    }

    public void RefillDeckFromGrave()
    {
        if (grave.Count == 0) return;
        deck.AddRange(grave);
        grave.Clear();
        Shuffle(deck);
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
            if (activeCardObjects[i] != null)
            {
                grave.Add(hand[i]);
                Destroy(activeCardObjects[i]);
            }
        }
        hand.Clear();
        activeCardObjects.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) DrawCard();
    }
}