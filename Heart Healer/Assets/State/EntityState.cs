using UnityEngine;

public class EntityState : MonoBehaviour
{
    [Header("HP Stats")]
    [SerializeField] protected int hp;
    [SerializeField] protected int maxHp = 70;


    [Header("armor")]
    [SerializeField] protected int armor;
    [SerializeField] protected int maxArmor = 0;

    protected void InitStats()
    {
        hp = maxHp;
        armor = maxArmor;
        Debug.Log($"{gameObject.name}의 초기화 성공");
    }
    public void TakeDamage(int rawDamage)
    {
        int finalDamage = Mathf.Max(0, rawDamage - armor);
        hp -= finalDamage;
        Debug.Log($"{gameObject.name}이 {finalDamage}의 피해를 입었다.");
    }

    public void TakePureDamage(int pureDamage)
    {
        hp -= pureDamage;
        Debug.Log($"{gameObject.name}이 {pureDamage}의 고정 피해를 입었다.");
    }

    public void GetArmor(int finalArmor)
    {
        armor += finalArmor;
        Debug.Log($"{gameObject.name}이 {finalArmor}의 방어도를 얻었다.");
    }

}
