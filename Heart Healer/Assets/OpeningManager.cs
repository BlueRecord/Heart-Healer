using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    public void StartGame()
    {
        if (MapManager.Instance != null)
        {
            // 1. 처음 시작할 때는 1스테이지부터 시작
            MapManager.Instance.currentStage = 1;

            // 2. 새 게임이므로 기존 클리어 플래그 초기화
            MapManager.Instance.stage1Clear = false;
            MapManager.Instance.stage2Clear = false;
            MapManager.Instance.stage3Clear = false;
        }

        // 이동할 맵 씬 이름("Stage")으로 로드
        SceneManager.LoadScene("Stage");
    }
}