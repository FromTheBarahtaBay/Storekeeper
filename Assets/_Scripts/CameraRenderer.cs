using UnityEngine;

public class CameraRenderer : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Vector2 DefaultResolution = new Vector2(2072, 2732);

    private float initialSize;
    private float targetAspect;
    private float initialFov;

    private void OnValidate()
    {
        _mainCamera ??= GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
    }

    private void Awake()
    {
        ScreenSettingsFind();
        SaveInitialValues();
        CameraResolutionChange();
    }

    private void SaveInitialValues()
    {
        initialSize = _mainCamera.orthographicSize;
        initialFov = _mainCamera.fieldOfView;
    }

    public void ScreenSettingsFind()
    {
        targetAspect = DefaultResolution.x / DefaultResolution.y;
    }

    public void CameraResolutionChange()
    {
        //Debug.Log("!!! CameraResolutionChange !!!");
        float WidthOrHeight = (Screen.width > Screen.height) ? 1 : 0;
        float horizontalFov = 120f;

        if (_mainCamera.orthographic)
        {
            float constantWidthSize = initialSize * (targetAspect / _mainCamera.aspect);
            if (_mainCamera != null)
            _mainCamera.orthographicSize = Mathf.Round(Mathf.Lerp(constantWidthSize, initialSize, WidthOrHeight));
        }
        else
        {
            float constantWidthFov = CalcVerticalFov(horizontalFov, _mainCamera.aspect);
            if (_mainCamera != null)
                _mainCamera.fieldOfView = Mathf.Lerp(constantWidthFov, initialFov, WidthOrHeight);
        }
    }

    private float CalcVerticalFov(float hFovInDeg, float aspectRatio)
    {
        float hFovInRads = hFovInDeg * Mathf.Deg2Rad;
        float vFovInRads = 2 * Mathf.Atan(Mathf.Tan(hFovInRads / 2) / aspectRatio);
        return vFovInRads * Mathf.Rad2Deg;
    }
}