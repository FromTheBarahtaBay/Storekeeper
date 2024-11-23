using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private GameObject gameObj;
    
    private float _waitingTime;
    private float _hideTime = 1f;
    private StringBuilder stringBuilder = new StringBuilder();

    public void StartFloatingNumber(int number)
    {
        StartCoroutine(StartText(number));
    }

    private IEnumerator StartText(int number)
    {
        if (number > 0) stringBuilder.Append("+");
        gameObj.SetActive(true);
        _waitingTime = Time.time;
        stringBuilder.Append(number);
        textMesh.SetText(stringBuilder);

        while (Time.time - _waitingTime < _hideTime)
        {
            yield return new WaitForFixedUpdate();
        }
        stringBuilder.Clear();
        gameObj.SetActive(false);
    }
}