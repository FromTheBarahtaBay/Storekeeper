using TMPro;
using UnityEngine;
using GamePush;

public class TextLives : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private int _countOfLives = 1;

    private void OnEnable() => EventSystem.LivesСhanged += ChangeCountOfLifes;

    private void OnDisable() => EventSystem.LivesСhanged -= ChangeCountOfLifes;

    private void Start()
    {
        GP_Game.GameplayStart();
        if (CommonSettings.Instance)
            _countOfLives = CommonSettings.Instance.Data.countOfLives > 1 ? (CommonSettings.Instance.Data.countOfLives >= 3 ? 3 : CommonSettings.Instance.Data.countOfLives) : 1;
        text.SetText("{0}", _countOfLives);
    }

    public void ChangeCountOfLifes(int live)
    {
        _countOfLives = _countOfLives + live;
        if (CommonSettings.Instance && live < 0 && CommonSettings.Instance.Data.countOfLives > 0)
            CommonSettings.Instance.Data.countOfLives += live;

        if (_countOfLives < 0)
        {
            if (CommonSettings.Instance && CommonSettings.Instance.Data.isBreakeRecord && CommonSettings.Instance.Data.currentScore > CommonSettings.Instance.Data.recordScore)
                CommonSettings.Instance.Data.recordScore = CommonSettings.Instance.Data.currentScore;
            GP_Game.GameplayStop();
            CommonSettings.SetSetting();
            EventSystem.OnGameOverHappened();
            return;
        }
            
        text.SetText("{0}", _countOfLives);
    }
}