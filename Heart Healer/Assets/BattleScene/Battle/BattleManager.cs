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

    private CombatUIController[] allUIControllers;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
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
        yield return new WaitForSeconds(1.5f);

        Debug.Log("<color=red>[몬스터 행동 완료] 플레이어를 공격했습니다!</color>");
        yield return new WaitForSeconds(0.5f);

        StartNextPlayerTurn();
    }

    private void StartNextPlayerTurn()
    {
        turnCount++;
        currentState = BattleState.PlayerTurn;

        Debug.Log($"<color=green><b>[플레이어 턴 시작] 제 {turnCount}턴</b></color>");

        // ★ [방어도 타이밍 수정] 새 턴이 시작될 때 방어도를 안전하게 리셋합니다.
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