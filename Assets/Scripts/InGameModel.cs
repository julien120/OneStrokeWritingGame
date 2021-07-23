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

    //通知発火
    private readonly Subject<Unit> clearEffect = new Subject<Unit>();
    public IObservable<Unit> IOclearEffect => clearEffect;

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

    private const int MaxCol = 4;
    private const int MaxRow = 4;
    private bool quickFlg = true;
    private bool nextflg = false;
    private PointerEventData pointer;

    private StageData.StageDataFormat[] stageDatas;
    private int stagedataidx = 0;
    private bool stageIndexFlg = true;

    void Start()
    {
        tileQueue = new Tile[MaxRow, MaxCol];
        Initialize();

        ResetStageIndex();
        LoadLevelDesignModel();
        SetLevelDesignModel();
        //SetDepressionPosData();
    }


    public void Initialize()
    {
        gameProcList = new Dictionary<GameState, gameProc> {
            { GameState.IDLE, Idle },
            { GameState.MATCH, MatchAllTile},
            { GameState.VICTORY, SetVictryeffects},
            { GameState.LOSER, SetLoserffects},
            { GameState.RESULT, Result},
            };
        gamestate = GameState.IDLE;


        SetStageData();
        pointer = new PointerEventData(EventSystem.current);
    }

    private void SetStageData()
    {
        for (var i = 0; i < tileArray.Length; i++)
        {
            tileList.Add(tileArray[i]);
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

    private void SetDepressionPosData()
    {
        index = 0;
        for (var i = 0; i < MaxCol; i++)
        {
            for (var j = 0; j < MaxRow; j++)
            {
                if (tileQueue[stageDatas[stagedataidx].depressionxPos, stageDatas[stagedataidx].depressionyPos] == tileArray[index])
                {
                    tileList.Remove(tileArray[index]);
                    tileArray[index].gameObject.SetActive(false);
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
        tileQueue[i, j].StartTilePos();
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
            MatchGetMousePosition();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            gamestate = GameState.MATCH;
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
                            tileQueue[i, j].TouchedTile();
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
                                tileQueue[indexI, indexJ].BackedTile();
                                prevIndexList.RemoveAt(prevIndexList.Count - 1);
                                indexI = prevIndexList[prevIndexList.Count - 1].Item1;
                                indexJ = prevIndexList[prevIndexList.Count - 1].Item2;
                            }
                            return;
                        }

                        if (quickFlg) { return; }
                        //進む場合
                        if (tileQueue[Mathf.Clamp(indexI + 1, 0, 3), indexJ] == tileQueue[i, j] ||
                            tileQueue[Mathf.Clamp(indexI - 1, 0, 3), indexJ] == tileQueue[i, j] ||
                            tileQueue[indexI, Mathf.Clamp(indexJ + 1, 0, 3)] == tileQueue[i, j] ||
                            tileQueue[indexI, Mathf.Clamp(indexJ - 1, 0, 3)] == tileQueue[i, j])
                        {
                            tileQueue[i, j].TouchedTile();
                            indexI = i;
                            indexJ = j;
                            prevIndexList.Add((i, j));
                        }
                    }
                }
            }
        }
    }

    private void SetVictryeffects()
    {
        stageIndexFlg = true;
        inGamePanel.raycastTarget = false;
        //clearEffectの通知が終わった段階じゃなくて、clearEffectの処理が終わった段階でステイト変更する方法とは？
        //UniTaskでやるの？
        clearEffect.OnNext(Unit.Default);
        //gamestate = GameState.RESULT;
        DOVirtual.DelayedCall(1.0f, () => {gamestate = GameState.RESULT;});  
    }

    private void SetLoserffects()
    {
        inGamePanel.raycastTarget = false;
        gamestate = GameState.IDLE;
    }

    private void Result()
    {
        inGamePanel.raycastTarget = false;
        nextflg = true;
        DOVirtual.DelayedCall(0.3f, () => {
            CountStageIndex();
            SetLevelDesignModel();
            gamestate = GameState.IDLE;
        });
    }

    private void MatchAllTile()
    {
        inGamePanel.raycastTarget = false;
        if (tileList.Any(x => !x.flg))
        {
            matchText.text = "一筆書き失敗";
            DOVirtual.DelayedCall(0.5f, () => gamestate = GameState.LOSER);
        }
        else
        {
            matchText.text = "一筆書き成功";
            DOVirtual.DelayedCall(0.5f, () => gamestate = GameState.VICTORY);
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
            if (cols.Length != 5) continue;

            stagecsvdata.Add(
                new StageData.StageDataFormat(
                    int.Parse(cols[0]),
                    int.Parse(cols[1]),
                    int.Parse(cols[2]),
                    int.Parse(cols[3]),
                    int.Parse(cols[4])
                    )
                );
        }
        stageDatas = stagecsvdata.ToArray();

    }

    private void CountStageIndex()
    {
        if (stageIndexFlg && nextflg)
        {
            stagedataidx++;
            stageIndexFlg = false;
        }
    }

    //todo:
    private void SetLevelDesignModel()
    {
        if (stagedataidx >= stageDatas.Length)
        {
            //todo:ステージ上限まで行ったらステージ０に戻る
            matchText.text = "全てのステージクリア";
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
            tileList.Remove(tileArray[i]);
        }
        SetStageData();
        for (var i = 0; i < MaxCol; i++)
        {
            for (var j = 0; j < MaxRow; j++)
            {
                tileQueue[i, j].gameObject.SetActive(true);
                tileQueue[i, j].BackedTile();
            }
        }

        SetDepressionPosData();
    }

}
