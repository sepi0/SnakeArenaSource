using UnityEngine.Advertisements;
using UnityEngine;

public class InitializeAdsScript : MonoBehaviour {
    private string gameId = "3782245";
    private bool testMode = false;

    private void Start() {
        Advertisement.Initialize( gameId, testMode );
        Debug.Log( "Ads initialized" );
    }

    public static void ShowInterstitialAd() {
        if (Advertisement.IsReady()) {
            Advertisement.Show();
        } 
        else {
            Debug.Log("Interstitial ad not ready at the moment! Please try again later!");
        }
    }
}