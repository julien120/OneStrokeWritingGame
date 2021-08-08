using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveController : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private float baseWidth = 9.0f;
    [SerializeField] private float baseHeight = 16.0f;

    void Awake()
    {
        // ベース維持
        var scaleWidth = (Screen.height / this.baseHeight) * (this.baseWidth / Screen.width);
        var scaleRatio = Mathf.Max(scaleWidth, 1.0f);
        this.camera.fieldOfView = Mathf.Atan(Mathf.Tan(this.camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * scaleRatio) * 2.0f * Mathf.Rad2Deg;
    }
}
