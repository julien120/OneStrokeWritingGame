using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

public class InGameView : MonoBehaviour
{
    //クリアエフェクト
    [SerializeField] private ParticleSystem[] clearParticleSystem;

    //リザルトパネル
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Button nextButton;

    //通知
    private readonly Subject<Unit> loadStage = new Subject<Unit>();
    public IObservable<Unit> IOloadStage => loadStage;

    void Start()
    {
        nextButton.onClick.AddListener(() =>
        {
            DOVirtual.DelayedCall(0.3f, () => resultPanel.SetActive(false));
            loadStage.OnNext(Unit.Default);
        });
    }

    public void SetClearEffect()
    {
        clearParticleSystem[1].Play();
        clearParticleSystem[2].Play();
        clearParticleSystem[0].Play();
    }

    public void OpenResultPanel()
    {
        resultPanel.SetActive(true);
    }
}

