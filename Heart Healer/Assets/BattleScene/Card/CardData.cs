using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/BasicCard")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public int damage;
    public int puredamage;
    public int armorAmount;
    [TextArea] public string baseDescription;

    // 카드마다 들어갈 일러스트 스프라이트 변수
    public Sprite cardGraphic;

    public string GetDynamicDescription()
    {
        string desc = baseDescription;
        if (string.IsNullOrEmpty(desc)) return "";

        desc = desc.Replace("[cost]", cost.ToString());
        desc = desc.Replace("[dmg]", damage.ToString());
        desc = desc.Replace("[Pdmg]", puredamage.ToString());
        desc = desc.Replace("[arm]", armorAmount.ToString());
        return desc;
    }
}