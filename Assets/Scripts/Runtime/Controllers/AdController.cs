using System;
using System.Collections.Generic;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine;
using AdRequest = GoogleMobileAds.Api.AdRequest;

namespace Runtime.Controllers
{
    public class AdController : MonoBehaviour
    {
        #region Singleton

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;

#if UNITY_ANDROID
        private string _adBannerId = "ca-app-pub-3940256099942544/6300978111";
        private string _adInterstitialId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        private string _adBannerId = "ca-app-pub-3940256099942544/2934735716";
        private string _adInterstitialId = "ca-app-pub-3940256099942544/4411468910";
#else
        private string _adBannerId = "unexpected_platform";
        private string _adInterstitialId = "unexpected_platform";
#endif

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            MobileAds.Initialize((InitializationStatus initStatus) => { });
        }

        void Start()
        {
            LoadAd();
            CreateInterstitialAd();
            ShowInterstitialAd();
            ListenToBannerAdEvents();
            ListenToInterstitialAdEvents();
        }

        #endregion

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
                Debug.Log("Interstitial ad full screen content closed.");
            };

            _interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.Log("Interstitial ad full screen content failed with error code : " + error);
            };
        }

        #endregion
    }
}