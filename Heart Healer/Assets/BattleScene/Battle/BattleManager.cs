using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Mana Settings")]
    [SerializeField] private int maxMana = 3;
    [SerializeField] private int currentMana;

    public int CurrentMana => currentMana; // 에러가 나던 프로퍼티 확보

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetMana();
    }

    public void ResetMana()
    {
        currentMana = maxMana;
    }

    public bool CanUseCard(int cost)
    {
        return currentMana >= cost;
    }

    public void SpendMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;
        Debug.Log($"[마나 사용] 남은 마나: {currentMana}/{maxMana}");
    }

    public void EndTurn()
    {
        Debug.Log("[턴 종료] 다음 기믹 실행을 대기합니다.");
        ResetMana();

        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.DiscardHand();
            cardManager.DrawCard();
        }
    }
}