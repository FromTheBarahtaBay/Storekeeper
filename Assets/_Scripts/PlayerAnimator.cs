using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private StackAttackTilemap _stack;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Push = Animator.StringToHash("Push");
    private static readonly int Hited = Animator.StringToHash("Hited");

    private int _currentState;

    private void Start()
    {
        _animator.CrossFade(Idle, 0, 0);
    }

    private void LateUpdate()
    {
        var state = GetState();
        if (state == _currentState) return;
        _animator.CrossFade(state, 0, 0);
        _currentState = state;
    }

    private int GetState()
    {
        if (_stack.Direction.x > 0)
            _spriteRenderer.flipX = true;
        else if (_stack.Direction.x < 0)
            _spriteRenderer.flipX = false;

        if (_stack.PlayerIsHited)
            return Hited;
        else if (_stack.Direction.y > 0 || _stack.IsFalling)
            return Jump;
        else if (_stack.IsPushing)
            return Push;
        else if (_stack.Direction.x != 0)
            return Walk;
        return Idle;
    }

    public void ReturnPlayerToLife()
    {
        _stack.PlayerIsHited = false;
    }
}