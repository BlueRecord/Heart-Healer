using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUIController : MonoBehaviour
{
    [Header("Target Stats Pointer")]
    [SerializeField] public EntityStats targetStats;

    // ★ [핵심 추가] 상단에 이름을 표시할 텍스트 컴포넌트 포트
    [Header("Name UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("HP UI Elements")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Armor UI Elements")]
    [SerializeField] private GameObject armorGroupObject;
    [SerializeField] private Slider armorSlider;
    [SerializeField] private TextMeshProUGUI armorText;

    [Header("Attack (Buff) UI Settings")]
    [SerializeField] private GameObject attackIconGroup;
    [SerializeField] private TextMeshProUGUI attackText;

    void Start()
    {
        if (targetStats == null)
        {
            targetStats = FindFirstObjectByType<PlayerStats>();
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (targetStats == null) return;

        int currentHp = targetStats.CurrentHp;
        int maxHp = targetStats.MaxHp;
        int currentArmor = targetStats.CurrentArmor;
        int currentAttack = targetStats.CurrentAttack;

        // ★ [핵심 추가] 타겟 오브젝트의 이름을 가져와 상단 텍스트에 반영합니다.
        if (nameText != null)
        {
            // 만약 몬스터이고 MonsterStats에 데이터가 연결되어 있다면 기획 이름을 쓰고, 
            // 그렇지 않다면 게임 오브젝트의 기본 이름을 가져옵니다.
            MonsterStats monsterStats = targetStats as MonsterStats;
            if (monsterStats != null && monsterStats.GetComponent<MonsterStats>() != null)
            {
                // MonsterStats 내부에 저장된 ScriptableObject의 이름을 가져오는 안전장치
                // 만약 아래 주석 처리된 구조처럼 데이터를 가져오고 싶다면 확장 가능합니다.
                nameText.text = targetStats.gameObject.name;
            }
            else
            {
                nameText.text = "플레이어"; // 플레이어 UI일 경우 고정 문자열 처리
            }
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHp} / {maxHp}";
        }

        // 방어도가 0보다 클 때만 그룹과 내부 슬라이더를 활성화
        if (armorGroupObject != null)
        {
            if (currentArmor > 0)
            {
                armorGroupObject.SetActive(true);

                if (armorSlider != null)
                {
                    armorSlider.gameObject.SetActive(true);
                    armorSlider.maxValue = maxHp;
                    armorSlider.value = Mathf.Min(maxHp, currentHp + currentArmor);
                }

                if (armorText != null)
                {
                    armorText.text = currentArmor.ToString();
                }
            }
            else
            {
                armorGroupObject.SetActive(false);
            }
        }

        // 공격력이 0보다 클 때만 버프 아이콘 그룹 활성화
        if (attackIconGroup != null)
        {
            if (currentAttack > 0)
            {
                attackIconGroup.SetActive(true);

                if (attackText != null)
                {
                    attackText.text = currentAttack.ToString();
                }
            }
            else
            {
                attackIconGroup.SetActive(false);
            }
        }
    }
}