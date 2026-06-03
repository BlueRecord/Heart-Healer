using UnityEngine;

public class MonsterStats : EntityStats
{
    void Start()
    {
        InitStats();
    }

    // BattleManager가 적 행동을 처리하기 직전에 자동으로 호출해 줍니다.
    /*public override void OnTurnEndProcess()
    {
        base.OnTurnEndProcess();

        // 기획 규칙: 몬스터는 매 턴이 끝날 때마다 위력(공격력)이 2씩 강해지는 시한폭탄 기믹
        AddAttack(2);
    }*/

    protected override void Die()
    {
        base.Die();
        Debug.Log("<color=green><b>[전투 승리] 몬스터의 상처가 모두 치유되어 성불했습니다!</b></color>");
    }
}