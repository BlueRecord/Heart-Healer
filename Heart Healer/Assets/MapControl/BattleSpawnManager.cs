using UnityEngine;

public class BattleSpawnManager : MonoBehaviour
{
    [Header("Stage Monster Scriptables")]
    [SerializeField] private MonsterData elementSchoolData; // 1스테이지 몬스터
    [SerializeField] private MonsterData universityData;    // 2스테이지 몬스터
    [SerializeField] private MonsterData grandmotherData;   // 3스테이지 몬스터

    // [중요] Awake 타이밍 이슈를 해결하기 위해 Start() 시점으로 미룹니다.
    void Start()
    {
        AssignMonsterDataForStage();
    }

    void AssignMonsterDataForStage()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("[BattleSpawnManager] MapManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // currentStep 대신 MapManager의 currentStage를 가져옵니다.
        int stage = MapManager.Instance.currentStage;
        MonsterData selectedData = null;

        // 깔끔하게 기획된 스테이지 단위로 라우팅 처리
        if (stage == 1) selectedData = elementSchoolData;
        else if (stage == 2) selectedData = universityData;
        else if (stage == 3) selectedData = grandmotherData;
        else
        {
            Debug.LogWarning($"[BattleSpawnManager] 정의되지 않은 스테이지({stage})입니다. 1스테이지 데이터로 세팅합니다.");
            selectedData = elementSchoolData;
        }

        // 🛠️ [핵심 수정] 찾은 데이터를 BattleManager의 새로 개설된 통로에 수령해줍니다!
        if (selectedData != null && BattleManager.Instance != null)
        {
            // BattleManager의 명시적 수령 함수 호출
            BattleManager.Instance.SetStageMonsterData(selectedData);
            Debug.Log($"[BattleSpawnManager] {stage}스테이지 데이터({selectedData.name})를 배틀 매니저로 완벽히 토스했습니다.");
        }
        else
        {
            Debug.LogError("[BattleSpawnManager] 데이터 매칭은 성공했으나 BattleManager를 찾을 수 없습니다.");
        }
    }
}