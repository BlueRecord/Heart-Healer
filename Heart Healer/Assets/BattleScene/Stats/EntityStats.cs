using UnityEngine;

public class EntityStats : MonoBehaviour
{
    [Header("HP Stats")]
    [SerializeField] protected int hp;
    [SerializeField] protected int maxHp = 70;

    [Header("Armor (Shield) Stats")]
    [SerializeField] protected int armor;
    [SerializeField] protected int maxArmor = 0;

    // ★ [중요] 공격력(위력) 기본 스탯 필드
    [Header("Attack Stats")]
    [SerializeField] protected int baseAttack = 0;

    [Header("UI Reference")]
    [SerializeField] protected CombatUIController uiController;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
    public int CurrentArmor => armor;
    public int CurrentAttack => baseAttack; // 외부에 현재 공격력을 리턴하는 Getter

    protected void InitStats()
    {
        hp = maxHp;
        armor = maxArmor;
        Debug.Log($"<color=white>[초기화] {gameObject.name} (HP: {hp}/{maxHp}, 공격력: {baseAttack})</color>");
        RefreshUI();
    }

    protected void RefreshUI()
    {
        if (uiController != null)
        {
            uiController.UpdateUI();
        }
    }

    // 벌크업 카드나 몬스터 성장 기믹 작동 시 공격력을 올리는 함수
    public void AddAttack(int amount)
    {
        baseAttack += amount;
        Debug.Log($"<color=yellow>[버프] {gameObject.name}의 공격력이 {amount} 증가했습니다! 현재 공격력: {baseAttack}</color>");
        RefreshUI(); // ★ 공격력이 바뀌는 순간 화면 UI 자동 동기화
    }

    // 카드 고유 데미지에 내 현재 공격력 버프를 융합하여 최종 대미지 연산
    public int CalculateOutputDamage(int cardBaseDamage)
    {
        return Mathf.Max(0, cardBaseDamage + baseAttack);
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

        Debug.Log($"<color=orange>[전투] {gameObject.name}이(가) {rawDamage}의 피해를 받음 -> 남은 HP: {hp}</color>");
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
        // 자식 클래스에서 오버라이드하여 턴 정산 기믹 처리
    }

    // MonsterStats.cs 내부에 추가
    public void ResetArmorHardcoded()
    {
        this.armor = 0; // 부모의 armor 변수에 직접 0을 대입
        RefreshUI();    // 변경된 0 수치를 UI에 즉시 반영
        Debug.Log($"<color=gray>[턴 정산] {gameObject.name}의 방어도가 0으로 초기화되었습니다.</color>");
    }

    protected virtual void Die()
    {
        Debug.Log($"<color=black><b>[사망] {gameObject.name}이 탈진했습니다.</b></color>");
    }
}