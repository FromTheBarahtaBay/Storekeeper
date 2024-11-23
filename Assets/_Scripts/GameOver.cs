using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI textFrom;
    [SerializeField] private TextMeshProUGUI textTo;

    private void OnEnable() => EventSystem.GameOverHappened += GameOverScript;

    private void OnDisable() => EventSystem.GameOverHappened -= GameOverScript;

    public void GameOverScript()
    {
        CommonSettings.Instance.Data.isBreakeRecord = false;
        gameOverPanel.SetActive(true);
        textTo.text = textFrom.text;
        Time.timeScale = 0f;
    }
}