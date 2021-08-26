using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class AdsManager : MonoBehaviour
{
    //ID
    private static string BANNER_PLACEMENT_ID = "Banner";
    private const string iosID = "4271708";
    private const string androidID = "4271709";


    //動画id
    private static string INTERSITIAL_PLACEMENT_ID = "Interstitial";

    [SerializeField] private InterstitialAdExample interstitialAd;

    void Awake()
    {
#if UNITY_ANDROID
                string gameID = androidID;
                BANNER_PLACEMENT_ID = "Banner_Android";
                INTERSITIAL_PLACEMENT_ID = "Interstitial_Android";
#else
        string gameID = iosID;
        BANNER_PLACEMENT_ID = "Banner_iOS";
        INTERSITIAL_PLACEMENT_ID = "Interstitial_iOS";
#endif

        //広告の初期化
        Advertisement.Initialize(gameID, testMode: false, enablePerPlacementLoad: true);
        //Advertisement.Initialize(gameID, testMode: true, enablePerPlacementLoad: true);
        //Advertisement.Initialize(gameID, testMode: false, enablePerPlacementLoad: true);
        StartCoroutine(ShowBannerWhenInitialized());

    }
    private void Start()
    {
        // ShowBannerAd();
        //LoadHomeScene().Forget();
    }



    IEnumerator ShowBannerWhenInitialized()
    {
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.3f);
        }
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(BANNER_PLACEMENT_ID);
    }

    /// <summary>
    /// バナー広告の表示
    /// </summary>
    public void ShowBannerAd()
    {
        //広告全体の準備が出来ているかチェック
        if (!Advertisement.IsReady())
        {
            Debug.LogWarning("広告全体の準備が出来ていません");
            return;
        }

        //表示したい広告の準備が出来ているかチェック
        var state = Advertisement.GetPlacementState(BANNER_PLACEMENT_ID);
        if (state != PlacementState.Ready)
        {
            Debug.LogWarning($"{BANNER_PLACEMENT_ID}の準備が出来ていません。現在の状態 : {state}");
            return; ;
        }

        //バナー広告の表示場所の設定
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);

        //バナー広告の表示
        Advertisement.Banner.Show(BANNER_PLACEMENT_ID);

    }

    /// <summary>
    /// バナー広告の非表示
    /// </summary>
    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    //private async UniTaskVoid LoadHomeScene()
    //{
    //    await image.DOScale(0f, 0.01f).SetEase(Ease.Linear).AsyncWaitForCompletion();
    //    await DOVirtual.DelayedCall(1.4f, () => image.DOScale(7f, 0.4f).SetEase(Ease.Linear)).AsyncWaitForCompletion();
    //    //  await 
    //    await DOVirtual.DelayedCall(0.5f, () => SceneController.Instance.LoadHomeScene()).AsyncWaitForCompletion();

    //}

    public void ShowAdsMove()
    {
        //if (!Advertisement.IsReady())
        //{
        //    Debug.LogWarning("動画広告の準備が出来ていません");
        //    return;
        //}

        ////表示したい広告の準備が出来ているかチェック
        //var state = Advertisement.GetPlacementState(INTERSITIAL_PLACEMENT_ID);
        //if (state != PlacementState.Ready)
        //{
        //    Debug.LogWarning($"{INTERSITIAL_PLACEMENT_ID}の準備が出来ていません。現在の状態 : {state}");
        //    return; ;
        //}

        interstitialAd.LoadAd();
        interstitialAd.ShowAd();
        Debug.Log("動画");
    }
}
