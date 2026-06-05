using System.Collections.Generic;
using UnityEngine;
using System.Reflection; // 변수명을 자동으로 추적하기 위한 라이브러리

public class OpenGraveAndDeck : MonoBehaviour
{
    [Header("UI Panels & Content")]
    [SerializeField] private GameObject cardListPopupPanel; // CardListPopUpPanel 등록
    [SerializeField] private Transform scrollViewContent;   // Scroll View -> Viewport -> Content 등록

    [Header("Card Properties")]
    [SerializeField] private GameObject cardPrefab;          // 원본 카드 프리팹(CardBase) 등록

    private CardManager cardManager;

    void Start()
    {
        cardManager = FindFirstObjectByType<CardManager>(); //
    }

    public void OpenCardListPopup(bool isDeck)
    {
        // 1. 컴포넌트 연결 예외 처리
        if (cardListPopupPanel == null || scrollViewContent == null || cardPrefab == null) //
        {
            Debug.LogError("<color=red><b>[오류] 인스펙터 세팅(Panel, Content, Prefab) 중 연결 안 된 빈칸이 있습니다!</b></color>"); //
            return; //
        }

        if (cardManager == null) //
        {
            cardManager = FindFirstObjectByType<CardManager>(); //
            if (cardManager == null) //
            {
                Debug.LogError("<color=red><b>[오류] 맵에 CardManager 오브젝트가 존재하지 않습니다!</b></color>"); //
                return; //
            }
        }

        // ★ [추가] 새 카드를 그리기 전, 팝업창 안에 남아있던 옛날 카드 오브젝트들을 깨끗하게 청소합니다.
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // 2. 실시간 남은 덱(runtimeDeck)과 무덤(grave)을 리플렉션으로 안전하게 가져옵니다.
        string targetFieldName = isDeck ? "runtimeDeck" : "grave";
        List<CardData> targetList = GetCardListViaReflection(targetFieldName);

        if (targetList == null)
        {
            Debug.LogError($"<color=red><b>[오류] CardManager에서 '{targetFieldName}' 리스트 데이터를 찾을 수 없습니다!</b></color>");
            return;
        }

        // 3. 카드 생성 루프
        foreach (CardData data in targetList) //
        {
            if (data == null) continue; //

            GameObject newCard = Instantiate(cardPrefab, scrollViewContent); //

            RectTransform rect = newCard.GetComponent<RectTransform>(); //
            if (rect != null) //
            {
                rect.anchoredPosition = Vector2.zero; //
                rect.localScale = Vector3.one;       //
                rect.localRotation = Quaternion.identity; // 부채꼴 회전 리셋
            }

            // ★ [정상 복구] 사용자님의 원래 컴포넌트인 CardUIPopVer를 그대로 사용하여 유실 없이 텍스트를 채웁니다!
            CardUIPopVer ui = newCard.GetComponent<CardUIPopVer>(); //
            if (ui != null) //
            {
                ui.RefreshUI(data); //
            }
        }

        // ★ [추가] 팝업창이 열리는 동안 뒤에 깔린 손패 카드가 드래그되는 것을 막기 위해 잠금 처리
        if (cardManager != null)
        {
            cardManager.SetHandInteractable(false);
        }

        cardListPopupPanel.SetActive(true);
        Debug.Log($"<color=lime><b>[성공] {(isDeck ? "남은 덱" : "무덤")} 리스트 {targetList.Count}장 화면 표시 완료!</b></color>");
    }

    // CardManager 내부의 비공개/공개 리스트를 안전하게 읽어오는 함수
    private List<CardData> GetCardListViaReflection(string fieldName) //
    {
        if (cardManager == null) return null; //

        FieldInfo field = cardManager.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); //
        if (field != null) //
        {
            return field.GetValue(cardManager) as List<CardData>; //
        }
        return null; //
    }

    // ★ [추가/수정] 팝업창을 닫을 때 손패 잠금을 정상적으로 해제해 줍니다.
    public void CloseCardListPopup()
    {
        if (cardListPopupPanel != null)
        {
            cardListPopupPanel.SetActive(false);
        }

        if (cardManager == null) cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.SetHandInteractable(true); // 손패 다시 드래그 가능하게 잠금 해제
        }
    }
}