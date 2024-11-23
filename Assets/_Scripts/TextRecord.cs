using TMPro;
using UnityEngine;

public class TextRecord : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        Invoke("ShowRecord", 0.5f);
    }

    void ShowRecord()
    {
        if (CommonSettings.Instance)
            text.SetText("{0}", CommonSettings.Instance.Data.recordScore);
    }
}