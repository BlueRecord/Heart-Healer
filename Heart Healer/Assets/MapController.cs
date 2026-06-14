using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    public GameObject stagePanel;
    public TMP_Text saveMessageText;//TextMeshPro Text를 추가 Inspector에서 Text 비워두기 체크박스 Enabled = 꺼두기 (처음에는 안 보이게)

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
            !MapManager.Instance.stage1Clear;

        // 1클리어 시 2 가능
        stage2Button.interactable =
            MapManager.Instance.stage1Clear &&
            !MapManager.Instance.stage2Clear;

        // 2클리어 시 3 가능
        stage3Button.interactable =
            MapManager.Instance.stage1Clear &&
            MapManager.Instance.stage2Clear &&
            !MapManager.Instance.stage3Clear;
    }

    IEnumerator ShowMessage(string message)
    {
        saveMessageText.text = message;
        saveMessageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);//3초후

        saveMessageText.gameObject.SetActive(false);//비활성화
    }

    public void EnterStage1()
    {
        MapManager.Instance.currentStage = 1;
        SceneManager.LoadScene("BattleScene");
    }

    public void EnterStage2()
    {
        MapManager.Instance.currentStage = 2;
        SceneManager.LoadScene("BattleScene");
    }

    public void EnterStage3()
    {
        MapManager.Instance.currentStage = 3;
        SceneManager.LoadScene("BattleScene");
    }

    public void SaveButton()
    {
        SaveManager.Instance.SaveGame();

        StartCoroutine(
            ShowMessage("Game Saved.")//게임 세이브 성공시 메시지 출력
        );
    }

    public void LoadButton()
    {
        if (!SaveManager.Instance.HasSaveFile())
        {
            StartCoroutine(
                ShowMessage("No save data found.")//세이브 데이터가 없으면 이 메시지 출력
            );
            return;
        }

        SaveManager.Instance.LoadGame();
        UpdateStageButtons();

        StartCoroutine(
        ShowMessage("Game Loaded.")//게임 로드 성공시 메시지 출력
        );
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
