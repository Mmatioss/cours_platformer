using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
#region Setup
#region Setup Physics
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;
#endregion
#region Setup Movements
    [Header("Horizontal Movements")]
    [FormerlySerializedAs("movementsSettings")]
    [SerializeField] private HeroHorizontalMovementsSettings _groundHorizontalMovementsSettings;
    [SerializeField] private HeroHorizontalMovementsSettings _airHorizontalmovementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;
    [Header("Vertical Movements")]
    private float _verticalSpeed =0f;
    [SerializeField] private HeroDashSettings _DashSettings;
#endregion
#region Setup Fall/Ground
    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;
    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround {get; private set;} = false;
#endregion
#region Setup Jump
    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;
#endregion
#region Setup Orientation
    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;
#endregion
#region Setup Debug
    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;
#endregion
#endregion

#region Function
#region Functions Jump
    public void JumpStart()
    {
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;
    public bool IsJumping => _jumpState != JumpState.NotJumping;
#endregion
#region Functions move Dir
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }
#endregion
#region Functions move smooth
    private void _Accelerate(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax)
            _horizontalSpeed = settings.speedMax;
    }

    private void _Decelerate(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f){
            _horizontalSpeed = 0f;
            }
    }

    private void _TurnBack(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFriction * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f){
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }
#endregion
#region Functions Horizontal Speed
    private bool _AreOrientAndMovementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementsSettings settings)
    {
        if (_moveDirX != 0f)
        {
            _Accelerate(settings);
        }else{
            _Decelerate(settings);
        }
    }

    private void _ChangeOrientFromHorizontalMovement()
    {
        if (_moveDirX == 0f) return;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }

    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementsSettings()
    {
        return IsTouchingGround ? _groundHorizontalMovementsSettings : _airHorizontalmovementsSettings;
    }
#endregion
#region Functions Dash 
    public void Dash()
    {
        _horizontalSpeed = _DashSettings.Speed;
    }

    private void _StopDash()
    {
        _horizontalSpeed = 0f;
    }
#endregion
#region Functions Update
    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();

        HeroHorizontalMovementsSettings horizontalMovementsSettings = _GetCurrentHorizontalMovementsSettings();
        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack(horizontalMovementsSettings);
        }else{
            _UpdateHorizontalSpeed(horizontalMovementsSettings);
            _ChangeOrientFromHorizontalMovement();
        }
        if (IsJumping)
        {
            _UpdateJump();
        }else{
            if(!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            }else{
                _ResetVerticalSpeed();
            }
        }

        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }
    #region UpdateJump
    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration) {
            _verticalSpeed = _jumpSettings.jumpSpeed;
        }else{
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if (!IsTouchingGround){
            _ApplyFallGravity(_jumpFallSettings);
        }else{
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    private void _UpdateJump()
    {
        switch (_jumpState){
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;
            
            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }
    #endregion
#endregion
#region Functions fall
    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax)
        {
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }

    private void _ApplyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }
#endregion
#region Functions ground
    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }
    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }
#endregion
#region Functions debugGUI
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label(text:$"MoveDirX = {_moveDirX}");
        GUILayout.Label(text:$"OrientX = {_orientX}");
        if (IsTouchingGround){
            GUILayout.Label(text:"OnGround");
        }else{
            GUILayout.Label(text:"InAir");
        }
        GUILayout.Label(text:$"JumpingState = {_jumpState}");
        GUILayout.Label(text:$"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label(text:$"Vertical Speed = {_verticalSpeed}");
        GUILayout.EndVertical();
    }
#endregion
#endregion
}