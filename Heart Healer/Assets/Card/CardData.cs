using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/BasicCard")]
public class CardData : ScriptableObject
{
    public string cardName; //카드 이름
    public int cost;       //카드 코스트
    public int damage;     //공격력(일반)
    public int puredamage;//공격력(퓨어)
    public int armorAmount;//방어 획득량
    public string baseDescription;//카드 설명
    public string GetDynamicDescription()
    {
        string desc = baseDescription;
        desc = desc.Replace("[cost]", cost.ToString());
        desc = desc.Replace("[dmg]", damage.ToString());
        desc = desc.Replace("[Pdmg]", puredamage.ToString());
        desc = desc.Replace("[arm]", armorAmount.ToString());
        return desc;
    }
}
