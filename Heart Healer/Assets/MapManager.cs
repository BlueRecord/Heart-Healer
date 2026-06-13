using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool stage1Clear = false;
    public bool stage2Clear = false;
    public bool stage3Clear = false;

    public int currentStage = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}