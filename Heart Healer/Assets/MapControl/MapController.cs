using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    [Header("Old Map Panels")]
    public GameObject stagePanel;
    public TMP_Text saveMessageText;

    [Header("Stage Main Buttons")]
    public Button stage1Button;
    public Button stage2Button;
    public Button stage3Button;

    // [주석 처리] 휴식 버튼 제거
    // public Button mainRestButton;

    // [주석 처리] 휴식 패널 및 오브젝트 제거
    // [Header("Rest Site UI Setup")]
    // public GameObject restSitePanel;
    // [Header("Rest Site Objects by Stage")]
    // public GameObject restSite1;
    // public GameObject restSite2;
    // public GameObject restSite3;
    // [Header("Rest Site Character Image Target")]
    // public Image characterImage;
    // [Header("Character Sprites (Before / After)")]
    // public Sprite charBeforeRestSprite;
    // public Sprite charAfterRestSprite;

    // private bool hasRestedInCurrentStage = false;

    void Start()
    {
        UpdateMapFlowUI();
    }

    public void UpdateMapFlowUI()
    {
        if (MapManager.Instance == null) return;

        // 모든 스테이지 버튼 우선 비활성화
        stage1Button.gameObject.SetActive(false);
        stage2Button.gameObject.SetActive(false);
        stage3Button.gameObject.SetActive(false);

        // [주석 처리]
        // mainRestButton.gameObject.SetActive(false);
        // restSitePanel.SetActive(false);

        // 1스테이지 클리어 전 -> 1스테이지 전투 버튼 활성화
        if (!MapManager.Instance.stage1Clear)
        {
            stage1Button.gameObject.SetActive(true);
        }
        // 1스테이지 깼고 2스테이지 아직 안 깼음 -> 2스테이지 전투 버튼 활성화
        else if (MapManager.Instance.stage1Clear && !MapManager.Instance.stage2Clear)
        {
            stage2Button.gameObject.SetActive(true);
        }
        // 2스테이지 깼고 3스테이지 아직 안 깼음 -> 3스테이지 전투 버튼 활성화
        else if (MapManager.Instance.stage2Clear && !MapManager.Instance.stage3Clear)
        {
            stage3Button.gameObject.SetActive(true);
        }
        else if (MapManager.Instance.stage3Clear)
        {
            Debug.Log("모든 스테이지를 클리어하여 게임이 종료되었습니다.");
        }
    }

    // [주석 처리] 휴식 장소 진입 및 완료 코드 전체 주석 처리
    /*
    public void OnClickMainRestButton()
    {
        if (MapManager.Instance == null) return;
        restSitePanel.SetActive(true);
        // ... 생략 ...
    }

    public void OnClickRestActionComplete()
    {
        // ... 생략 ...
    }

    IEnumerator CoWaitAndProceedStep()
    {
        // ... 생략 ...
    }
    */

    // 순수하게 전투 진입만 남김 (시작 시 MapManager에 스테이지 확정 주입)
    public void EnterStage1()
    {
        MapManager.Instance.StartBattle(1);
        SceneManager.LoadScene("BattleScene");
    }
    public void EnterStage2()
    {
        MapManager.Instance.StartBattle(2);
        SceneManager.LoadScene("BattleScene");
    }
    public void EnterStage3()
    {
        MapManager.Instance.StartBattle(3);
        SceneManager.LoadScene("BattleScene");
    }

    public void OpenMap() { stagePanel.SetActive(true); }
    public void CloseMap() { stagePanel.SetActive(false); }

    IEnumerator ShowMessage(string message)
    {
        saveMessageText.text = message;
        saveMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        saveMessageText.gameObject.SetActive(false);
    }

    public void SaveButton() { SaveManager.Instance.SaveGame(); StartCoroutine(ShowMessage("Game Saved.")); }
    public void LoadButton()
    {
        if (!SaveManager.Instance.HasSaveFile()) { StartCoroutine(ShowMessage("No save data found.")); return; }
        SaveManager.Instance.LoadGame();
        UpdateMapFlowUI(); // 로드 후 직관적으로 UI만 갱신
        StartCoroutine(ShowMessage("Game Loaded."));
    }
}