using UnityEngine;

[System.Serializable]
public class SaveData
{
    public bool stage1Clear;
    public bool stage2Clear;
    public bool stage3Clear;

    public int currentStage;
    //이런식으로 저장해야 하는 데이터를 다 써주시면 됩니다
}
