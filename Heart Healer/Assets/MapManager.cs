using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

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

void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
