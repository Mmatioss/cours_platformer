using UnityEngine;

public class HeroEntity : MonoBehaviour
{
#region Setup Physics
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;
#endregion
#region Setup Movements
    [Header("Horizontal Movements")]
    [SerializeField] private HeroHorizontalMovementsSettings _movementsSettings;
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
#region Setup Orientation
    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;
#endregion
#region Setup Debug
    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;
#endregion

    private void FixedUpdate()
    {
        _ApplyGroundDetection();

        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack();
        }else{
            _UpdateHorizontalSpeed();
            _ChangeOrientFromHorizontalMovement();
        }
        if(!IsTouchingGround)
        {
            _ApplyFallGravity();
        }else{
            _ResetVerticalSpeed();
        }
        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
    }

#region Functions move Dir
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }
#endregion
#region Functions move smooth
    private void _Accelerate()
    {
        _horizontalSpeed += _movementsSettings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > _movementsSettings.speedMax)
            _horizontalSpeed = _movementsSettings.speedMax;
    }

    private void _Decelerate()
    {
        _horizontalSpeed -= _movementsSettings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f){
            _horizontalSpeed = 0f;
            }
    }

    private void _TurnBack()
    {
        _horizontalSpeed -= _movementsSettings.turnBackFriction * Time.fixedDeltaTime;
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

    private void _UpdateHorizontalSpeed()
    {
        if (_moveDirX != 0f)
        {
            _Accelerate();
        }else{
            _Decelerate();
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
#endregion
#region Functions Dash 
    public void Dash()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _DashSettings.Speed * _orientX;
        _rigidbody.velocity = velocity;
    }
#endregion
#region Functions Update
    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }
#endregion
#region Functions fall
    private void _ApplyFallGravity()
    {
        _verticalSpeed -= _fallSettings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -_fallSettings.fallSpeedMax)
        {
            _verticalSpeed = -_fallSettings.fallSpeedMax;
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
        GUILayout.Label(text:$"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label(text:$"Vertical Speed = {_verticalSpeed}");
        GUILayout.EndVertical();
    }
#endregion
}