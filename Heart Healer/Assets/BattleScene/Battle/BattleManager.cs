using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// 전투 상태를 관리하는 열거형
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
        // 싱글톤 패턴 적용
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. 플레이어 및 몬스터 프리팹 생성
        GameObject pObj = Instantiate(playerPrefab, playerSpawnPoint);
        GameObject mObj = Instantiate(monsterPrefab, monsterSpawnPoint);

        pObj.name = "Player";
        mObj.name = "Monster";

        // 2. 몬스터 스탯 초기화
        MonsterStats monsterStats = mObj.GetComponent<MonsterStats>();
        if (monsterStats != null && stageMonsterData != null)
        {
            monsterStats.SetupMonster(stageMonsterData);
            Debug.Log($"[BattleManager] {stageMonsterData.monsterName} 데이터 로드 완료!");
        }
        else
        {
            Debug.LogError("[BattleManager] stageMonsterData가 비어있거나 MonsterStats 컴포넌트를 찾을 수 없습니다!");
        }

        // 3. UI 컨트롤러 할당 및 버튼 리스너 추가
        allUIControllers = FindObjectsByType<CombatUIController>(FindObjectsSortMode.None);
        if (turnEndButton != null)
        {
            turnEndButton.onClick.AddListener(OnTurnEndButtonClicked);
        }

        // 4. 덱 초기화 및 전투 시작
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

    // 카드 사용 가능 여부 체크 (턴 및 마나 확인)
    public bool CanUseCard(int cost)
    {
        return currentState == BattleState.PlayerTurn && currentMana >= cost;
    }

    // 마나 소비 처리
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

    // 플레이어 턴 종료 시 호출
    private IEnumerator PlayerEndTurnRoutine()
    {
        currentState = BattleState.EnemyTurn;
        if (turnEndButton != null) turnEndButton.interactable = false;

        // 플레이어 버프/디버프 정산
        PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
        if (pStats != null) pStats.OnTurnEndProcess();

        // 손패 카드 무덤으로 이동
        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null) cardManager.DiscardHand();

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(EnemyTurnRoutine());
    }

    // 몬스터 턴 로직
    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        MonsterStats activeMonster = FindFirstObjectByType<MonsterStats>();

        if (activeMonster != null)
        {
            activeMonster.ResetArmorHardcoded();
            activeMonster.ExecuteMonsterTurn();
        }
        else
        {
            Debug.LogWarning("[BattleManager] 씬에 활성화된 몬스터(MonsterStats)를 찾을 수 없습니다!");
        }

        yield return new WaitForSeconds(1.0f);

        // 몬스터 버프/디버프 정산
        if (activeMonster != null) activeMonster.OnTurnEndProcess();

        // 게임 오버가 아닐 경우 다음 플레이어 턴으로 진행
        if (currentState != BattleState.Lost)
        {
            StartNextPlayerTurn();
        }
    }

    private void StartNextPlayerTurn()
    {
        turnCount++;
        currentState = BattleState.PlayerTurn;

        // 플레이어 턴 시작 시 방어도 초기화 및 지속 효과 적용
        PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
        if (pStats != null)
        {
            pStats.ResetArmor();
            pStats.ProcessStartTurnRegen();
        }

        ResetMana();

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.DrawCard();
        }

        if (turnEndButton != null) turnEndButton.interactable = true;
        RefreshBattleUI();
    }

    // 패배 시 호출 함수
    public void HandlePlayerDefeat()
    {
        if (currentState == BattleState.Lost) return;

        currentState = BattleState.Lost;

        // 버튼 잠금 및 카드 입력 차단
        if (turnEndButton != null) turnEndButton.interactable = false;

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.SetHandInteractable(false);
        }
    }

    // UI 동기화
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

        // 등록된 모든 UI 컨트롤러 갱신
        if (allUIControllers == null) return;
        foreach (var ui in allUIControllers)
        {
            if (ui != null) ui.UpdateUI();
        }
    }
}