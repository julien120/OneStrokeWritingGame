using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;
    public bool flg = false;
    private string colorString = "#ffffff";
    public (float, float, float, float) tileOffset;
    [SerializeField] private Sprite[] indexImage;
    [SerializeField] private string[] colorCode;
    [SerializeField] public Image leftImage;
    [SerializeField] public Image rightImage;
    [SerializeField] public Image bottomImage;
    [SerializeField] public Image topImage;

    // Start is called before the first frame update
    void Start()
    {
        var hoge = this.GetComponent<RectTransform>().offsetMax;
        var koge = this.GetComponent<RectTransform>().offsetMin;
        tileOffset = (hoge.x, hoge.y, koge.x, koge.y);

    }

    public void TouchedTile(int stageIndex)
    {
        Color color = default(Color);
        ColorUtility.TryParseHtmlString(colorCode[stageIndex], out color);
        image.color = color;
        image.sprite = null;
    }


    public void BackedTile(int stageIndex)
    {
        // DCDDE0
        Color color = default(Color);
        ColorUtility.TryParseHtmlString(colorString, out color);
        image.sprite = null;
        image.color = color;
        flg = false;

        for (int k = 0; k < transform.childCount; k++)
        {
            var childObject = transform.GetChild(k).gameObject;
            childObject.SetActive(false);
        }
    }

    public void StartTilePos(int stageIndex)
    {
        image.sprite = indexImage[stageIndex];
        image.color = Color.white;
    }

    public void DrawHeadTile(int stageIndex)
    {
        image.sprite = indexImage[stageIndex];
        Color color = default(Color);
        ColorUtility.TryParseHtmlString(colorString, out color);
        image.color = Color.white;
        flg = true;
    }

    public void DrawSideColor(Image hoge, int stageIndex)
    {
        Color color = default(Color);
        ColorUtility.TryParseHtmlString(colorCode[stageIndex], out color);
        hoge.color = color;
    }

}
