using UnityEngine;
using UnityEngine.UI; // UI 시스템 명시
using TMPro;

public class CardUIPopVer : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    // 모호성 차단을 위해 UnityEngine.UI 타입을 명확히 지정합니다.
    [SerializeField] private UnityEngine.UI.Image cardIllustration;
    [SerializeField] private UnityEngine.UI.Image cardBackground;

    public void RefreshUI(CardData data)
    {
        if (data == null) return;

        // 1. 이름 및 코스트 반영
        if (cardNameText != null) cardNameText.text = data.cardName;

        // [CS0103 해결] missing 변수였던 costText를 정상 참조하도록 수정합니다.
        if (costText != null) costText.text = data.cost.ToString();

        // 2. 설명문 조립 (팝업창은 플레이어의 버프 상태와 관계없이 항상 기본 고유값만 출력)
        if (descriptionText != null)
        {
            string desc = "";

            if (!string.IsNullOrEmpty(data.baseDescription))
            {
                // 버프값 0, 취약 유무 false를 강제 전달하여 깡스탯만 보여줍니다.
                desc = data.GetDynamicDescription(0, false);
            }
            else
            {
                if (data.damage > 0) desc += $"공격력 {data.damage} 부여.\n";
                if (data.puredamage > 0) desc += $"관통 피해 {data.puredamage} 부여.\n";
                if (data.armor > 0) desc += $"방어도 {data.armor} 획득.\n";

                if (!string.IsNullOrEmpty(data.cardName) && (data.cardName.Contains("벌크업") || data.cardName.Contains("영양제")))
                {
                    desc += "공격력 +2 영구 증가.";
                }
            }

            descriptionText.text = desc;
        }

        // 3. 이미지 바인딩
        if (cardIllustration != null && data.cardArt != null)
        {
            cardIllustration.sprite = data.cardArt;
        }
    }
}