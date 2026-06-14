using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수 포함
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("UI Templates")]
    [SerializeField] private GameObject rewardPanel;        // 껐다 켤 보상 판넬
    [SerializeField] private Button[] rewardButtons;        // 보상 버튼 3개
    [SerializeField] private TMP_Text[] buttonTexts;       // 버튼 안의 TMP 텍스트 3개

    [Header("Card Database")]
    [SerializeField] private List<CardData> allCardDatabase; // 프로젝트의 모든 카드 데이터 목록

    private List<CardData> selectedRewardCards = new List<CardData>();

    private void Awake()
    {
        Instance = this;
    }

    // 전투에서 승리했을 때 호출할 함수 (MonsterStats 등에서 호출됨)
    public void ShowBattleReward()
    {
        if (rewardPanel == null || allCardDatabase == null || allCardDatabase.Count < 3)
        {
            Debug.LogError("보상 시스템 세팅이 누락되었거나 카드 데이터베이스가 부족합니다.");
            return;
        }

        selectedRewardCards.Clear();
        rewardPanel.SetActive(true); // 팝업창 켜기

        // 1. 전체 카드 풀에서 랜덤으로 중복 없이 3장 선택
        List<CardData> tempPool = new List<CardData>(allCardDatabase);
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            selectedRewardCards.Add(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex); // 중복 방지
        }

        // 2. 선택된 3장의 카드를 버튼 UI에 주입 및 온클릭 이벤트 리스너 연결
        for (int i = 0; i < 3; i++)
        {
            int index = i; // 람다 캡처 이슈 방지용 임시 변수
            CardData card = selectedRewardCards[index];

            // 버튼 텍스트 변경 (카드 이름과 간단한 설명 연출)
            if (buttonTexts[index] != null)
            {
                buttonTexts[index].text = $"<b>{card.cardName}</b>\n\n{card.baseDescription}";
            }

            // 버튼 이벤트 초기화 후 새 기능 할당
            rewardButtons[index].onClick.RemoveAllListeners();
            rewardButtons[index].onClick.AddListener(() => OnClickSelectReward(card));
        }
    }

    // 카드를 선택했을 때 실행되는 핵심 로직
    private void OnClickSelectReward(CardData chosenCard)
    {
        if (chosenCard == null) return;

        Debug.Log($"보상 선택 완료: {chosenCard.cardName} 카드를 덱에 추가합니다.");

        // 🛠️ [핵심 수정] DeckManager의 PlayerDeck 리스트에 선택한 카드를 추가합니다.
        if (DeckManager.Instance != null && DeckManager.Instance.PlayerDeck != null)
        {
            DeckManager.Instance.PlayerDeck.Add(chosenCard);
            Debug.Log($"[RewardManager] DeckManager에 {chosenCard.cardName} 추가 완료. 현재 덱 크기: {DeckManager.Instance.PlayerDeck.Count}");
        }
        else
        {
            Debug.LogError("[RewardManager] DeckManager 또는 PlayerDeck 리스트가 씬에 존재하지 않거나 Null입니다!");
        }

        // 보상 판넬을 닫습니다.
        rewardPanel.SetActive(false);

        // 안전하게 스테이지 선택 지도 씬으로 이동시킵니다.
        Debug.Log("[RewardManager] 스테이지 선택 지도로 복귀합니다.");
        SceneManager.LoadScene("Stage");
    }

    // 만약 보상창에 "건너뛰기(Skip)" 버튼이 따로 있다면 연결해 줄 예외 메서드
    public void OnClickSkipReward()
    {
        rewardPanel.SetActive(false);
        SceneManager.LoadScene("Stage");
    }
}