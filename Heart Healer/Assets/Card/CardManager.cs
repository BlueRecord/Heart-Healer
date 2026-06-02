using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Card Prefab & Spawn Point")]
    [SerializeField] private GameObject cardPrefab;        // Card_Base 프리팹
    [SerializeField] private Transform handSpawnParent;   // 카드가 생성될 부모 위치 (선택)

    [Header("Card Layout Settings (기본 정렬 설정)")]
    [SerializeField] private float cardGap = 2.0f;        // 카드 사이의 최대 간격
    [SerializeField] private float maxHandWidth = 8.0f;   // 카드가 퍼질 수 있는 최대 가로 폭
    [SerializeField] private float handYPosition = -3.5f; // 손패 기본 Y 좌표 (하단)

    [Header("Arc Layout Settings (부채꼴 추가 설정)")]
    [SerializeField] private float arcIntensity = 0.2f;   // 곡선의 깊이 (클수록 양끝 카드가 더 아래로 내려감)
    [SerializeField] private float rotateIntensity = 4.0f; // 회전 강도 (클수록 양끝 카드가 더 많이 기울어짐)

    [Header("Card Databases")]
    public List<CardData> allCards; // 게임에 존재하는 모든 카드
    public List<CardData> deck = new List<CardData>(); // 현재 남은 덱
    public List<CardData> hand = new List<CardData>(); // 현재 내 손의 카드 데이터
    public List<CardData> grave = new List<CardData>(); // 버려진 카드 (무덤)

    // 씬(화면)에 생성되어 존재하는 진짜 카드 게임 오브젝트 리스트
    private List<GameObject> activeCardObjects = new List<GameObject>();

    [Header("Turn Settings")]
    public int initDrawCount = 5;  // 매 턴 시작할 때 뽑을 카드 수
    public int maxHandSize = 10;

    void Start()
    {
        PrepareInitDeck();
        StartNewTurn();
    }

    public void PrepareInitDeck()
    {
        deck.Clear();
        hand.Clear();
        grave.Clear();

        foreach (CardData card in allCards)
        {
            deck.Add(card); deck.Add(card); deck.Add(card);
        }

        Shuffle(deck);
    }

    public void StartNewTurn()
    {
        Debug.Log("<color=green><b>[턴 시작] 새로운 턴이 시작되었습니다!</b></color>");
        for (int i = 0; i < initDrawCount; i++)
        {
            DrawCard();
        }
    }

    public void EndTurn()
    {
        Debug.Log("<color=red><b>[턴 종료] 턴을 종료하고 손패를 모두 무덤으로 보냅니다.</b></color>");
        grave.AddRange(hand);
        hand.Clear();

        foreach (GameObject cardObj in activeCardObjects)
        {
            if (cardObj != null) Destroy(cardObj);
        }
        activeCardObjects.Clear();

        StartNewTurn();
    }

    public void DrawCard()
    {
        if (hand.Count >= maxHandSize) return;
        if (deck.Count == 0) RefillDeckFromGrave();
        if (deck.Count == 0) return;

        CardData drawnCardData = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawnCardData);

        GameObject newCardObj = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, handSpawnParent);

        CardUI cardUI = newCardObj.GetComponent<CardUI>();
        if (cardUI != null)
        {
            // 자기 자신(this)을 주소로 넘겨줍니다.
            cardUI.Setup(drawnCardData, this);
        }

        activeCardObjects.Add(newCardObj);

        AlignHandCards();

        Debug.Log($"드로우: {drawnCardData.cardName} (남은 덱: {deck.Count}장 / 무덤: {grave.Count}장)");
    }

    /// <summary>
    /// 플레이어가 카드를 필드 위로 던져서 사용했을 때 호출되는 함수
    /// </summary>
    /// <param name="cardObj">사용된 카드 오브젝트</param>
    public void UseCard(GameObject cardObj)
    {
        // 1. 리스트에서 이 카드 오브젝트의 인덱스를 찾습니다.
        int index = activeCardObjects.IndexOf(cardObj);

        // 만약 리스트에 없는 유령 오브젝트라면 예외 처리
        if (index == -1) return;

        // 2. 데이터(hand)와 오브젝트 리스트에서 각각 제거하고, 무덤(grave)으로 데이터를 보냅니다.
        CardData usedCardData = hand[index];
        hand.RemoveAt(index);
        activeCardObjects.RemoveAt(index);
        grave.Add(usedCardData);

        // 3. 디버그 로그로 카드가 정상적으로 발동했는지 확인합니다.
        // (나중에여기에 '공격력 추가', '방어력 추가' 같은 실제 효과 발동 코드가 들어갑니다!)
        Debug.Log($"<color=cyan><b>[카드 사용] {usedCardData.cardName} 카드를 사용했습니다! (효과: {usedCardData.GetDynamicDescription()})</b></color>");

        // 4. 화면에서 카드 오브젝트를 제거합니다.
        Destroy(cardObj);

        // 5. ★중요: 카드가 한 장 사라졌으므로, 남아있는 손패들을 부채꼴 모양으로 다시 정렬합니다.
        AlignHandCards();
    }

    /// <summary>
    /// 업그레이드됨: 카드들을 부채꼴 모양으로 둥글게 곡선 정렬하는 함수
    /// </summary>
    public void AlignHandCards()
    {
        int cardCount = activeCardObjects.Count;
        if (cardCount == 0) return;

        // 1. 가로 너비 및 간격 계산
        float currentGap = cardGap;
        float totalWidth = (cardCount - 1) * cardGap;

        if (totalWidth > maxHandWidth)
        {
            currentGap = maxHandWidth / (cardCount - 1);
            totalWidth = maxHandWidth;
        }

        float startX = -totalWidth / 2f;

        // 2. 루프를 돌며 각 카드의 위치(포물선)와 회전 계산
        for (int i = 0; i < cardCount; i++)
        {
            if (activeCardObjects[i] == null) continue;

            // 기본 가로(X) 위치 계산
            float targetX = startX + (i * currentGap);

            // ★ [부채꼴 핵심 1] 비율(Normalized Value) 계산
            // 카드가 1장이면 중앙(0), 여러 장이면 왼쪽 끝(-1)부터 오른쪽 끝(1)까지 변하게 만듭니다.
            float normalizedPos = 0f;
            if (cardCount > 1)
            {
                normalizedPos = (i / (float)(cardCount - 1)) * 2f - 1f; // 결과값 범위: -1.0 ~ 1.0
            }

            // ★ [부채꼴 핵심 2] 포물선 공식으로 Y축 값 깎기 (y = -x^2)
            // 중앙(0)에 가까울수록 0이 되고, 양끝(-1, 1)으로 갈수록 arcIntensity 비율만큼 아래로 내려갑니다.
            float yOffset = -Mathf.Pow(normalizedPos, 2) * arcIntensity;
            float targetY = handYPosition + yOffset;

            // 위치 지정 (Z축은 겹침 순서를 위해 뒤에 배치된 카드가 아주 미세하게 앞으로 오도록 세팅 가능)
            Vector3 targetPosition = new Vector3(targetX, targetY, -i * 0.01f);
            activeCardObjects[i].transform.position = targetPosition;

            // 카드 프리팹에 방금 붙인 Sorting Group을 찾아 순서(Order)를 강제로 먹입니다.
            // i가 커질수록(오른쪽 카드로 갈수록) 높은 레이어 값을 가져서 앞쪽에 예쁘게 덮입니다.
            var sortingGroup = activeCardObjects[i].GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sortingGroup != null)
            {
                sortingGroup.sortingOrder = i;
            }
            // ★ [부채꼴 핵심 3] Z축 회전 적용
            // 왼쪽 카드는 양수 회전(시계방향), 오른쪽 카드는 음수 회전(반시계방향)을 줍니다.
            float targetRotationZ = -normalizedPos * rotateIntensity;
            activeCardObjects[i].transform.rotation = Quaternion.Euler(0f, 0f, targetRotationZ);
        }
    }

    public void RefillDeckFromGrave()
    {
        if (grave.Count == 0) return;
        deck.AddRange(grave); grave.Clear();
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) EndTurn();
        if (Input.GetKeyDown(KeyCode.Space)) DrawCard();
    }
}