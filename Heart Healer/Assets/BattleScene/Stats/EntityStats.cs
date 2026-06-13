using UnityEngine;

public class EntityStats : MonoBehaviour
{
    [Header("HP Stats")]
    [SerializeField] protected int hp;
    [SerializeField] protected int maxHp = 70;

    [Header("Armor (Shield) Stats")]
    [SerializeField] protected int armor;
    [SerializeField] protected int maxArmor = 0;

    [Header("Attack Stats")]
    [SerializeField] public int baseAttack = 0; // 공격력 버프

    [Header("Advanced States System")]
    [SerializeField] public int vulnerableTurns = 0; // 취약(마음 열림) 지속 턴
    [SerializeField] protected int nextTurnBonusDraw = 0; // 다음 턴 추가 드로우 예약
    [SerializeField] protected int permanentArmorRegen = 0; // 매 턴 방어도 생성력

    [Header("UI Reference")]
    [SerializeField] protected CombatUIController uiController;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
    public int CurrentArmor => armor;
    public int CurrentAttack => baseAttack;

    public bool IsVulnerable => vulnerableTurns > 0;
    public int NextTurnBonusDraw => nextTurnBonusDraw;

    protected void InitStats()
    {
        hp = maxHp;
        armor = maxArmor;
        vulnerableTurns = 0;
        nextTurnBonusDraw = 0;
        permanentArmorRegen = 0;
        RefreshUI();
    }

    protected void RefreshUI()
    {
        if (uiController != null)
        {
            uiController.UpdateUI();
        }
    }

    public void AddAttack(int amount)
    {
        baseAttack += amount;
        RefreshUI();
    }

    public int CalculateOutputDamage(int cardBaseDamage)
    {
        int finalDmg = Mathf.Max(0, cardBaseDamage + baseAttack);
        if (IsVulnerable)
        {
            finalDmg = Mathf.RoundToInt(finalDmg * 1.5f); // 취약 시 1.5배 대미지
        }
        return finalDmg;
    }

    public void ApplyVulnerable(int turns)
    {
        if (turns <= 0) return;
        vulnerableTurns += turns;
        RefreshUI();
    }

    public void ReserveNextTurnDraw(int amount)
    {
        nextTurnBonusDraw += amount;
    }

    public void IncreasePermanentArmorRegen(int amount)
    {
        permanentArmorRegen += amount;
    }

    public void TakeDamage(int rawDamage)
    {
        if (rawDamage <= 0) return;
        int finalDamage = rawDamage;

        if (armor > 0)
        {
            if (armor >= finalDamage)
            {
                armor -= finalDamage;
                finalDamage = 0;
            }
            else
            {
                finalDamage -= armor;
                armor = 0;
            }
        }

        if (finalDamage > 0)
        {
            hp -= finalDamage;
            if (hp < 0) hp = 0;
        }

        RefreshUI();
        if (hp <= 0) Die();
    }

    public void TakePureDamage(int pureDamage)
    {
        if (pureDamage <= 0) return;
        hp -= pureDamage;
        if (hp < 0) hp = 0;
        RefreshUI();
        if (hp <= 0) Die();
    }

    public void GetArmor(int finalArmor)
    {
        if (finalArmor <= 0) return;
        armor += finalArmor;
        RefreshUI();
    }

    public virtual void OnTurnEndProcess()
    {
        if (vulnerableTurns > 0)
        {
            vulnerableTurns--;
            RefreshUI();
        }
    }

    public void ProcessStartTurnRegen()
    {
        if (permanentArmorRegen > 0)
        {
            GetArmor(permanentArmorRegen);
        }
    }

    public void ClearNextTurnDraw()
    {
        nextTurnBonusDraw = 0;
    }

    public void ResetArmorHardcoded()
    {
        this.armor = 0;
        RefreshUI();
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name}이 탈진했습니다.");
    }
}