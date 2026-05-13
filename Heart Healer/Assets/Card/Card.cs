using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData data;
    public void Use(EntityState target, PlayerState player)
    {
        //코스트 체크 및 소모
        if(player.CompareCost(data.cost)) return ;
        player.UsingCost(data.cost);

        //데미지
        if(data.damage > 0) target.TakeDamage(data.damage);
        if (data.puredamage > 0) target.TakePureDamage(data.puredamage);

        //방어도
        if (data.armorAmount > 0) player.GetArmor(data.armorAmount);
    }
}
