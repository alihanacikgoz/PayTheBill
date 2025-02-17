using System;
using System.Collections;
using GoogleMobileAds.Api;
using NaughtyAttributes;
using UnityEngine;
using AdRequest = GoogleMobileAds.Api.AdRequest;


namespace Runtime.Controllers
{
    
    public class AdController : MonoBehaviour
    {
        #region Variables

        [Foldout("Ad Counter"), SerializeField] private float countdownTime;
        [Foldout("Ad Counter"), SerializeField] private float initialTime;

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private bool _isPremium ;

#if UNITY_ANDROID
        private string _adBannerId = "ca-app-pub-6309338851156090/5923370973";
        private string _adInterstitialId = "ca-app-pub-6309338851156090/5806948769";
#elif UNITY_IPHONE
        private string _adBannerId = "ca-app-pub-6309338851156090/8280498049";
        private string _adInterstitialId = "ca-app-pub-6309338851156090/4588665049";
#else
        private string _adBannerId = "unexpected_platform";
        private string _adInterstitialId = "unexpected_platform";
#endif

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            CheckPremium();
            initialTime = countdownTime;
            MobileAds.Initialize((InitializationStatus initStatus) => { });
        }

        void Start()
        {
            LoadAd();
            CreateInterstitialAd();
            ShowInterstitialAd();
            ListenToBannerAdEvents();
        }
        
        private void Update()
        {
            ListenToInterstitialAdEvents();
        }
        
        #endregion
        
        
        private void CheckPremium()
        {
            gameObject.GetComponent<AdController>().enabled = !_isPremium? true: false;
        }

        #region BannerView Methods

        private void CreateBannerView()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
            }

            _bannerView = new BannerView(_adBannerId, AdSize.IABBanner, 0, 60);
        }

        private void LoadAd()
        {
            if (_bannerView == null)
            {
                CreateBannerView();
            }

            var adRequest = new AdRequest();

            if (_bannerView != null) _bannerView.LoadAd(adRequest);
        }

        private void ListenToBannerAdEvents()
        {
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.Log("Banner view failed to load an ad with error : " + error);
            };

            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
            };

            _bannerView.OnAdImpressionRecorded += () => { Debug.Log("Banner view impression recorded."); };

            _bannerView.OnAdClicked += () => { Debug.Log("Banner view clicked."); };

            _bannerView.OnAdFullScreenContentOpened += () => { Debug.Log("Banner view full screen content opened."); };

            _bannerView.OnAdFullScreenContentClosed += () => { Debug.Log("Banner view full screen content closed."); };
        }

        #endregion

        #region Interstitial Ad Methods

        private void ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Show Interstitial Ad");
                _interstitialAd.Show();
            }
            else
            {
                Debug.Log("Show Interstitial Ad Failed");
            }
        }

        private void CreateInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            var adRequest = new AdRequest();

            InterstitialAd.Load(_adInterstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                _interstitialAd = ad;
            });
        }

        private void ListenToInterstitialAdEvents()
        {
            _interstitialAd.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad clicked.");
            };

            _interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
            };

            _interstitialAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad impression recorded.");
            };

            _interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Time.timeScale = 1;
                Debug.Log("Interstitial ad full screen content closed.");
                CreateInterstitialAd();
                StartCoroutine(IntersitialCountdownCoroutine());
                countdownTime = initialTime;
            };

            _interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.Log("Interstitial ad full screen content failed with error code : " + error);
            };
        }

        #endregion
        
        #region Countdown Coroutine Methods

        private IEnumerator IntersitialCountdownCoroutine()
        {
            yield return new WaitForSeconds(countdownTime);
            ShowInterstitialAd();
        }
        
        #endregion
    }
}