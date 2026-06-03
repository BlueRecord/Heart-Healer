using UnityEngine;

public class PlayerStats : EntityStats
{
    void Start()
    {
        InitStats();
    }

    // BattleManager가 플레이어 턴을 닫을 때 자동으로 실행시켜 줍니다.
    public override void OnTurnEndProcess()
    {
        base.OnTurnEndProcess();
        ResetArmor();
    }

    public void ResetArmor()
    {
        if (armor > 0)
        {
            armor = 0;
            Debug.Log("<color=gray>[턴 종료 세정] 플레이어의 소모성 방어도가 소멸되어 0이 되었습니다.</color>");
            RefreshUI();
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("<color=red><b>[게임 오버] 플레이어가 탈진 상태에 빠졌습니다!</b></color>");
    }
}