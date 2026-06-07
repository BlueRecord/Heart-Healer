using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    // ==========================================
    // ★ [원하셨던 핵심] 프리팹 및 소환 위치 등록 포트 개설
    // ==========================================
    [Header("Dynamic Spawn Prefab Ports")]
    [SerializeField] private GameObject playerPrefab; // 여기에 플레이어 프리팹 연결
    [SerializeField] private GameObject monsterPrefab; // 여기에 몬스터 프리팹 연결
    [SerializeField] private Transform playerSpawnPoint; // 소환될 위치 1
    [SerializeField] private Transform monsterSpawnPoint; // 소환될 위치 2

    [Header("Monster Data Configuration")]
    [SerializeField] private MonsterData stageMonsterData; // 몬스터 데이터를 담을 새로운 포트

    private CombatUIController[] allUIControllers;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. 프리팹 생성 (수동 배치 삭제 후 사용)
        Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        GameObject mObj = Instantiate(monsterPrefab, monsterSpawnPoint.position, Quaternion.identity);

        // 2. 소환된 몬스터에게 데이터 주입
        MonsterStats mStats = mObj.GetComponent<MonsterStats>();
        if (mStats != null && stageMonsterData != null)
        {
            mStats.SetupMonster(stageMonsterData); // 여기서 데이터가 연결됩니다!
        }

        // 1. 배틀 시작 전, 포트에 등록된 프리팹들을 지정 위치에 실시간 소환합니다.
        SpawnEntities();

        // 2. 캐릭터들이 소환 완료된 직후 씬에 생성된 UI 컨트롤러들을 안전하게 수집합니다.
        allUIControllers = FindObjectsByType<CombatUIController>(FindObjectsSortMode.None);

        if (turnEndButton != null)
        {
            turnEndButton.onClick.AddListener(OnTurnEndButtonClicked);
        }

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.PrepareInitDeck();
        }

        StartBattle();
    }

    // ★ 프리팹 소환 전담 함수 생성
    private void SpawnEntities()
    {
        // 플레이어 프리팹 동적 소환
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            GameObject spawnedPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            spawnedPlayer.name = "Player"; // 복사본 생성 시 뒤에 (Clone) 붙는 현상 깔끔하게 정리
        }
        else
        {
            Debug.LogWarning("[BattleManager] 플레이어 프리팹이나 스폰 포인트가 인스펙터 포트에 등록되지 않았습니다!");
        }

        // 몬스터 프리팹 동적 소환
        if (monsterPrefab != null && monsterSpawnPoint != null)
        {
            GameObject spawnedMonster = Instantiate(monsterPrefab, monsterSpawnPoint.position, Quaternion.identity);
            spawnedMonster.name = "Monster";
        }
        else
        {
            Debug.LogWarning("[BattleManager] 몬스터 프리팹이나 스폰 포인트가 인스펙터 포트에 등록되지 않았습니다!");
        }
    }

    public void StartBattle()
    {
        currentState = BattleState.PlayerTurn;
        turnCount = 1;

        ResetMana();

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.DrawCard();
        }

        RefreshBattleUI();

        Debug.Log($"<color=green><b>[전투 시작] 제 {turnCount}턴 - 플레이어 차례</b></color>");
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
        Debug.Log($"[마나 사용] 남은 마나: {currentMana}/{maxMana}");

        RefreshBattleUI();
    }

    private void OnTurnEndButtonClicked()
    {
        if (currentState != BattleState.PlayerTurn) return;
        StartCoroutine(PlayerEndTurnRoutine());
    }

    public void HandlePlayerDefeat()
    {
        if (currentState == BattleState.Lost) return;

        currentState = BattleState.Lost;
        Debug.Log("<color=black><b>[배틀 결과] 플레이어 패배... 전투가 종료됩니다.</b></color>");

        if (turnEndButton != null) turnEndButton.interactable = false;
    }

    private IEnumerator PlayerEndTurnRoutine()
    {
        Debug.Log("<color=yellow>[플레이어 턴 종료]</color>");
        currentState = BattleState.EnemyTurn;

        if (turnEndButton != null) turnEndButton.interactable = false;

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.DiscardHand();
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("<color=red><b>[몬스터 턴 시작]</b></color>");
        yield return new WaitForSeconds(1.0f);

        MonsterStats activeMonster = FindFirstObjectByType<MonsterStats>();

        if (activeMonster != null)
        {
            activeMonster.ResetArmorHardcoded();

            Debug.Log($"<color=red>[몬스터 행동] {activeMonster.gameObject.name}의 행동을 개시합니다.</color>");
            activeMonster.ExecuteMonsterTurn();
        }
        else
        {
            Debug.LogWarning("[BattleManager] 씬에 활성화된 몬스터(MonsterStats)를 찾을 수 없습니다!");
        }

        yield return new WaitForSeconds(1.0f);
        StartNextPlayerTurn();
    }

    private void StartNextPlayerTurn()
    {
        turnCount++;
        currentState = BattleState.PlayerTurn;

        Debug.Log($"<color=green><b>[플레이어 턴 시작] 제 {turnCount}턴</b></color>");

        PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
        if (pStats != null)
        {
            pStats.ResetArmor();
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