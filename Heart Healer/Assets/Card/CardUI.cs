using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ TextMeshPro 기능을 쓰기 위해 반드시 필요한 네임스페이스

public class CardUI : MonoBehaviour
{
    [Header("UI Components (TextMeshPro)")]
    // 일반 Text에서 TextMeshProUGUI로 타입을 다시 변경했습니다.
    [SerializeField] private TextMeshPro costText;        // 좌측 상단 코스트
    [SerializeField] private TextMeshPro nameText;        // 상단 중앙 카드 이름
    [SerializeField] private TextMeshPro descriptionText; // 하단 카드 설명
    [SerializeField] private UnityEngine.SpriteRenderer artworkImage; // Image에서 SpriteRenderer로 교체!            // 중앙 카드 일러스트

    [Header("Card Visual Sub-Objects")]
    [SerializeField] private GameObject cardFrontObject;      // 앞면 오브젝트 (부모)
    [SerializeField] private GameObject cardBackObject;       // 뒷면 오브젝트 (부모)

    [Header("Hover Animation Settings")]
    [SerializeField] private float hoverYOffset = 1.5f;       // 마우스 올렸을 때 위로 솟구칠 높이
    [SerializeField] private float hoverScale = 1.2f;         // 마우스 올렸을 때 확대될 배율

    // 원래 상태를 기억하기 위한 변수들
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private int originalSortingOrder;

    private bool isHovered = false;
    private bool isDragging = false; // 현재 드래그 중인지 여부

    // 드래그 시 마우스 커서와 카드 중심점 사이의 거리 오프셋
    private Vector3 dragOffset;

    // 컴포넌트 참조
    private Card cardLogic;
    private UnityEngine.Rendering.SortingGroup sortingGroup;
    private Camera mainCamera;

    void Awake()
    {
        cardLogic = GetComponent<Card>();
        sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
        mainCamera = Camera.main; // 메인 카메라 캐싱
    }

    /// <summary>
    /// 카드 데이터를 받아 UI 텍스트(TMP) 및 이미지를 셋업합니다.
    /// </summary>
    // 캐싱용 멤버 변수 추가
    private CardManager cardManager;

    // Setup 함수에서 CardManager 주소를 넘겨받도록 수정
    public void Setup(CardData data, CardManager manager)
    {
        this.cardManager = manager; // 매니저 주소 저장 (Find 함수가 필요 없어짐)

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.cost.ToString();
        if (descriptionText != null) descriptionText.text = data.GetDynamicDescription();
        if (artworkImage != null && data.cardGraphic != null) artworkImage.sprite = data.cardGraphic;
        if (cardLogic != null) cardLogic.data = data;
        SetCardFace(true);
    }

    public void SetCardFace(bool isFront)
    {
        if (cardFrontObject != null) cardFrontObject.SetActive(isFront);
        if (cardBackObject != null) cardBackObject.SetActive(!isFront);
    }

    private void OnMouseEnter()
    {
        if (isHovered || isDragging) return;
        isHovered = true;

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        if (sortingGroup != null) originalSortingOrder = sortingGroup.sortingOrder;

        Vector3 targetPos = originalPosition + new Vector3(0f, hoverYOffset, 0f);
        transform.position = targetPos;
        transform.rotation = Quaternion.identity;
        transform.localScale = originalScale * hoverScale;

        if (sortingGroup != null) sortingGroup.sortingOrder = 100;
    }

    private void OnMouseExit()
    {
        if (!isHovered || isDragging) return;
        isHovered = false;

        RestoreOriginalState();
    }

    private void RestoreOriginalState()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (sortingGroup != null) sortingGroup.sortingOrder = originalSortingOrder;
    }

    private void OnMouseDown()
    {
        isDragging = true;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
        dragOffset.z = 0f;

        if (sortingGroup != null) sortingGroup.sortingOrder = 200;
    }

    private void OnMouseDrag()
    {
        Vector3 currentMousePos = GetMouseWorldPosition();
        transform.position = currentMousePos + dragOffset;
    }

    [Header("Play Settings (사용 판정)")]
    [SerializeField] private float playYThreshold = -1.0f; // 이 Y 좌표보다 위에서 마우스를 놓으면 카드가 사용됩니다.


    private void OnMouseUp()
    {
        isDragging = false;
        isHovered = false;

        if (transform.position.y > playYThreshold)
        {
            // 이제 Find를 쓰지 않고 미리 들고 있던 주소로 바로 호출합니다.
            if (cardManager != null)
            {
                cardManager.UseCard(gameObject);
                return;
            }
        }
        RestoreOriginalState();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}