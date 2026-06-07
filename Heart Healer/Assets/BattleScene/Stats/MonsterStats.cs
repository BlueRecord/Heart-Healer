using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterStats : EntityStats // 부모의 스탯 필드 및 InitStats 상속
{
    [Header("Monster Data (ScriptableObject)")]
    [SerializeField] private MonsterData currentMonsterData;

    [Header("UI Extension References")]
    [SerializeField] private Image monsterGraphicImage;

    // ★ [변경] 이미지 컴포넌트 대신 텍스트 컴포넌트를 받아옵니다!
    // 만약 텍스트메시프로를 쓰신다면 Text 대신 TextMeshProUGUI를 쓰시면 됩니다.
    [SerializeField] private TextMeshProUGUI intentText;

    private int patternIndex = 0;

    void Start()
    {
        // 1. 씬에 배치된 CombatUIController를 코드 내부에서 자동으로 찾아서 내 부모 변수에 주입
        if (uiController == null)
        {
            uiController = FindFirstObjectByType<CombatUIController>();
        }

        // ★ [핵심 추가]: 찾은 UI 컨트롤러에게 "내가 네 타겟이야"라고 역주입해 줍니다.
        if (uiController != null)
        {
            // CombatUIController에 정의된 Target Stats 변수에 나(this)를 직접 꽂아줍니다.
            // ※ 만약 CombatUIController 내부의 변수명이 targetStats가 아니라 다른 이름(예: targetMonster 등)이라면 
            // 변수 오류가 날 수 있으므로, CombatUIController의 해당 변수명으로 변경해 주세요!
            uiController.targetStats = this;
        }

        // 이제 uiController 세팅과 타겟 주입이 끝났으므로 데이터를 안전하게 세팅합니다.
        if (currentMonsterData != null)
        {
            SetupMonster(currentMonsterData);
        }
    }

    public void SetupMonster(MonsterData data)
    {
        if (data == null) return;

        currentMonsterData = data;
        gameObject.name = data.monsterName;

        this.maxHp = data.hpValue;
        this.baseAttack = data.attackValue;
        this.patternIndex = 0;

        InitStats(); // 부모 초기화 (체력을 maxHp로 채움)

        if (monsterGraphicImage != null && data.monsterGraphic != null)
        {
            monsterGraphicImage.sprite = data.monsterGraphic;
        }

        UpdateNextIntent(); // 의도 텍스트 갱신

        // ★ [여기에 이 코드 한 줄을 추가해 주세요!]
        // 데이터 세팅이 완전히 끝난 시점에 UI를 강제로 새로고침 시킵니다.
        // (부모인 EntityStats가 uiController를 가지고 있으므로 바로 접근 가능합니다)
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
            intentText.text = $"다음 행동: [{nextAction.actionType}] ({nextAction.value})";
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

    protected override void Die()
    {
        base.Die();
    }
}