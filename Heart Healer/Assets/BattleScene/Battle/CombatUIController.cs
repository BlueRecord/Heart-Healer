using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUIController : MonoBehaviour
{
    [Header("Target Stats Pointer")]
    [SerializeField] public EntityStats targetStats;

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
                    armorSlider.gameObject.SetActive(true); // 자식 슬라이더 강제 활성화
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