using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI Components (TextMeshPro)")]
    [SerializeField] private TextMeshPro costText;        
    [SerializeField] private TextMeshPro nameText;        
    [SerializeField] private TextMeshPro descriptionText; 
    [SerializeField] private SpriteRenderer artworkImage; 

    [Header("Card Visual Sub-Objects")]
    [SerializeField] private GameObject cardFrontObject;      
    [SerializeField] private GameObject cardBackObject;       

    [Header("Hover Animation Settings")]
    [SerializeField] private float hoverYOffset = 1.5f;       
    [SerializeField] private float hoverScale = 1.2f;         

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private int originalSortingOrder;

    private bool isHovered = false;
    private bool isDragging = false; 

    private Vector3 dragOffset;

    // ★ 꼬임 해결: 존재하지 않는 스크립트 타입 대신 순수 데이터를 직접 바라보게 변경
    [HideInInspector] public CardData runtimeData; 
    private CardManager cardManager;
    private UnityEngine.Rendering.SortingGroup sortingGroup;
    private Camera mainCamera;

    void Awake()
    {
        sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
        mainCamera = Camera.main;
    }

    // ★ CardManager가 호출하는 함수명을 완전히 일치하도록 정돈
    public void RefreshUI(CardData data, CardManager manager)
    {
        this.cardManager = manager;
        this.runtimeData = data; // 데이터 캐싱

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.cost.ToString();
        if (descriptionText != null) descriptionText.text = data.GetDynamicDescription();
        if (artworkImage != null && data.cardGraphic != null) artworkImage.sprite = data.cardGraphic;
        
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
    [SerializeField] private float playYThreshold = -1.0f; 

    private void OnMouseUp()
    {
        isDragging = false;
        isHovered = false;

        if (transform.position.y > playYThreshold)
        {
            // 수정된 runtimeData 마킹 구조에서 코스트 추출
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

        RestoreOriginalState();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}