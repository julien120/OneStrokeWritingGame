using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using System.IO;
using UniRx;

public class InGameModel : MonoBehaviour
{
    //Tile
    [SerializeField] private Tile[] tileArray;
    private List<Tile> tileList = new List<Tile>();
    private Tile[,] tileQueue;
    private int index = 0;

    //タイルの座標情報
    private int indexI;
    private int indexJ;
    private List<(int, int)> prevIndexList = new List<(int, int)>();
    private (int, int) startTilePos;

    private List<(float,float,float,float)> tileOffset = new List<(float,float,float,float)>();

    //通知発火
    private readonly Subject<Unit> clearEffect = new Subject<Unit>();
    public IObservable<Unit> IOclearEffect => clearEffect;

    private readonly Subject<Unit> setResultPanel = new Subject<Unit>();
    public IObservable<Unit> IOsetResultPanel => setResultPanel;

    //GUI
    [SerializeField] private Image inGamePanel;

    //ステートマシーン
    private GameState gamestate = GameState.IDLE;
    delegate void gameProc();
    Dictionary<GameState, gameProc> gameProcList;

    //デバック用
    [SerializeField] private Text stateText;
    [SerializeField] private Text matchText;
    [SerializeField] private Text stageText;

    private const int MaxCol = 5;
    private const int MaxRow = 4;
    private bool quickFlg = true;
    private bool nextflg = false;
    private PointerEventData pointer;

    private StageData.StageDataFormat[] stageDatas;
    private StageData.DeleteDataFormat[] deleteDatas;
    private int stagedataidx = 0;
    private bool stageIndexFlg = true;

    private int nextStageCount = 1;

    void Start()
    {
        tileQueue = new Tile[ MaxCol,MaxRow];
        Initialize();

        //ResetStageIndex();
        
        LoadLevelDesignModel();
        SetLevelDesignModel();
        SetDepressionPosData();
    }


