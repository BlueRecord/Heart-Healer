using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class PlayerData
{
    //public 저장할 데이터 변수로 다 적어주기
    public string name;
}
public class DataManger : MonoBehaviour
{
    public static DataManger instance;
    //하나만 존재 쉬운접근을 위해 static사용

    public PlayerData savefile = new PlayerData();

    public string path;
    public int nowslot;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)//새로운게 생성되면 삭제
        {
            Destroy(instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);//씬이 바뀌어도 안지워짐

        path = Application.persistentDataPath + "/save";//유니티가 별도의 저장경로를 할당
        print(path);
    }
    void Start()
    {
       
    }

    public void SaveData()
    {
        string data = JsonUtility.ToJson(savefile);//제이슨 파일로 변환

        File.WriteAllText(path + nowslot.ToString(), data);  //filename으로 설정한 것으로 저장
    }

    public void LoadData()
    {
        string data = File.ReadAllText(path + nowslot.ToString());  //읽어온거 저장
        savefile = JsonUtility.FromJson<PlayerData>(data);//제이슨 파일을 읽어 플레이어 데이터 형식으로 변환

        
    }

    public void DataClear()
    {
        nowslot = -1;
        savefile = new PlayerData();
    }

    void Update()
    {

    }
}





