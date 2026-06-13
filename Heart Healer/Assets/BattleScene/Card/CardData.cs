using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardID;
    public string cardName;
    public int cost;
    public Sprite cardArt;

    [Header("전투 수치 설정")]
    public int damage;             // 기본 장벽 제거(공격력) 수치
    public int puredamage;
    public int armor;
    public int vulnerableTurns;
    public int drawCount;

    [Header("설명문 (대괄호 치환 가능)")]
    [TextArea(3, 5)]
    public string baseDescription;

    public string GetDynamicDescription(int playerAttackBuff, bool isMonsterVulnerable)
    {
        if (string.IsNullOrEmpty(baseDescription)) return "";

        string parsedText = baseDescription;

        int finalDamage = damage;
        if (damage > 0)
        {
            // 기본 데미지 + 플레이어 공격력 버프 합산
            finalDamage += playerAttackBuff;

            // 몬스터가 취약 상태라면 1.5배 배율 적용
            if (isMonsterVulnerable)
            {
                finalDamage = Mathf.FloorToInt(finalDamage * 1.5f);
            }
        }

        // ★ [가독성 개선] 순수 베이스 값과 달라진 상태(버프 혹은 취약 활성화)일 때만 색상으로 강조 표시
        if ((playerAttackBuff > 0 || isMonsterVulnerable) && damage > 0)
        {
            parsedText = parsedText.Replace("[dmg]", $"<color=#FFD700><b>{finalDamage}</b></color>");
        }
        else
        {
            parsedText = parsedText.Replace("[dmg]", finalDamage.ToString());
        }

        // 방어도는 기본 수치 치환
        parsedText = parsedText.Replace("[arm]", armor.ToString());

        return parsedText;
    }
}