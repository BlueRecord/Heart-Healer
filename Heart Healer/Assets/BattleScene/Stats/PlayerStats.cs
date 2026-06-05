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

    // PlayerStats 스크립트 내부에 이 함수를 새로 하나 선언해 주세요.
    public void ResetArmorAtTurnEnd()
    {
        // 현재 방어도를 저장하는 변수명(예: currentArmor 등)에 맞춰 0을 대입합니다.
        armor = 0;

        // 방어도 UI를 새로고침하는 코드가 내부에 있다면 여기서 함께 호출해 줍니다.
        // 예: UpdateArmorUI();
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("<color=red><b>[게임 오버] 플레이어가 탈진 상태에 빠졌습니다!</b></color>");
    }
}