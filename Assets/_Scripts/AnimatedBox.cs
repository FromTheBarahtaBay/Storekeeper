using UnityEngine;

public class AnimatedBox : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void HideBoxAnimated()
    {
        this.gameObject.SetActive(false);
    }
}
