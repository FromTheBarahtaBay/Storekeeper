using UnityEngine;
using UnityEngine.SceneManagement;
using GamePush;

public class UIMainMenu : MonoBehaviour
{
    private void Start()
    {
        GP_Game.GameplayStop();
        GP_Ads.RefreshSticky();
    }

    public void OnStartGameButton()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void OnShowRewardsButton()
    {
        ShowRewarded();
    }

    // Показать rewarded video
    public void ShowRewarded() => GP_Ads.ShowRewarded("BUST", OnRewardedReward, OnRewardedStart, OnRewardedClose);


    // Начался показ
    private void OnRewardedStart() => Debug.Log("ON REWARDED: START");
    // Получена награда
    private void OnRewardedReward(string value)
    {
        if (value == "BUST")
        {
            if (!CommonSettings.Instance) return;
            CommonSettings.Instance.Data.countOfLives += 3;
            CommonSettings.Instance.Data.countOfSecondJump += 3;
            CommonSettings.SetSetting();
            Debug.Log("ON REWARDED: +BUST");
        }
    }

    // Закончился показ
    private void OnRewardedClose(bool success) => Debug.Log("ON REWARDED: CLOSE");
}