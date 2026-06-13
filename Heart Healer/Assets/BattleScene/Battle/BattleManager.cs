using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// =================================================================
// ★ [핵심 해결] 빠져있던 배틀 상태(BattleState) 열거형 정의 추가
// =================================================================
public enum BattleState { PlayerTurn, EnemyTurn, Victory, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("State Settings")]
    [SerializeField] private BattleState currentState = BattleState.PlayerTurn;
    [SerializeField] private int turnCount = 1;

    [Header("Mana Settings")]
    [SerializeField] private int maxMana = 3;
    [SerializeField] private int currentMana;

    public int CurrentMana => currentMana;
    public BattleState CurrentState => currentState;

    [Header("Mana UI Components")]
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Image singleManaImage;

    [Header("Mana Sprites")]
    [SerializeField] private Sprite manaActiveSprite;
    [SerializeField] private Sprite manaInactiveSprite;

    [Header("Turn UI Components")]
    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private Button turnEndButton;

    [Header("Dynamic Spawn Prefab Ports")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform monsterSpawnPoint;

    [Header("Monster Data Configuration")]
    [SerializeField] private MonsterData stageMonsterData;

    private CombatUIController[] allUIControllers;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. 프리팹 스폰 (인스펙터에 등록된 위치 기준으로 배치)
        GameObject pObj = Instantiate(playerPrefab, playerSpawnPoint);
        GameObject mObj = Instantiate(monsterPrefab, monsterSpawnPoint);

        // 2. 이름 정렬 (Find 기능을 안전하게 쓰기 위함)
        pObj.name = "Player";
        mObj.name = "Monster";

        // ★ [핵심 수정] 생성된 몬스터에게 기획 데이터 주입하기
        MonsterStats monsterStats = mObj.GetComponent<MonsterStats>();
        if (monsterStats != null && stageMonsterData != null)
        {
            // MonsterStats 내부에 데이터 초기화 함수(예: Setup 또는 Initialize)가 있다면 호출합니다.
            // 만약 함수명이 다르면 프로젝트에 맞춰 수정하세요.
            monsterStats.SetupMonster(stageMonsterData);
            Debug.Log($"[BattleManager] {stageMonsterData.monsterName} 데이터 로드 완료!");
        }
        else
        {
            Debug.LogError("[BattleManager] stageMonsterData가 비어있거나 MonsterStats 컴포넌트를 찾을 수 없습니다!");
        }

        // 3. UI 및 카드 초기화
        allUIControllers = FindObjectsByType<CombatUIController>(FindObjectsSortMode.None);

        if (turnEndButton != null)
        {
            turnEndButton.onClick.AddListener(OnTurnEndButtonClicked);
        }

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null) cardManager.PrepareInitDeck();

        StartBattle();
    }

    public void StartBattle()
    {
        currentState = BattleState.PlayerTurn;
        turnCount = 1;
        ResetMana();

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null) cardManager.DrawCard();

        RefreshBattleUI();
    }

    public void ResetMana()
    {
        currentMana = maxMana;
        RefreshBattleUI();
    }

    public bool CanUseCard(int cost)
    {
        return currentState == BattleState.PlayerTurn && currentMana >= cost;
    }

    public void SpendMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;
        RefreshBattleUI();
    }

    private void OnTurnEndButtonClicked()
    {
        if (currentState != BattleState.PlayerTurn) return;
        StartCoroutine(PlayerEndTurnRoutine());
    }

    private IEnumerator PlayerEndTurnRoutine()
    {
        currentState = BattleState.EnemyTurn;
        if (turnEndButton != null) turnEndButton.interactable = false;

        // 플레이어 턴 종료 시 버프/디버프 지속 턴 정산
        PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
        if (pStats != null) pStats.OnTurnEndProcess();

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null) cardManager.DiscardHand();

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        MonsterStats activeMonster = FindFirstObjectByType<MonsterStats>();

        if (activeMonster != null)
        {
            activeMonster.ResetArmorHardcoded(); // 몬스터 기존 방어도 리셋
            activeMonster.ExecuteMonsterTurn();  // 패턴 실행
        }
        else
        {
            Debug.LogWarning("[BattleManager] 씬에 활성화된 몬스터(MonsterStats)를 찾을 수 없습니다!");
        }

        yield return new WaitForSeconds(1.0f);

        // 몬스터 턴 종료 시 몬스터의 버프/디버프 지속 턴 감소 처리
        if (activeMonster != null) activeMonster.OnTurnEndProcess();

        // 몬스터의 공격으로 플레이어가 사망(Lost 상태)한 게 아니라면 다음 턴 진행
        if (currentState != BattleState.Lost)
        {
            StartNextPlayerTurn();
        }
    }

    private void StartNextPlayerTurn()
    {
        turnCount++;
        currentState = BattleState.PlayerTurn;

        Debug.Log($"<color=green><b>[플레이어 턴 시작] 제 {turnCount}턴</b></color>");

        PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
        if (pStats != null)
        {
            pStats.ResetArmor();           // 플레이어 일회성 안정감 소멸
            pStats.ProcessStartTurnRegen(); // 지속 버프가 있다면 턴 시작 시 방어도 생성
        }

        ResetMana();

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.DrawCard(); // 카드 드로우 (추가 예약 드로우 자동 합산)
        }

        if (turnEndButton != null) turnEndButton.interactable = true;
        RefreshBattleUI();
    }

    // 플레이어 탈진(패배) 처리 함수
    public void HandlePlayerDefeat()
    {
        if (currentState == BattleState.Lost) return; // 중복 실행 방지

        currentState = BattleState.Lost;
        Debug.Log("<color=red><b>[배틀 매니저] 플레이어 탈진 상태 확인. 게임 오버를 선언합니다.</b></color>");

        // 턴 종료 버튼 잠금
        if (turnEndButton != null) turnEndButton.interactable = false;

        // 플레이어가 카드를 더 이상 건드리지 못하게 상호작용 차단
        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.SetHandInteractable(false);
        }
    }

    public void RefreshBattleUI()
    {
        if (manaText != null)
        {
            manaText.text = $"{currentMana} / {maxMana}";
        }

        if (singleManaImage != null)
        {
            singleManaImage.sprite = (currentMana > 0) ? manaActiveSprite : manaInactiveSprite;
        }

        if (turnCountText != null)
        {
            turnCountText.text = turnCount.ToString();
        }

        if (allUIControllers == null) return;
        foreach (var ui in allUIControllers)
        {
            if (ui != null) ui.UpdateUI();
        }
    }
}