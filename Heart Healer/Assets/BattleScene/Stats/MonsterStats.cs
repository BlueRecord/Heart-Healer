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