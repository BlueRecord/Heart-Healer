using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardIllustration;

    [Header("Card Visual Sub-Objects")]
    [SerializeField] private GameObject cardFrontObject;
    [SerializeField] private GameObject cardBackObject;

    [Header("Play Settings")]
    [SerializeField] private float playYThreshold = 150f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [HideInInspector] public CardData runtimeData;
    private CardManager cardManager;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 확실하게 시스템 소스를 명시하여 컴파일러 모호성 차단
        cardManager = UnityEngine.Object.FindFirstObjectByType<CardManager>();
    }

    void Update()
    {
        if (runtimeData == null || descriptionText == null) return;

        // 씬에서 활성화된 스탯 컴포넌트 안전하게 탐색
        PlayerStats pStats = UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
        MonsterStats mStats = UnityEngine.Object.FindFirstObjectByType<MonsterStats>();

        int currentAttackBuff = 0;
        bool isMonsterVulnerable = false;

        // [공격력 참조 트래킹] 
        // 만약 하단의 pStats.attack 이나 mStats.vulnerableTurns에서 빨간줄(CS1061/CS0122)이 계속 뜬다면,
        // 부모 클래스인 EntityStats.cs 파일을 열어 해당 변수들을 public int attack; / public int vulnerableTurns; 로 변경해 주셔야 정상 연결됩니다.
        if (pStats != null)
        {
            currentAttackBuff = pStats.baseAttack;
        }

        if (mStats != null)
        {
            isMonsterVulnerable = mStats.vulnerableTurns > 0;
        }

        // 손패 카드는 현재 실시간 스탯(버프값 포함)을 받아 실시간으로 갱신
        string updatedDesc = runtimeData.GetDynamicDescription(currentAttackBuff, isMonsterVulnerable);

        if (descriptionText.text != updatedDesc)
        {
            descriptionText.text = updatedDesc;
        }
    }

    public void SetCardData(CardData data)
    {
        if (data == null) return;
        runtimeData = data;

        if (cardNameText != null) cardNameText.text = data.cardName;
        if (costText != null) costText.text = data.cost.ToString();

        if (cardIllustration != null && data.cardArt != null)
        {
            cardIllustration.sprite = data.cardArt;
        }

        SetCardSide(true);
    }

    public void SetCardSide(bool isFront)
    {
        if (cardFrontObject != null) cardFrontObject.SetActive(isFront);
        if (cardBackObject != null) cardBackObject.SetActive(!isFront);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalRotation = rectTransform.localRotation;
        canvasGroup.blocksRaycasts = false;
        rectTransform.SetAsLastSibling();
        rectTransform.localRotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (rectTransform.anchoredPosition.y > playYThreshold)
        {
            int cardCost = runtimeData != null ? runtimeData.cost : 0;

            if (BattleManager.Instance != null && BattleManager.Instance.CanUseCard(cardCost))
            {
                if (cardManager != null)
                {
                    cardManager.UseCard(gameObject);
                    return;
                }
            }
        }

        rectTransform.position = originalPosition;
        rectTransform.localRotation = originalRotation;
    }
}