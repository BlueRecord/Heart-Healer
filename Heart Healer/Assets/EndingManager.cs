using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    public void OnClickRestart()
    {
        // 1. 데이터 초기화
        if (MapManager.Instance != null)
        {
            //MapManager.Instance.ResetGameData();
        }

        // 2. 오프닝 씬으로 이동
        SceneManager.LoadScene("Opening");
    }
}