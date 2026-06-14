using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private string savePath;

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
            return;
        }

        savePath = Application.persistentDataPath + "/save.json";
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public void SaveGame()
    {
        if (MapManager.Instance == null) return;

        CustomSaveData data = new CustomSaveData();

        // 덱 카드 저장
        data.deckCardIDs.Clear();
        if (DeckManager.Instance != null && DeckManager.Instance.PlayerDeck != null)
        {
            foreach (var card in DeckManager.Instance.PlayerDeck)
            {
                if (card != null) data.deckCardIDs.Add(card.name);
            }
        }

        // currentStep 대신 currentStage 저장
        data.stage1Clear = MapManager.Instance.stage1Clear;
        data.stage2Clear = MapManager.Instance.stage2Clear;
        data.stage3Clear = MapManager.Instance.stage3Clear;
        data.currentStage = MapManager.Instance.currentStage;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("저장 완료 : " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("세이브 파일 없음");
            return;
        }

        string json = File.ReadAllText(savePath);
        CustomSaveData data = JsonUtility.FromJson<CustomSaveData>(json);

        if (MapManager.Instance != null)
        {
            MapManager.Instance.stage1Clear = data.stage1Clear;
            MapManager.Instance.stage2Clear = data.stage2Clear;
            MapManager.Instance.stage3Clear = data.stage3Clear;
            MapManager.Instance.currentStage = data.currentStage; // currentStage 반영
        }

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.LoadDeckFromIDs(data.deckCardIDs);
        }

        Debug.Log("로드 완료");
    }
}

[System.Serializable]
public class CustomSaveData
{
    public bool stage1Clear;
    public bool stage2Clear;
    public bool stage3Clear;
    public int currentStage; // 변수명 변경
    public List<string> deckCardIDs = new List<string>();
}