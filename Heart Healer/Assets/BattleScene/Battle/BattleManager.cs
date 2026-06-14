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

    // ★ 핵심 수정: Start를 코루틴(IEnumerator)으로 변경하여 배달부(BattleSpawnManager)가 데이터를 꽂아줄 시간을 벌어줍니다.
    IEnumerator Start()
    {
        // 0. 다른 매니저들의 Awake 처리가 모두 끝날 때까지 단 1프레임 양보하고 기다립니다.
        yield return null;

        // 1. 플레이어 및 몬스터 프리팹 생성
        GameObject pObj = Instantiate(playerPrefab, playerSpawnPoint);
        GameObject mObj = Instantiate(monsterPrefab, monsterSpawnPoint);

        pObj.name = "Player";
        mObj.name = "Monster";

        // 2. 이제 배달부에게 전달받은 최신 stageMonsterData를 바탕으로 안전하게 스탯을 세팅합니다.
        MonsterStats monsterStats = mObj.GetComponent<MonsterStats>();
        if (monsterStats != null && stageMonsterData != null)
        {
            monsterStats.SetupMonster(stageMonsterData);
            Debug.Log($"[BattleManager] {stageMonsterData.monsterName} 데이터 최종 로드 및 스탯 적용 완료!");
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

        // 4. 데이터 세팅이 완벽히 끝난 후 덱을 초기화하고 전투를 시작하므로 카드 에러가 나지 않습니다.
        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null) cardManager.PrepareInitDeck();

        StartBattle();
    }

    // ★ 배달부(BattleSpawnManager)가 호출해서 데이터를 강제로 꽂아넣을 통로 함수를 명시적으로 열어줍니다.
    public void SetStageMonsterData(MonsterData data)
    {
        stageMonsterData = data;
        Debug.Log($"[BattleManager] 배달부로부터 '{data.monsterName}' 데이터를 수령했습니다.");
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
            activeMonster.ResetArmorHardcoded();
            activeMonster.ExecuteMonsterTurn();
        }
        else
        {
            Debug.LogWarning("[BattleManager] 씬에 활성화된 몬스터(MonsterStats)를 찾을 수 없습니다!");
        }

        yield return new WaitForSeconds(1.0f);

        if (activeMonster != null) activeMonster.OnTurnEndProcess();

        if (currentState != BattleState.Lost)
        {
            StartNextPlayerTurn();
        }
    }

    private void StartNextPlayerTurn()
    {
        turnCount++;
        currentState = BattleState.PlayerTurn;

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

    public void HandlePlayerDefeat()
    {
        if (currentState == BattleState.Lost) return;

        currentState = BattleState.Lost;

        if (turnEndButton != null) turnEndButton.interactable = false;

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