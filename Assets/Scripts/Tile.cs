using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;
    public bool flg = false;
    private string colorString = "#FFFFFF";
    public (float, float, float, float) tileOffset;
    [SerializeField] private Image[] indexImage;
    [SerializeField] private string[] colorCode;

    // Start is called before the first frame update
    void Start()
    {
        var hoge = this.GetComponent<RectTransform>().offsetMax;
        var koge = this.GetComponent<RectTransform>().offsetMin;
        tileOffset = (hoge.x, hoge.y, koge.x, koge.y);

    }

    public void TouchedTile()
    {
        image.color = Color.red;
        flg = true;
    }

    public void drawTile(string colorCode)
    {
        Color color = default(Color);
        ColorUtility.TryParseHtmlString(colorCode, out color);
        image.color = color;
        flg = true;
    }

    public void BackedTile()
    {
        // DCDDE0
        Color color = default(Color);
        ColorUtility.TryParseHtmlString(colorString, out color);
        image.color = color;
        flg = false;
    }

    public void StartTilePos()
    {
        image.color = Color.blue;
    }


}
