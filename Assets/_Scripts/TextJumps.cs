using TMPro;
using UnityEngine;

public class TextJumps : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private int _countOfJumps = 1;
    public int CountOfJumps => _countOfJumps;

    // Start is called before the first frame update
    void Start()
    {
        if (CommonSettings.Instance)
            _countOfJumps = CommonSettings.Instance.Data.countOfSecondJump > 1 ? CommonSettings.Instance.Data.countOfSecondJump : 1;
        text.SetText("{0}", _countOfJumps);
    }

    private void OnEnable() => EventSystem.PlayerJumpedTwice += ChangeCountOfJumps;

    private void OnDisable() => EventSystem.PlayerJumpedTwice -= ChangeCountOfJumps;

    public void ChangeCountOfJumps(int jump)
    {
        _countOfJumps = _countOfJumps + jump < 0 ? 0 : _countOfJumps + jump;
        if (CommonSettings.Instance && jump < 0 && CommonSettings.Instance.Data.countOfSecondJump > 0)
            CommonSettings.Instance.Data.countOfSecondJump += jump;
        text.SetText("{0}", _countOfJumps);
    }
}
