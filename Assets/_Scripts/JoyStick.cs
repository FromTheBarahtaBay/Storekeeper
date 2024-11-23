using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class JoyStick : MonoBehaviour
{
    [SerializeField] private StackAttackTilemap _stack;
    [SerializeField] private RectTransform _backgroundJump;
    [SerializeField] private RectTransform _backgroundJoystick; 
    [SerializeField] private RectTransform _handleJoystick;
    [SerializeField] private RectTransform _handleJump;
    [SerializeField] private RectTransform[] _squaresForJoystick;
    [SerializeField] private RectTransform[] _squaresForJump;
    [SerializeField] private TextJumps _textJumps;

    private int _currentSquareIndexJoystick = 1;
    private int _currentSquareIndexJump = 1;
    private bool _isHolding;
    private float _stationaryThreshold = 0.01f;

    private Finger _movementFingerOnLeft;
    private Finger _movementFingerOnRight;
    private Vector2 _movementAmount;
    private Vector2 _joystickSize = new Vector2(300, 150);
    private Vector2 _jumpSize = new Vector2(150, 300);

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleFingerUp;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleFingerUp;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable();
    }

    private void HandleFingerDown(Finger touchedFinger)
    {
        if (_movementFingerOnLeft == null && touchedFinger.screenPosition.x <= Screen.width / 2f)
        {
            if (_movementFingerOnLeft == null && touchedFinger.screenPosition.y > Screen.height / 1.55f) return;

            _movementFingerOnLeft = touchedFinger;
            _movementAmount = Vector2.zero;
            _backgroundJoystick.gameObject.SetActive(true);
            _backgroundJoystick.sizeDelta = _joystickSize;
            _backgroundJoystick.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition, _joystickSize);
        }

        if (_movementFingerOnRight == null && touchedFinger.screenPosition.x > Screen.width / 2f)
        {
            _isHolding = true;
            _movementFingerOnRight = touchedFinger;
            _movementAmount = Vector2.zero;
            _backgroundJump.gameObject.SetActive(true);
            _backgroundJump.sizeDelta = _jumpSize;
            _backgroundJump.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition, _jumpSize);
        }
    }

    private Vector2 ClampStartPosition (Vector2 startPosition, Vector2 size)
    {
        if (startPosition.x < size.x / 2)
            startPosition.x = size.x / 2;
        else if (startPosition.x > Screen.width - size.x / 2f)
            startPosition.x = Screen.width - size.x / 2f;
        if (startPosition.y < size.y / 2)
            startPosition.y = size.y / 2;
        else if (startPosition.y > Screen.height - size.y / 2f)
            startPosition.y = Screen.height - size.y / 2f;
        return startPosition;
    }

    private void HandleFingerUp(Finger lostFinger)
    {
        if (lostFinger == _movementFingerOnLeft)
        {
            _movementFingerOnLeft = null;
            _handleJoystick.anchoredPosition = Vector2.zero;
            _backgroundJoystick.gameObject.SetActive(false);
            _movementAmount = Vector2.zero;
            _currentSquareIndexJoystick = 1;
        }

        if (lostFinger == _movementFingerOnRight)
        {
            _isHolding = false;
            if (_currentSquareIndexJump == 1)
                _stack.Jump();
            _movementFingerOnRight = null;
            _handleJump.anchoredPosition = Vector2.zero;
            _backgroundJump.gameObject.SetActive(false);
            _movementAmount = Vector2.zero;
            _currentSquareIndexJump = 1;
        }
    }

    private void HandleFingerMove(Finger movedFinger)
    {
        if (movedFinger == _movementFingerOnLeft)
        {
            _currentSquareIndexJoystick = HandleFingerMoveMethod(movedFinger, _backgroundJoystick, _currentSquareIndexJoystick);
            _handleJoystick.anchoredPosition = _squaresForJoystick[_currentSquareIndexJoystick].anchoredPosition;
        }
        else if (movedFinger == _movementFingerOnRight)
        {
            _currentSquareIndexJump = HandleFingerMoveMethod(movedFinger, _backgroundJump, _currentSquareIndexJump);
            _handleJump.anchoredPosition = _squaresForJump[_currentSquareIndexJump].anchoredPosition;
            if (_stack.PlayerIsMoving)
                HandleFingerUp(_movementFingerOnRight);

            if (movedFinger.currentTouch.delta.magnitude > _stationaryThreshold)
                _isHolding = false;
            else
                _isHolding = true;
        }
    }

    private int HandleFingerMoveMethod(Finger movedFinger, RectTransform background, int currentSquareIndex)
    {
        Vector2 handlePosition;
        float maxMovement = _joystickSize.x / 2;
        ETouch.Touch currentTouch = movedFinger.currentTouch;

        // Рассчет позиции handle на основе касания
        if (Vector3.Distance(currentTouch.screenPosition, background.anchoredPosition) > maxMovement)
        {
            handlePosition = maxMovement * (currentTouch.screenPosition - background.anchoredPosition).normalized;
        }
        else
        {
            handlePosition = currentTouch.screenPosition - background.anchoredPosition;
        }

        // Поиск ближайшего квадрата
        Vector2 clampedPosition = handlePosition;
        float minDistance = float.MaxValue;
        int closestSquareIndex = currentSquareIndex;

        for (int i = 0; i < _squaresForJoystick.Length; i++)
        {
            RectTransform square = _squaresForJoystick[i];
            float distance = Vector2.Distance(clampedPosition, square.anchoredPosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestSquareIndex = i;
            }
        }

        // Обновление движения (опционально, если нужно для других целей)
        _movementAmount = _handleJoystick.anchoredPosition / maxMovement;

        // Установка handle в центр ближайшего квадрата
        return closestSquareIndex;
    }

    private void Start()
    {
        SetHandlePosition(_squaresForJoystick[_currentSquareIndexJoystick].anchoredPosition);
    }

    private void LateUpdate()
    {
        if(_isHolding)
        {
            if (_textJumps.CountOfJumps > 0 && _stack.CanDoubleJump && !_stack.IsGroundedPublic)
            {
                _stack.IsFalling = false;
                _stack.DoubleJump();
            }
        }
    }

    private void SetHandlePosition(Vector2 position)
    {
        _handleJoystick.anchoredPosition = position;
    }

    public int Horizontal()
    {
        if (_currentSquareIndexJoystick == 0 || _currentSquareIndexJump == 0) return -1; // Левые квадраты (1)
        if (_currentSquareIndexJoystick == 2 || _currentSquareIndexJump == 2) return 1;  // Правые квадраты (3)
        return 0; // Центр 
    }

    public int Vertical()
    {
        if (_currentSquareIndexJump == 0 || _currentSquareIndexJump == 2) return 1;
        return 0;
    }
}