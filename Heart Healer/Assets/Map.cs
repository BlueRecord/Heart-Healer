using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Map : MonoBehaviour
{

    public GameObject[] stage;

    public void ShowImage()
    {
        // 모든 이미지 숨기기
        foreach (GameObject img in stage)
        {
            img.SetActive(false);
        }

        //if를 사용해서 1스테이지 클리어 여부에따라 1스테이지 이미지 출력 1스테이지가 클리어 됐다면 2스테이지 이미지 출력 2스테이지 클리어 됐다면 3스테이지 이미지 출력
        //if (stage1bosshp != 0)
        {
            stage[0].SetActive(true);
        }
        //else if . . .

    }
        void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       
    }
   
}
