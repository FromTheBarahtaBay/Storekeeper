using UnityEngine;
using System.Threading.Tasks;

public class CameraShake : MonoBehaviour
{
    private float shakeDuration = 0.3f; // Продолжительность тряски
    private float shakeAmount = 0.15f;   // Сила тряски
    private float decreaseFactor = 1.0f; // Коэффициент снижения силы тряски по времени

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    async public void ShakeCamera()
    {
        await Shake(shakeDuration);
    }

    private async Task Shake(float shakeDuration)
    {
        if (this == null) return;

        float elapsedTime = 0.0f;

        var shakeA = shakeAmount;

        while (elapsedTime < shakeDuration)
        {
            if (this == null) return;
            // Генерируем случайное смещение в пределах заданной силы тряски
            Vector3 shakeOffset = Random.insideUnitSphere * shakeAmount;
            transform.localPosition = originalPosition + shakeOffset;

            elapsedTime += Time.deltaTime;

            // Уменьшаем силу тряски по времени
            shakeA *= 1.0f - Time.deltaTime * decreaseFactor;

            await Task.Yield();
        }

        transform.localPosition = originalPosition; // Возвращаем камеру в исходное положение
    }
}