    public void Initialize()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKeyName.ClearStageIndexData) == false)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeyName.ClearStageIndexData, 9);
        }

        gameProcList = new Dictionary<GameState, gameProc> {
            { GameState.IDLE, Idle },
            { GameState.MATCH, MatchAllTile},
            { GameState.VICTORY, SetVictryeffects},
            { GameState.LOSER, SetLoserffects},
            { GameState.RESULT, Result},
            {GameState.CLEAR,Clear },
            };
        gamestate = GameState.IDLE;

        LoadSaveData();
        SetStageData();
        LoadDeleteTileModel();
        pointer = new PointerEventData(EventSystem.current); 
    }


    private void SetStageData()
    {
        for (var i = 0; i < tileArray.Length; i++)
        {
            tileList.Add(tileArray[i]);
            var hoge = tileArray[i].GetComponent<RectTransform>().offsetMax;
            var koge = tileArray[i].GetComponent<RectTransform>().offsetMin;
            tileOffset.Add((hoge.x, hoge.y, koge.x, koge.y));
        }
        for (var i = 0; i < MaxCol; i++)
        {
            for (var j = 0; j < MaxRow; j++)
            {
                tileQueue[i, j] = tileArray[index];
                index++;
            }
        }
    }

    private int stageIndex = 0;
    /// <summary>
    /// 
    /// </summary>
    private void SetDepressionPosData()
    {
       index = 0;
       stageIndex = 0;
        for (var i = 0; i < MaxCol; i++)
        {
            for (var j = 0; j < MaxRow; j++)
            {
                if (tileQueue[deleteDatas[stageIndex].xPos, deleteDatas[stageIndex].yPos] == tileArray[index])
                {
                    tileList.Remove(tileArray[index]);
                    tileArray[index].gameObject.SetActive(false);

                    stageIndex++;
                    if (deleteDatas.Length == stageIndex) return;
                }
                index++;
            }
        }
    }

    /// <summary>
    /// ステージ設計の仕方は分からないが一旦csvで作成する
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private void InGameStart(int i,int j)
    {
        tileQueue[i, j].StartTilePos(stageDatas[stagedataidx].stageIndex);
        startTilePos = (i, j);
    }


    void Update()
    {
        gameProcList[gamestate]();
        stateText.text = gamestate.ToString();
    }

    private void Idle()
    {
        inGamePanel.raycastTarget = true;

        if (Input.GetMouseButton(0))
        {
            flgf = true;
            MatchGetMousePosition();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            gamestate = GameState.MATCH;
            return;
        }     
    }

    /// <summary>
    /// 一筆書き機能と戻る機能
    /// </summary>
    private void MatchGetMousePosition()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        pointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointer, results);

        foreach (RaycastResult target in results)
        {
            for (var i = 0; i < MaxCol; i++) {
                for (var j = 0; j < MaxRow; j++)
                {
                    //csvで読み込んだ座標に対応するマップをfalseにし、そしてi,j番号と一致するtileListの値を削除
                    //複雑な形状にするときはこれでreturnし、
                    //ステージ更新時に地形マップをリセットし読み込みする
                    //todo: if (target.gameObject.GetComponent<Tile>() == tileQueue[i, j])[return;]
                    if (target.gameObject.GetComponent<Tile>() == tileQueue[i, j])
                    {
                        //初回のみ
                        if (target.gameObject.GetComponent<Tile>() == tileQueue[startTilePos.Item1, startTilePos.Item2] && quickFlg)
                        {
                            indexI = i;
                            indexJ = j;
                            prevIndexList.Add((indexI, indexJ));
                            tileQueue[i, j].DrawHeadTile(stageDatas[stagedataidx].stageIndex);
                            quickFlg = false;
                            return;
                        }

                        //戻る場合
                        if (tileQueue[i, j].flg)
                        {
                            if (tileQueue[i, j] == tileQueue[indexI, indexJ] && tileQueue[indexI, indexJ].flg) { return; }

                            
                            if (tileQueue[i, j] == tileQueue[prevIndexList[prevIndexList.Count - 2].Item1, prevIndexList[prevIndexList.Count - 2].Item2]
                                &&tileQueue[i, j].flg)
                            {
                                tileQueue[indexI, indexJ].BackedTile(stageDatas[stagedataidx].stageIndex);
                                //tileQueue[indexI, indexJ].GetComponent<RectTransform>().offsetMax = new Vector2(tileQueue[indexI, indexJ].tileOffset.Item1, tileQueue[indexI, indexJ].tileOffset.Item2);
                                //tileQueue[indexI, indexJ].GetComponent<RectTransform>().offsetMin = new Vector2(tileQueue[indexI, indexJ].tileOffset.Item3, tileQueue[indexI, indexJ].tileOffset.Item4);
                                tileQueue[i, j].DrawHeadTile(stageDatas[stagedataidx].stageIndex);

                                prevIndexList.RemoveAt(prevIndexList.Count - 1);
                                indexI = prevIndexList[prevIndexList.Count - 1].Item1;
                                indexJ = prevIndexList[prevIndexList.Count - 1].Item2;
                            }
  
                            return;
                        }

                        if (quickFlg) { return; }
                        //進む場合
                        if (tileQueue[Mathf.Clamp(indexI + 1, 0, MaxCol-1), indexJ] == tileQueue[i, j] ||
                            tileQueue[Mathf.Clamp(indexI - 1, 0, MaxCol - 1), indexJ] == tileQueue[i, j] ||
                            tileQueue[indexI, Mathf.Clamp(indexJ + 1, 0, MaxRow - 1)] == tileQueue[i, j] ||
                            tileQueue[indexI, Mathf.Clamp(indexJ - 1, 0, MaxRow - 1)] == tileQueue[i, j])
                        {
                            tileQueue[indexI, indexJ].TouchedTile(stageDatas[stagedataidx].stageIndex);
          
                            tileQueue[i, j].DrawHeadTile(stageDatas[stagedataidx].stageIndex);


                            if (indexJ<j)
                            {
                                //右に移動
                                    var rectX = tileQueue[i, j].GetComponent<RectTransform>().offsetMin.x;
                                    var rectY = tileQueue[i, j].GetComponent<RectTransform>().offsetMin.y;
                                    //tileQueue[i, j].GetComponent<RectTransform>().offsetMin = new Vector2(rectX-40, rectY);
                                    tileQueue[i, j].leftImage.gameObject.SetActive(true);
                                    tileQueue[i, j].DrawSideColor(tileQueue[i, j].leftImage, stageDatas[stagedataidx].stageIndex);
                            }
                            if (indexJ > j)
                            {
                                //左
                                    var rectX = tileQueue[i, j].GetComponent<RectTransform>().offsetMax.x;
                                    var rectY = tileQueue[i, j].GetComponent<RectTransform>().offsetMax.y;
                                    //tileQueue[i, j].GetComponent<RectTransform>().offsetMax = new Vector2(rectX + 40, rectY);
                                    tileQueue[i, j].rightImage.gameObject.SetActive(true);
                                    tileQueue[i, j].DrawSideColor(tileQueue[i, j].rightImage, stageDatas[stagedataidx].stageIndex);
                            }
                            if (indexI < i)
                            {
                                //下
                                    var rectX = tileQueue[i, j].GetComponent<RectTransform>().offsetMax.x;
                                    var rectY = tileQueue[i, j].GetComponent<RectTransform>().offsetMax.y;
                                    //tileQueue[i, j].GetComponent<RectTransform>().offsetMax = new Vector2(rectX, rectY+40);
                                    tileQueue[i, j].topImage.gameObject.SetActive(true);
                                    tileQueue[i, j].DrawSideColor(tileQueue[i, j].topImage, stageDatas[stagedataidx].stageIndex);
                            }
                            if(indexI > i)
                            {
                                //上
                                var rectX = tileQueue[i, j].GetComponent<RectTransform>().offsetMin.x;
                                var rectY = tileQueue[i, j].GetComponent<RectTransform>().offsetMin.y;
                                //tileQueue[i, j].GetComponent<RectTransform>().offsetMin = new Vector2(rectX, rectY - 40);
                                tileQueue[i, j].bottomImage.gameObject.SetActive(true);
                                tileQueue[i, j].DrawSideColor(tileQueue[i, j].bottomImage, stageDatas[stagedataidx].stageIndex);
                            }
                            indexI = i;
                            indexJ = j;
                            prevIndexList.Add((i, j));
                        }

                    }
                }
            }
        }
    }

    bool effectFlg = true;
    private void SetVictryeffects()
    {
        if(effectFlg)
        {
             effectFlg = false;
            stageIndexFlg = true;
             inGamePanel.raycastTarget = false;
            //clearEffectの通知が終わった段階じゃなくて、clearEffectの処理が終わった段階でステイト変更する方法とは？
            //UniTaskでやるの？
            clearEffect.OnNext(Unit.Default);
            //gamestate = GameState.RESULT;
            DOVirtual.DelayedCall(0.5f, () => { setResultPanel.OnNext(Unit.Default); });
        }
    }
    public void LoadNextStage()
    {
        effectFlg = true;
       gamestate = GameState.RESULT;
    }

    private void SetLoserffects()
    {
        inGamePanel.raycastTarget = false;
        gamestate = GameState.IDLE;
    }

    private void Result()
    {
        if (stagedataidx >= stageDatas.Length)
        {
            Debug.Log("最後");
            //todo:ステージ上限まで行ったらステージ０に戻る
            matchText.text = "全てのステージクリア";
            inGamePanel.raycastTarget = false;
            gamestate = GameState.CLEAR;

            return;
        }
        inGamePanel.raycastTarget = false;
        nextflg = true;
        DOVirtual.DelayedCall(0.3f, () => {
            CountStageIndex();
            SetLevelDesignModel();
            gamestate = GameState.IDLE;
        });
    }

    bool flgf = true;
    private void MatchAllTile()
    {
        inGamePanel.raycastTarget = false;
        if (flgf) {
            flgf = false;
            if (tileList.Any(x => !x.flg))
            {
                matchText.text = "一筆書き失敗";
                DOVirtual.DelayedCall(0.3f, () => gamestate = GameState.LOSER);
            }
            else
            {
                matchText.text = "一筆書き成功";
                stageDataIndex++;
                nextStageCount = stageDataIndex;
                UpdateStageLevelSelection();
                DOVirtual.DelayedCall(0.3f, () => gamestate = GameState.VICTORY);
            }
        }
    }

    /// <summary>
    /// ステージごとにシーンを作成しない場合は以下の通り
    /// csvから毎ステージごとに参照するPrefab名、スタート座標を取得する
    /// ステージ番号,スタートx座標,スタートy座標,陥没x座標,陥没y座標
    /// </summary>
    private void LoadLevelDesignModel()
    {
        var stagecsvdata = new List<StageData.StageDataFormat>();
        var csvdata = Resources.Load<TextAsset>("StageData").text;

        StringReader sr = new StringReader(csvdata);
        while (sr.Peek() != -1)
        {
            var line = sr.ReadLine();
            var cols = line.Split(',');
            if (cols.Length != 3) continue;

            stagecsvdata.Add(
                new StageData.StageDataFormat(
                    int.Parse(cols[0]),
                    int.Parse(cols[1]),
                    int.Parse(cols[2])
                    )
                );
        }
        stageDatas = stagecsvdata.ToArray();
    }
    private void Clear()
    {
        inGamePanel.raycastTarget = false;
        Debug.Log(gamestate);
    }

    private int stageDataIndex = 1;
    //削除するタイル群
    private void LoadDeleteTileModel()
    {
        
        var index = "DeleteData" + PlayerPrefs.GetInt(PlayerPrefsKeyName.ClearStageIndexData);
//        Debug.Log(index);
        var stagecsvdata = new List<StageData.DeleteDataFormat>();
        var csvdata = Resources.Load<TextAsset>(index).text;

        StringReader sr = new StringReader(csvdata);
        while (sr.Peek() != -1)
        {
            var line = sr.ReadLine();
            var cols = line.Split(',');
            if (cols.Length != 2) continue;

            stagecsvdata.Add(
                new StageData.DeleteDataFormat(
                    int.Parse(cols[0]),
                    int.Parse(cols[1])
                    )
                );
        }
        deleteDatas = stagecsvdata.ToArray();
        
    }

    private void CountStageIndex()
    {
        if (stageIndexFlg && nextflg)
        {

            stagedataidx++;
            nextStageCount = stagedataidx;
            stageIndexFlg = false;
        }
    }

    //todo:
    private void SetLevelDesignModel()
    {
        if (stagedataidx >= stageDatas.Length)
        {
            
            
            return;
        }else { 
            ResetStage();
            InGameStart(stageDatas[stagedataidx].xPos, stageDatas[stagedataidx].yPos);
            stageText.text = stageDatas[stagedataidx].stageIndex.ToString();
        }
    }

    private void ResetStageIndex()
    {
        stagedataidx = 0;
    }

    private void ResetStage()
    {
        quickFlg = true;
        index = 0;

        //todo
        for (var i = 0; i < tileArray.Length; i++)
        {
            tileArray[i].GetComponent<RectTransform>().offsetMax = new Vector2(tileOffset[i].Item1, tileOffset[i].Item2);
            tileArray[i].GetComponent<RectTransform>().offsetMin = new Vector2(tileOffset[i].Item3, tileOffset[i].Item4);
            tileList.Remove(tileArray[i]);

        }
        //stageDataIndex++;
        SetStageData();
        LoadDeleteTileModel();
        for (var i = 0; i < MaxCol; i++)
        {
            for (var j = 0; j < MaxRow; j++)
            {
                tileQueue[i, j].gameObject.SetActive(true);
                tileQueue[i, j].BackedTile(stageDatas[stagedataidx].stageIndex);
            }
        }

        SetDepressionPosData();
    }


    private void UpdateStageLevelSelection()
    {
        if(stageDatas.Length> PlayerPrefs.GetInt(PlayerPrefsKeyName.ClearStageIndexData))
        {
            PlayerPrefs.SetInt(PlayerPrefsKeyName.ClearStageIndexData, stageDatas[stagedataidx].stageIndex+1);
        }else
        {
            PlayerPrefs.SetInt(PlayerPrefsKeyName.ClearStageIndexData, 1);
            stagedataidx = -1;
        }
    }

    private void LoadSaveData()
    {
        stagedataidx = PlayerPrefs.GetInt(PlayerPrefsKeyName.ClearStageIndexData)-1;
        
    }



}
