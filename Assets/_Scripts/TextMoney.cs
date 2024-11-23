using TMPro;
using UnityEngine;

public class TextMoney : MonoBehaviour
{
    [SerializeField] private CameraShake camera;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private FloatingText floatingText;

    private int _salary = 0;
    private bool _isShaked;

    private int _difficultyIndex = 1000;

    private void OnEnable() => EventSystem.SalaryChanged += ChangeSalary;

    private void OnDisable() => EventSystem.SalaryChanged -= ChangeSalary;

    public void ChangeSalary(int difference)
    {
        floatingText.StartFloatingNumber(difference);
        _salary = _salary + difference < 0 ? 0 : _salary + difference;

        if(CommonSettings.Instance)
        {
            CommonSettings.Instance.Data.currentScore = _salary;
            if (CommonSettings.Instance.Data.recordScore < _salary)
            {
                if (CommonSettings.Instance.Data.recordScore == 0) _isShaked = true;
                if (!_isShaked)
                {
                    camera.ShakeCamera();
                    _isShaked = true;
                }
                CommonSettings.Instance.Data.isBreakeRecord = true;
                CommonSettings.Instance.Data.recordScore = _salary;
            }
        }
            
        text.SetText("{0}", _salary);

        if (_salary > _difficultyIndex)
        {
            _difficultyIndex += 1000;
            EventSystem.OnDifficultyChanged();
        }
    }
}