using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterData : ScriptableObject
{
        
    [Header("Monster Basic Info")]
    public string monsterName;       // 몬스터 이름 (예: 완벽주의 대학생)
    public int hpValue;              // 몬스터의 기획 체력 수치
    public int attackValue;          // 몬스터의 기획 기본 공격력 수치
    // ★ 몬스터 이미지를 전투 중 / 전투 승리로 구분하여 할당할 수 있게 두 칸으로 나눕니다.
    [Header("Monster Visuals")]
    [Tooltip("전투 중 화면에 표시될 기본 일러스트입니다.")]
    public Sprite monsterGraphic;        // 기존 전투 중 이미지

    [Tooltip("상담사가 승리했을 때(몬스터 처치 시) 표시될 일러스트입니다.")]
    public Sprite monsterVictoryGraphic; // ★ 새로 추가된 전투 승리 이미지

    [Header("AI Behavior Pattern")]
    public MonsterAction[] behaviorPattern;
}

[System.Serializable]
public class MonsterAction
{
    public enum ActionType
    {
        Attack,
        Defend,
        Buff,
        Debuff,
        DefendAndAttack,   // [대학생] 방어와 동시에 공격하는 복합 기믹
        AddDummyCards,     // [초등학생] 더미 카드 2장 주입
        DrawReduce,        // [초등학생] 다음 턴 드로우 -1
        ScaleByHandCount,  // [대학생] 상담사 카드 장수 x 2 대미지
        IncreaseCost,      // [할머니] 다음 턴 모든 카드 코스트 +1
        DecreaseDamage     // [할머니] 다음 턴 카드 공격력 -1
    }

    [Header("1. Pattern Meta Data")]
    public string patternName;        // ★ 패턴 이름 (예: 이어폰끼기, 팩트 폭행)

    [Header("2. Dialogue")]
    [TextArea(2, 3)]
    public string monsterDialogue;    // ★ 몬스터가 실제로 출력할 대사 (예: "어차피 뻔한 소리...")

    [Header("3. Tooltip / Intent Description")]
    [TextArea(2, 3)]
    public string intentDescription;  // ★ 마우스 호버 시 유저에게 보여줄 기믹 설명 (예: "방어도 +5 및 정신력 -3")

    [Header("4. System Logic Settings")]
    public ActionType actionType;     // 행동 종류
    public int value;                 // 기본 수치 (대미지, 방어도 수치, 드로우 감소량 등)

    [Tooltip("DefendAndAttack(복합) 기믹에서 동시 적용할 추가 대미지 수치입니다.")]
    public int additionalDamage;      // 복합 기믹용 추가 대미지 칸
};