using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;
    public bool flg = false;
    private string colorString = "#DCDDE0";
    
    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TouchedTile()
    {
        image.color = Color.red;
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
