using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardManager : MonoBehaviour
{
    public List<CardData> GetRuntimeDeck => runtimeDeck; //
    public List<CardData> GetGraveList => grave;       //

    [Header("Card Prefab & Spawn Point")]
    [SerializeField] private GameObject cardPrefab;      //
    [SerializeField] private Transform handSpawnParent;  //

    [Header("Card Layout Settings")]
    [SerializeField] private float cardGap = 2.0f;       //
    [SerializeField] private float maxHandWidth = 8.0f;   //
    [SerializeField] private float handYPosition = -3.5f; //

    [Header("Arc Layout Settings")]
    [SerializeField] private float arcIntensity = 0.2f;     // 카드가 아래로 휘는 둥근 강도
    [SerializeField] private float rotateIntensity = 4.0f;  // 카드가 부채꼴로 회전하는 강도

    [Header("Card Databases")]
    public List<CardData> allCards; //
    public List<CardData> deck = new List<CardData>(); //

    [HideInInspector] public List<CardData> runtimeDeck = new List<CardData>(); //
    [HideInInspector] public List<CardData> hand = new List<CardData>();        //
    [HideInInspector] public List<CardData> grave = new List<CardData>();       //
    [HideInInspector] public List<GameObject> activeCardObjects = new List<GameObject>(); //

    [Header("Combat Targets")]
    [SerializeField] private PlayerStats playerStats;   //
    [SerializeField] private MonsterStats monsterStats; //

    [Header("Deck & Grave UI Components")]
    [SerializeField] private TextMeshProUGUI deckCountText;   //
    [SerializeField] private TextMeshProUGUI graveCountText;  //

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
        int amount = 5;
        for (int i = 0; i < amount; i++)
        {
            if (runtimeDeck.Count == 0)
            {
                RefillDeckFromGrave();
                if (runtimeDeck.Count == 0) break;
            }

            CardData drawnCard = runtimeDeck[0];
            runtimeDeck.RemoveAt(0);
            hand.Add(drawnCard);

            // 카드 오브젝트 생성
            GameObject newCard = Instantiate(cardPrefab, handSpawnParent);
            activeCardObjects.Add(newCard); // 리스트 등록 필수!

            RectTransform rect = newCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition3D = Vector3.zero;
                rect.localScale = Vector3.one;
            }

            // ★ [복구] 생성된 CardUI를 가져와 데이터를 밀어 넣고 상호 연결을 활성화합니다.
            CardUI cardUI = newCard.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetCardData(drawnCard); // 데이터를 연결하고 UI 텍스트들을 갱신시킵니다.
            }
        }
        AlignHandCards(); // 배치 정렬 호출
        RefreshPileTexts();
    }

    // ★ [복구] 카드를 부채꼴 모양(아치형)으로 예쁘게 펼쳐주는 정렬 기능 복구
    private void AlignHandCards()
    {
        if (activeCardObjects.Count != hand.Count)
        {
            activeCardObjects.RemoveAll(item => item == null);
        }

        int totalCards = activeCardObjects.Count;

        for (int i = 0; i < totalCards; i++)
        {
            if (activeCardObjects[i] == null) continue;

            RectTransform rect = activeCardObjects[i].GetComponent<RectTransform>();
            if (rect != null)
            {
                // 1. 카드의 중앙 기준 상대적 인덱스 계산 (-1.5, -0.5, 0.5, 1.5 형태)
                float normalizeIndex = (i - (totalCards - 1) / 2f);

                // 2. 가로(X축) 간격 계산 (기존 cardGap 가중치 적용)
                float xPos = normalizeIndex * cardGap * 100f;

                // 3. 세로(Y축) 아치형 휨 계산 (arcIntensity 설정을 활용하여 중앙 카드가 더 높게 배치)
                // UI 환경에 맞게 기본 handYPosition 비율에 곡선 가중치를 더해줍니다.
                float yOffset = -Mathf.Pow(normalizeIndex, 2) * arcIntensity * 50f;
                float yPos = (handYPosition * 50f) + yOffset;

                // 4. 회전 각도 계산 (rotateIntensity 설정을 활용하여 양 끝 카드가 부채꼴로 기울어짐)
                float zRotation = -normalizeIndex * rotateIntensity;

                // UI RectTransform 좌표 및 회전 적용
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

    // CardManager.cs 내부 아무 데나 추가 (맨 아래 추천)
    public void SetHandInteractable(bool isInteractable)
    {
        foreach (GameObject cardObj in activeCardObjects)
        {
            if (cardObj == null) continue;

            CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // false가 되면 Raycast Target을 완전히 끈 것과 동일하게 마우스가 그냥 통과합니다.
                canvasGroup.blocksRaycasts = isInteractable;
            }
        }
    }
}

