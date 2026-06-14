using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MonsterStats : EntityStats // 부모의 스탯 필드 및 InitStats 상속
{
    [Header("Monster Data (ScriptableObject)")]
    [SerializeField] private MonsterData currentMonsterData;

    // MonsterStats.cs 상단 변수 선언부 영역에 추가/변경
    [Header("UI Extension References")]
    [SerializeField] private Image monsterGraphicImage; //

    // 1. 작은 사각형 위에 상시 노출될 요약 텍스트 (예: "Attack")
    [SerializeField] private TextMeshProUGUI intentText; //

    // 2. ★ [추가] 마우스를 올렸을 때 판넬(IntentTooltipPanel) 내부에서 보여줄 상세 설명 텍스트
    [SerializeField] private TextMeshProUGUI intentDetailText;

    private int patternIndex = 0;

    void Start()
    {
        // 1. 씬에 배치된 MonsterUI를 명확하게 이름으로 찾아 UI 컨트롤러를 맵핑합니다.
        if (uiController == null)
        {
            GameObject monsterUIObj = GameObject.Find("MonsterUI");
            if (monsterUIObj != null)
            {
                uiController = monsterUIObj.GetComponent<CombatUIController>();
            }
            else
            {
                uiController = FindFirstObjectByType<CombatUIController>();
            }
        }

        // ★ 찾은 UI 컨트롤러에게 내가 타겟임을 명확히 역주입
        if (uiController != null)
        {
            uiController.targetStats = this;
        }

        if (currentMonsterData != null)
        {
            SetupMonster(currentMonsterData);
        }
    }

    public void SetupMonster(MonsterData data)
    {
        currentMonsterData = data;

        // ★ [핵심 추가] 소환된 복사본의 오브젝트 이름을 "지쳐 보이는 고등학생" 같은 기획 이름으로 교체합니다.
        if (data != null && !string.IsNullOrEmpty(data.monsterName))
        {
            gameObject.name = data.monsterName;
        }

        maxHp = data.hpValue;
        hp = maxHp;
        baseAttack = data.attackValue;

        if (monsterGraphicImage != null && data.monsterGraphic != null)
        {
            monsterGraphicImage.sprite = data.monsterGraphic;
        }

        patternIndex = 0;
        UpdateNextIntent();
        RefreshUI();
    }

    // ★ [핵심 변경] 의도를 이미지가 아닌 '텍스트'와 '글자 색상'으로 표현합니다.
    public void UpdateNextIntent()
    {
        if (currentMonsterData == null || currentMonsterData.behaviorPattern.Length == 0) return;

        int index = patternIndex % currentMonsterData.behaviorPattern.Length;
        MonsterAction nextAction = currentMonsterData.behaviorPattern[index];

        if (intentText != null)
        {
            // 1. 글자 색상 분기 처리 (기존 컬러 연출 유지)
            switch (nextAction.actionType)
            {
                case MonsterAction.ActionType.Attack:
                    intentText.color = Color.red;      // 공격은 빨간 글씨
                    break;
                case MonsterAction.ActionType.Defend:
                    intentText.color = Color.cyan;     // 수비는 하늘색 글씨 (UI에서 가독성이 좋음)
                    break;
                case MonsterAction.ActionType.Buff:
                    intentText.color = Color.yellow;   // 버프는 노란 글씨
                    break;
                case MonsterAction.ActionType.Debuff:
                    intentText.color = Color.magenta;  // 디버프는 보라 글씨
                    break;
            }

            // 2. 텍스트 내용 주입
            // 만약 MonsterData 기획 단계에서 적어둔 상세 설명(intentDescription)을 바로 띄우고 싶다면 
            // intentText.text = nextAction.intentDescription; 으로 교체하셔도 됩니다!
            intentText.text = $"행동 예고: [{nextAction.actionType}] ({nextAction.value})";

            if (intentDetailText != null)
            {
                intentDetailText.text = $"<b>\"{nextAction.monsterDialogue}\"</b>\n\n{nextAction.intentDescription}";
            }
        }
    }

    public void ExecuteMonsterTurn()
    {
        if (currentMonsterData == null || currentMonsterData.behaviorPattern.Length == 0) return;

        int index = patternIndex % currentMonsterData.behaviorPattern.Length;
        MonsterAction currentAction = currentMonsterData.behaviorPattern[index];

        switch (currentAction.actionType)
        {
            case MonsterAction.ActionType.Attack:
                int finalDmg = CalculateOutputDamage(currentAction.value);

                PlayerStats player = FindFirstObjectByType<PlayerStats>();
                if (player != null)
                {
                    Debug.Log($"<color=red>[몬스터 턴]</color> {gameObject.name}의 공격! 플레이어에게 {finalDmg}의 피해를 줍니다.");
                    player.TakeDamage(finalDmg);
                }
                break;

            case MonsterAction.ActionType.Defend:
                Debug.Log($"<color=blue>[몬스터 턴]</color> {gameObject.name}의 수비! 방어도 {currentAction.value} 획득.");
                GetArmor(currentAction.value);
                break;

            case MonsterAction.ActionType.Buff:
                AddAttack(currentAction.value);
                break;

            case MonsterAction.ActionType.Debuff:
                Debug.Log($"<color=purple>[몬스터 턴]</color> {gameObject.name}이 플레이어에게 상태 이상을 겁니다. 수치: {currentAction.value}");
                break;
        }

        patternIndex++;
        UpdateNextIntent();
    }

    // MonsterStats.cs 맨 아래에 있는 Die() 함수를 다음과 같이 수정
    // MonsterStats.cs - Die() 함수 수정
    protected override void Die()
    {
        // [수정] 몬스터가 죽으면 그래픽과 UI 텍스트만 먼저 꺼줍니다. (오브젝트는 아직 살려둠)
        if (monsterGraphicImage != null) monsterGraphicImage.gameObject.SetActive(false);
        if (intentText != null) intentText.gameObject.SetActive(false);

        // 스테이지 3 최종 승리 판정
        if (MapManager.Instance != null && MapManager.Instance.currentStage == 3)
        {
            Debug.Log("최종 스테이지 클리어! 엔딩으로 이동합니다.");
            SceneManager.LoadScene("Ending");
            base.Die(); // 여기서 최종 파괴
            return;
        }

        // 1. 단계 진행 및 플래그 세우기
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnBattleVictory();
        }

        // 2. 자동 저장 진행
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        // 3. 보상 팝업 호출 (오브젝트 파괴 전에 안전하게 호출 완료)
        if (RewardManager.Instance != null)
        {
            Debug.Log("[MonsterStats] RewardManager를 통해 카드 보상 선택 창을 활성화합니다.");
            RewardManager.Instance.ShowBattleReward();
        }
        else
        {
            // 만약 RewardManager가 씬에 없거나 오류가 날 때를 대비한 예외 처리 예시
            Debug.LogWarning("[MonsterStats] RewardManager를 찾을 수 없어 즉시 스테이지 맵으로 나갑니다.");
            SceneManager.LoadScene("Stage");
        }

        // ★ [핵심] 모든 연동 작업이 무사히 다 끝난 뒤 부모의 Die(Destroy)를 수행합니다.
        base.Die();
    }
}