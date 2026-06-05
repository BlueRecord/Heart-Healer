using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUIPopVer : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI cardNameText;    // 카드 이름
    [SerializeField] private TextMeshProUGUI costText;        // 마나 코스트
    [SerializeField] private TextMeshProUGUI descriptionText; // 카드 설명
    [SerializeField] private Image cardIllustration;          // UI Image 컴포넌트
    [SerializeField] private Image cardBackground;            // 카드 배경 UI Image

    public void RefreshUI(CardData data)
    {
        if (data == null) return;

        // 1. 이름 반영
        if (cardNameText != null) cardNameText.text = data.cardName;

        // 2. 코스트 반영
        if (costText != null) costText.text = data.cost.ToString();

        // 3. 설명문 자동 조립
        if (descriptionText != null)
        {
            string desc = "";
            if (data.damage > 0) desc += $"공격력 {data.damage} 부여.\n";
            if (data.puredamage > 0) desc += $"관통 피해 {data.puredamage} 부여.\n";
            if (data.armorAmount > 0) desc += $"방어도 {data.armorAmount} 획득.\n";

            if (!string.IsNullOrEmpty(data.cardName) && (data.cardName.Contains("벌크업") || data.cardName.Contains("영양제")))
            {
                desc += "공격력 +2 영구 증가.";
            }
            descriptionText.text = desc;
        }

        // 4. [★ 에러 해결 핵심] CardData 내부의 실제 이미지 변수명 찾기
        if (cardIllustration != null)
        {
            // 방법 A: 만약 CardData 내부 이미지 변수명이 'sprite'일 경우
            // cardIllustration.sprite = data.sprite; 

            // 방법 B: 만약 CardData 내부 이미지 변수명이 'cardSprite'일 경우
            // cardIllustration.sprite = data.cardSprite;

            // -------------------------------------------------------------
            // ※ 우선 에러를 완전히 지우기 위해 아래처럼 주석 처리를 하거나, 
            // 원래 쓰고 계신 CardData의 이미지 변수명을 알아내어 대입해 주세요!
            // -------------------------------------------------------------
            Debug.Log("[안내] CardData의 이미지 변수명을 확인하여 매핑해 주세요.");
        }
    }
}