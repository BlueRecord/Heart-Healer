using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 사용하기 위해 추가

public class PlayerStats : EntityStats
{
    [Header("Player Graphic UI Port")]
    [SerializeField] private Image playerGraphicImage; // 씬에 있는 플레이어 이미지 컴포넌트 포트

    [Header("Player Sprite Resources")]
    [SerializeField] private Sprite normalSprite;      // 전투 중 (기본) 이미지 포트
    [SerializeField] private Sprite defeatedSprite;    // 전투 패배 (탈진) 이미지 포트

    void Start()
    {
        // 1. 프리팹 동적 소환 시 Hierarchy에 있는 'PlayerUI'를 이름으로 정확히 찾아 연결합니다.
        if (uiController == null)
        {
            GameObject playerUIObj = GameObject.Find("PlayerUI");
            if (playerUIObj != null)
            {
                uiController = playerUIObj.GetComponent<CombatUIController>();
            }
            else
            {
                uiController = FindFirstObjectByType<CombatUIController>();
            }
        }

        // 찾은 플레이어 UI 컨트롤러에게 타겟 설정 주입
        if (uiController != null)
        {
            uiController.targetStats = this;
        }

        // 게임 시작 시 초기 이미지셋을 '전투 중(기본)' 이미지로 강제 고정합니다.
        if (playerGraphicImage != null && normalSprite != null)
        {
            playerGraphicImage.sprite = normalSprite;
        }

        InitStats(); // 부모(EntityStats)의 스탯(HP 만땅 등) 초기화 수행
        RefreshUI();
    }

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
            Debug.Log("<color=gray>[턴 종료] 플레이어의 일회성 안정감(방어)이 소멸되었습니다.</color>");
            RefreshUI();
        }
    }

    // 플레이어 패배(탈진) 상태로 전환될 때 호출되는 부모 오버라이드 함수
    protected override void Die()
    {
        base.Die(); // 부모의 기본 탈진 로그 출력
        Debug.Log("<color=red><b>[게임 오버] 플레이어가 탈진 상태에 빠졌습니다!</b></color>");

        if (playerGraphicImage != null && defeatedSprite != null)
        {
            playerGraphicImage.sprite = defeatedSprite;
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.HandlePlayerDefeat();
        }
    }
}