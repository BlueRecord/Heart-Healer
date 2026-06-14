using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    string savePath;

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

        savePath = Application.persistentDataPath + "/save.json";
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.stage1Clear = GameManager.Instance.stage1Clear;
        data.stage2Clear = GameManager.Instance.stage2Clear;
        data.stage3Clear = GameManager.Instance.stage3Clear;
        data.currentStage = GameManager.Instance.currentStage;//저장해야 할 데이터가 추가되면 똑같이 적어줘야합니다

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
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        GameManager.Instance.stage1Clear = data.stage1Clear;
        GameManager.Instance.stage2Clear = data.stage2Clear;
        GameManager.Instance.stage3Clear = data.stage3Clear;
        GameManager.Instance.currentStage = data.currentStage;//저장해야 할 데이터가 추가되면 똑같이 적어줘야합니다

        Debug.Log("로드 완료");
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }
}
