using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameView : MonoBehaviour
{
    //クリアエフェクト
    [SerializeField] private ParticleSystem clearParticleSystem;

    void Start()
    {
       
    }

    void Update()
    {
        
    }

    public void SetClearEffect()
    {
        clearParticleSystem.Play();
    }
}

