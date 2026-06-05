using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // UI 이벤트 감지용 필수 라이브러리

public class CardUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI cardNameText;    // 카드 이름 텍스트
    [SerializeField] private TextMeshProUGUI costText;        // 마나 코스트 텍스트
    [SerializeField] private TextMeshProUGUI descriptionText; // 카드 설명 텍스트
    [SerializeField] private Image cardIllustration;          // 카드 일러스트 이미지

    [Header("Card Visual Sub-Objects")]
    [SerializeField] private GameObject cardFrontObject;      // 카드 앞면 오브젝트
    [SerializeField] private GameObject cardBackObject;       // 카드 뒷면 오브젝트

    [Header("Play Settings (사용 판정)")]
    [SerializeField] private float playYThreshold = 150f;     // UI 픽셀 좌표 기준 사용 인식을 위한 Y축 높이

    private Vector3 originalPosition;
    private Quaternion originalRotation; // 기존 회전값을 저장할 변수
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [HideInInspector] public CardData runtimeData; //
    private CardManager cardManager;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        cardManager = FindFirstObjectByType<CardManager>();
    }

    // ★ [복구] CardManager가 카드를 생성한 직후 데이터를 UI 컴포넌트에 바인딩하는 함수
    public void SetCardData(CardData data)
    {
        if (data == null) return;
        runtimeData = data;

        // 1. 이름 반영
        if (cardNameText != null) cardNameText.text = data.cardName;

        // 2. 코스트 반영
        if (costText != null) costText.text = data.cost.ToString();

        // 3. 동적 설명문 반영 (CardData에 정의된 GetDynamicDescription 활용)
        if (descriptionText != null) descriptionText.text = data.GetDynamicDescription();

        // 4. 카드 일러스트 이미지 반영
        if (cardIllustration != null && data.cardGraphic != null)
        {
            cardIllustration.sprite = data.cardGraphic;
        }

        // 기본적으로 앞면을 보여줍니다.
        ShowFront(true);
    }

    // ★ 앞뒷면 활성화/비활성화 제어 함수
    public void ShowFront(bool isFront)
    {
        if (cardFrontObject != null) cardFrontObject.SetActive(isFront);
        if (cardBackObject != null) cardBackObject.SetActive(!isFront);
    }

    // 드래그 전 위치와 회전값 저장
    public void OnPointerDown(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalRotation = rectTransform.localRotation; // 정렬되었던 회전값 기억

        canvasGroup.blocksRaycasts = false;
        rectTransform.SetAsLastSibling(); // 드래그하는 카드를 맨 앞으로 배치

        rectTransform.localRotation = Quaternion.identity; // 드래그 중에는 똑바로 세움
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position; // 마우스 커서 추적
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
            else
            {
                Debug.Log("<color=red><b>[시스템] 마나가 부족하여 카드를 사용할 수 없습니다!</b></color>");
            }
        }

        // 사용 실패 시 원래 정렬되었던 자리와 각도로 원상복구
        rectTransform.position = originalPosition;
        rectTransform.localRotation = originalRotation;
    }
}