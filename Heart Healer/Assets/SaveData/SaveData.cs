using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public bool stage1Clear;
    public bool stage2Clear;
    public bool stage3Clear;

    // public int currentStep; // 기존 주석 처리
    public int currentStage; // 스테이지 번호 저장용으로 변경
    public List<string> deckCardIDs = new List<string>();
}