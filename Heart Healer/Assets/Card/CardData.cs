using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/BasicCard")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public int damage;
    public int puredamage;
    public int armorAmount;
    public string baseDescription;

    //카드마다 들어갈 일러스트 스프라이트 변수
    public Sprite cardGraphic;

    public string GetDynamicDescription()
    {
        // ... 기존 코드 유지
        string desc = baseDescription;
        desc = desc.Replace("[cost]", cost.ToString());
        desc = desc.Replace("[dmg]", damage.ToString());
        desc = desc.Replace("[Pdmg]", puredamage.ToString());
        desc = desc.Replace("[arm]", armorAmount.ToString());
        return desc;
    }
}