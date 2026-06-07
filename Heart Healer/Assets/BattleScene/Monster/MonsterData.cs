using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Monster Basic Info")]
    public string monsterName;       // 몬스터 이름 (예: 지쳐 보이는 고등학생)

    // ★ 확실하게 소문자로 시작하는 '순수 정수형 변수'로 선언합니다. (뒤에 괄호가 없습니다)
    public int hpValue;              // 몬스터의 기획 체력 수치
    public int attackValue;          // 몬스터의 기획 기본 공격력 수치

    public Sprite monsterGraphic;    // 몬스터 일러스트 이미지

    [Header("AI Behavior Pattern")]
    public MonsterAction[] behaviorPattern;
}

[System.Serializable]
public class MonsterAction
{
    public enum ActionType { Attack, Defend, Buff, Debuff }

    public ActionType actionType;     // 행동 종류
    public int value;                 // 행동 수치 (대미지나 방어도)

    [TextArea(2, 3)]
    public string intentDescription;  // 마우스 호버 시 툴팁 설명
}