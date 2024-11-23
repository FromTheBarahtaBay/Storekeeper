using UnityEngine;

public class CameraSaveArea : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private void Start()
    {
        SaveArea();
    }

    private void SaveArea()
    {
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }

    private void OnRectTransformDimensionsChange()
    {
        SaveArea();
    }
}