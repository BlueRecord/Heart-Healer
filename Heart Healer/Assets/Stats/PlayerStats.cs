using UnityEngine;

public class PlayerStats : EntityStats
{
    [Header("Energy Stats")]
    [SerializeField] protected int maxCost = 3;
    [SerializeField] protected int currentCost;
    void Start()
    {
        InitStats();
        InitCost();
    }
    public void InitCost()
    {
        currentCost = maxCost;
    }
    public void UsingCost(int usedCost)
    {
        currentCost -= usedCost;
    }
    public bool CompareCost(int usedCost)
    {
        if (currentCost < usedCost) return true;
        return false;
    }

    void Update()
    {

    }
}
