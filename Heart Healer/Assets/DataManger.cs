using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public string name;
    // 플레이어가 들고 있는 카드들의 ID 목록
    public List<string> ownedCardIDs = new List<string>();
}

public class DataManger : MonoBehaviour
{
    public static DataManger instance; //

    public PlayerData savefile = new PlayerData(); //

    public string path; //
    public int nowslot; //

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; //
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 버그 방지 고정
            return;
        }
        DontDestroyOnLoad(this.gameObject); //

        path = Application.persistentDataPath + "/save"; //
        print(path); //
    }

    public void SaveData()
    {
        string data = JsonUtility.ToJson(savefile); //
        File.WriteAllText(path + nowslot.ToString(), data); //
    }

    public void LoadData()
    {
        if (File.Exists(path + nowslot.ToString()))
        {
            string data = File.ReadAllText(path + nowslot.ToString()); //
            savefile = JsonUtility.FromJson<PlayerData>(data); //
        }
    }

    public void DataClear()
    {
        nowslot = -1; //
        savefile = new PlayerData(); //
    }
}