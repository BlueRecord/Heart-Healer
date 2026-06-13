using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public GameObject stagePanel;

    public Button stage1Button;
    public Button stage2Button;
    public Button stage3Button;

    private void Start()
    {
        UpdateStageButtons();
    }

    public void OpenMap()
    {
        stagePanel.SetActive(true);
    }

    public void CloseMap()
    {
        stagePanel.SetActive(false);
    }

    void UpdateStageButtons()
    {
        // 처음엔 1스테이지만 가능
        stage1Button.interactable =
            !GameManager.Instance.stage1Clear;

        // 1클리어 시 2 가능
        stage2Button.interactable =
            GameManager.Instance.stage1Clear &&
            !GameManager.Instance.stage2Clear;

        // 2클리어 시 3 가능
        stage3Button.interactable =
            GameManager.Instance.stage1Clear &&
            GameManager.Instance.stage2Clear &&
            !GameManager.Instance.stage3Clear;
    }

    public void EnterStage1()
    {
        GameManager.Instance.currentStage = 1;
        SceneManager.LoadScene("BattleScene");
    }

    public void EnterStage2()
    {
        GameManager.Instance.currentStage = 2;
        SceneManager.LoadScene("BattleScene");
    }

    public void EnterStage3()
    {
        GameManager.Instance.currentStage = 3;
        SceneManager.LoadScene("BattleScene");
    }
}
