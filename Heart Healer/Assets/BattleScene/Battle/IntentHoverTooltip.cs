using UnityEngine;
using UnityEngine.EventSystems;

// 마우스 호버 이벤트를 받기 위해 유니티 UI 인터페이스를 상속받습니다.
public class IntentHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel; // 마우스를 올리면 켜질 팝업 판넬 오브젝트

    void Start()
    {
        // 게임 시작 시에는 툴팁 판넬을 기본적으로 숨겨둡니다.
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    // 1. 마우스가 이 오브젝트(행동 예고 텍스트/아이콘) 영역 안으로 들어왔을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true); // 팝업 켜기
            Debug.Log("[툴팁] 마우스 호버: 상세 기믹 설명창을 표시합니다.");
        }
    }

    // 2. 마우스가 오브젝트 영역 밖으로 나갔을 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false); // 팝업 끄기
            Debug.Log("[툴팁] 마우스 아웃: 상세 기믹 설명창을 숨깁니다.");
        }
    }
}