using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

public class InGamePresenter : MonoBehaviour
{

    [SerializeField] private InGameModel inGameModel;
    [SerializeField] private InGameView inGameView;


    // Start is called before the first frame update
    void Start()
    {
        inGameModel.IOclearEffect.Subscribe(_ => inGameView.SetClearEffect());
        inGameModel.IOsetResultPanel.Subscribe(_ => inGameView.OpenResultPanel());
        inGameView.IOloadStage.Subscribe(_ => inGameModel.LoadNextStage());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
