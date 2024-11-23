using GamePush;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [Header("Pause Button")]
    [SerializeField] private Image _pauseButton;
    [SerializeField] private Sprite[] _sprites;

    public void PlayOrPauseGameButton()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            GP_Game.GameplayStart();
            _pauseButton.sprite = _sprites[0];
        }
        else
        {
            SaveData();
            Time.timeScale = 0;
            _pauseButton.sprite = _sprites[1];
            GP_Game.GameplayStop();
        }
    }

    public void OnRestartLevelButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    public void OnBackToMenuButton()
    {
        SaveData();
        ShowFullscreen();
        SceneManager.LoadScene("MenuScene");
    }

    private void SaveData()
    {
        if (CommonSettings.Instance)
            CommonSettings.SetSetting();
    }

    // Показать fullscreen
    public void ShowFullscreen() => GP_Ads.ShowFullscreen(OnFullscreenStart, OnFullscreenClose);

    // Начался показ
    private void OnFullscreenStart() => Debug.Log("ON FULLSCREEN START");
    // Закончился показ
    private void OnFullscreenClose(bool success) => Debug.Log("ON FULLSCREEN CLOSE");
}