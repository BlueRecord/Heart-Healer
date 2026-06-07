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
        // 프리팹 동적 소환 시 플레이어 UI를 자동으로 찾아 연결합니다.
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

        // 게임 시작 시 초기 이미지셋을 '전투 중(기본)' 이미지로 강제 고정합니다.
        if (playerGraphicImage != null && normalSprite != null)
        {
            playerGraphicImage.sprite = normalSprite;
        }

        InitStats(); // 부모(EntityStats)의 스탯 및 UI 초기화
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
            Debug.Log("<color=gray>[턴 종료 세정] 플레이어의 소모성 방어도가 소멸되어 0이 되었습니다.</color>");
            RefreshUI();
        }
    }

    // 플레이어 패배(탈진) 상태로 전환될 때 호출되는 부모 오버라이드 함수
    protected override void Die()
    {
        base.Die(); // 부모의 기본 탈진 로그 출력
        Debug.Log("<color=red><b>[게임 오버] 플레이어가 탈진 상태에 빠졌습니다!</b></color>");

        // ★ [이미지 변경 포트 작동] 플레이어의 일러스트를 패배(탈진) 스프라이트로 전격 교체합니다!
        if (playerGraphicImage != null && defeatedSprite != null)
        {
            playerGraphicImage.sprite = defeatedSprite;
            Debug.Log("<color=red>[UI] 플레이어의 그래픽이 패배 일러스트로 교체되었습니다.</color>");
        }

        // 배틀 매니저에게 플레이어가 패배(Lost)했음을 실시간으로 알립니다.
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.HandlePlayerDefeat();
        }
    }
}