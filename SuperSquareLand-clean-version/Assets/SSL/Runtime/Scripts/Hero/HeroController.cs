using UnityEngine;

public class HeroController : MonoBehaviour
{
#region Setup
#region Setup Entity
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;
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
        _entity.SetMoveDirX(GetInputMoveX());

        if (_GetInputDownJump())
        {
            if (_entity.IsTouchingGround && !_entity.IsJumping)
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
            _entity.Dash();
        }
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
#region Functions DebugGUI
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.EndVertical();
    }
#endregion
#endregion
}