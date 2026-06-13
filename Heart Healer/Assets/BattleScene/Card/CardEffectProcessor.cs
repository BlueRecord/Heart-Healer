using UnityEngine;

public class CardEffectProcessor : MonoBehaviour
{
    private CardManager cardManager;

    void Awake()
    {
        cardManager = GetComponent<CardManager>();
    }

    public void ExecuteEffect(CardData card, PlayerStats player, MonsterStats monster)
    {
        if (card == null || player == null || monster == null) return;

        // [공통 1] 기본 방어도 적용
        if (card.armor > 0)
        {
            player.GetArmor(card.armor);
        }

        // [공통 2] 기본 대미지 연산 (취약 상태 반영)
        if (card.damage > 0)
        {
            int finalDmg = player.CalculateOutputDamage(card.damage);
            monster.TakeDamage(finalDmg);
        }

        // [공통 3] 관통 대미지 연산
        if (card.puredamage > 0)
        {
            monster.TakePureDamage(card.puredamage);
        }

        // [ID 기반 특수 기믹 분기]
        switch (card.cardID)
        {
            case "ActiveListening": // 공감적 경청
                player.AddAttack(2);
                break;

            case "SilentObservation": // 침묵의 응시
                player.ReserveNextTurnDraw(2);
                break;

            case "CupOfTea": // 차 한 잔
                if (BattleManager.Instance != null)
                {
                    int remainMana = BattleManager.Instance.CurrentMana;
                    player.GetArmor(remainMana * 2);
                }
                break;

            case "OpenMind": // 마음 열기
                monster.ApplyVulnerable(card.vulnerableTurns);
                break;

            case "SharpInsight": // 정곡 찌르기
                monster.ResetArmorHardcoded();
                break;

            case "TopicDiversion": // 주제 돌리기
                monster.AddAttack(-3);
                break;

            case "DeepEmpathy": // 깊은 공감
                player.AddAttack(1);
                break;

            case "DeepBreath": // 심호흡
                player.IncreasePermanentArmorRegen(6);
                break;

            case "Catharsis": // 감정 정화
                if (monster.IsVulnerable)
                {
                    int bonusDmg = player.CalculateOutputDamage(card.damage);
                    monster.TakeDamage(bonusDmg); // 취약 시 추가 타격
                }
                break;

            case "PsychologicalBreakthrough": // 심리적 돌파구
                if (cardManager != null)
                {
                    cardManager.DiscardHand();
                    cardManager.DrawSpecificAmount(3);
                }
                break;

            default:
                // 벌크업, 영양제 등 이름 기반 기존 연동 처리는 CardManager의 예외 처리단에서 커버 가능합니다.
                break;
        }

        // 즉시 드로우 카운트가 지정된 경우 처리
        if (card.drawCount > 0 && card.cardID != "PsychologicalBreakthrough")
        {
            if (cardManager != null) cardManager.DrawSpecificAmount(card.drawCount);
        }
    }
}