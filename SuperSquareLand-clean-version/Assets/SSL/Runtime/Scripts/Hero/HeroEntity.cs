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
#endregion
#region Setup Fall/Ground
    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;
    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround {get; private set;} = false;
#endregion
#region Setup Wall
    [Header("Wall")]
    [SerializeField] private WallDetector _wallDetector;
    public bool IsTouchingWall  {get; private set;} = false;
#endregion
#region Setup Dash
    [Header("Dash")]
    [SerializeField] private HeroDashSettings _DashSettings;
    [SerializeField] private HeroDashSettings _DashSettingsAir;
    private enum DashState
    {
        NotDashing,
        Dashing,
        EndDash
    }
    private DashState _dashState = DashState.NotDashing;
    private float _dashTimer = 0f;
#endregion
#region Setup Jump
    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;
    [SerializeField] private HeroHorizontalMovementsSettings _jumpHorizontalMovementsSettings;

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
    public float OrientX => _orientX;
#endregion
#region Setup Follow camera
    //Camera Follow
    private CameraFollowable _cameraFollowable;
#endregion
#region Setup Debug
    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;
#endregion
#endregion

#region Function
#region Function Awake
    private void Awake()
    {
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;
    }
#endregion
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
        if (IsJumping)
        {
            return _jumpHorizontalMovementsSettings;
        }
        return IsTouchingGround ? _groundHorizontalMovementsSettings : _airHorizontalmovementsSettings;
    }
#endregion
#region Functions Dash 
    public void DashStart()
    {
        _dashState = DashState.Dashing;
        _dashTimer = 0f;
    }

    private void _StopDash()
    {
        _horizontalSpeed = 0f;
    }

    private void _DashDeplacement()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
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
        _UpdateCameraFollowPosition();

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
            if(!IsTouchingGround && (_dashState != DashState.Dashing))
            {
                _ApplyFallGravity(_fallSettings);
            }else{
                _ResetVerticalSpeed();
            }
        }

        if (_dashState == DashState.Dashing || _dashState == DashState.EndDash)
        {
            _UpdateDash();
        }else{
            _ApplyHorizontalSpeed();
        }
        _ApplyVerticalSpeed();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }
    
    #region UpdateDash
    private void _UpdateDash()
    {
        switch(_dashState)
        {
            case DashState.Dashing:
                _dashTimer += Time.fixedDeltaTime;
                if (IsTouchingWall)
                {
                    _StopDash();
                    _dashState = DashState.EndDash;
                    break;
                }
                if (IsTouchingGround)
                {
                    if (_dashTimer < _DashSettings.Duration)
                    {
                        _horizontalSpeed = _DashSettings.Speed;
                        _DashDeplacement();
                    }else{
                        _horizontalSpeed = _groundHorizontalMovementsSettings.speedMax ;
                        _dashState = DashState.EndDash;
                    }
                    break;
                }else{
                    if (_dashTimer < _DashSettingsAir.Duration)
                    {
                        _horizontalSpeed = _DashSettingsAir.Speed;
                        _DashDeplacement();
                    }else{
                        _horizontalSpeed = _groundHorizontalMovementsSettings.speedMax ;
                        _dashState = DashState.EndDash;
                    }
                    break;
                }

            case DashState.EndDash:
                if (IsTouchingWall)
                {
                    _StopDash();
                    _dashState = DashState.EndDash;
                    break;
                }
                if (_orientX<0f)
                {
                    if (IsTouchingGround)
                    {
                        _horizontalSpeed -= _DashSettings.deceleration * Time.fixedDeltaTime;
                    }else{
                        _horizontalSpeed -= _DashSettingsAir.deceleration * Time.fixedDeltaTime;
                    }
                    
                }else{
                    if (IsTouchingGround)
                    {
                        _horizontalSpeed -= _DashSettings.deceleration * Time.fixedDeltaTime * _orientX;
                    }else{
                        _horizontalSpeed -= _DashSettingsAir.deceleration * Time.fixedDeltaTime * _orientX;
                    }
                }
                if (_horizontalSpeed < 0f)
                {
                    _horizontalSpeed = 0f;
                    _dashState = DashState.NotDashing;
                }

                _DashDeplacement();
                break;
        }
    }

    private void _UpdateCameraFollowPosition()
    {
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        if (IsTouchingGround && !IsJumping)
        {
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }
    #endregion
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
        if (!IsTouchingGround && _dashState != DashState.Dashing){
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
#region Fuctions wall
    private void _ApplyWallDetection()
    {
        IsTouchingWall = _wallDetector.DetectWallNearBy();
    }
    private void _ResetHorizontalSpeed()
    {
        _horizontalSpeed = 0f;
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
        GUILayout.Label(text:$"Dash State = {_dashState}");
        GUILayout.EndVertical();
    }
#endregion
#endregion
}