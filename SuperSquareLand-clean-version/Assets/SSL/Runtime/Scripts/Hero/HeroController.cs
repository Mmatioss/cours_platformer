using System.Xml;
using UnityEngine;

public class HeroController : MonoBehaviour
{
#region Setup
#region Setup Entity
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;
    private bool _entityWasTouchingGround = false;
#endregion
#region Setup Jump Buffer
    [Header("Jump Buffer")]
    [SerializeField] private float _jumpBufferDuration = 0.2f;
    private float _jumpBufferTimer = 0f;
#endregion
#region Setup Coyote Time
    [Header("Coyote Time")]
    [SerializeField] private float _coyoteTimeDuration = 0.2f;
    private float _coyoteTimeCountdown = -1f;
#endregion
#region Setup Debug
    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;
#endregion
#endregion

#region Functions
#region FUnctions Update
    private void Update()
    {
        _UpdateJumpBuffer();

        _entity.SetMoveDirX(GetInputMoveX());

        if (_EntityHasExitGround())
        {
            _ResetCoyoteTime();
        }else{
            _UpdatecoyoteTime();
        }

        if (_GetInputDownJump())
        {
            if ((_entity.IsTouchingGround || _IscoyoteTimeActive()) && !_entity.IsJumping)
            {
                _entity.JumpStart();
            }else{
                _ResetJumpBuffer();
            }
        }

        if (IsJumpBufferActive())
        {
            if ((_entity.IsTouchingGround || _IscoyoteTimeActive()) && !_entity.IsJumping)
            {
                _entity.JumpStart();
            }
        }

        if (_entity.IsJumpImpulsing)
        {
            if (!_GetInputJump() && _entity.IsJumpMinDurationReached)
            {
                _entity.StopJumpImpulsion();
            }
        }

        if (_GetInputDash())
        {
            _entity.DashStart();
        }

        _entityWasTouchingGround = _entity.IsTouchingGround;
    }

    private void _UpdateJumpBuffer()
    {
        if (!IsJumpBufferActive()) return;
        _jumpBufferTimer += Time.deltaTime;
    }

    private void _UpdatecoyoteTime()
    {
        if (!_IscoyoteTimeActive()) return;
        _coyoteTimeCountdown -= Time.deltaTime;
    }
#endregion
#region Functions MoveX
    private float GetInputMoveX()
    {
        float inputMoveX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q)){
            // Negative means : To the left <=
            inputMoveX = -1f;
        }

        if (Input.GetKey(KeyCode.D)){
            // Positive means : To the right =>
            inputMoveX = 1f;
        }

        return inputMoveX;
    }
#endregion
#region Functions Dash
    private bool _GetInputDash()
    {
        return Input.GetKeyDown(KeyCode.E);
    }
#endregion
#region Functions Jump
    private bool _GetInputDownJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool _GetInputJump()
    {
        return Input.GetKey(KeyCode.Space);
    }
#endregion
#region Functions Coyote Time
    private bool _IscoyoteTimeActive()
    {
        return _coyoteTimeCountdown > 0f;
    }

    private void _ResetCoyoteTime()
    {
        _coyoteTimeCountdown = _coyoteTimeDuration;
    }

    private bool _EntityHasExitGround()
    {
        return !_entity.IsTouchingGround && _entityWasTouchingGround;
    
    }
#endregion
#region Functions Jumps Buffer
    private void _ResetJumpBuffer()
    {
        _jumpBufferTimer = 0f;
    }

    private bool IsJumpBufferActive()
    {
        return _jumpBufferTimer < _jumpBufferDuration;
    }

    private void _CancelJumpBuffer()
    {
        _jumpBufferTimer = _jumpBufferDuration;
    }
#endregion
#region Functions DebugGUI
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label(text:$"Jump Buffer Timer = {_jumpBufferTimer}");
        GUILayout.Label(text:$"Coyote Time Countdown = {_coyoteTimeCountdown}");
        GUILayout.EndVertical();
    }
#endregion
#endregion
}