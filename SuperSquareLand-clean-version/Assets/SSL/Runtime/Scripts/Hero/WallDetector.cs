using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform[] _detectionPointsLeft;
    [SerializeField] private Transform[] _detectionPointsRight;
    [SerializeField] private float _detectionLength = 0.1f;
    [SerializeField] private LayerMask _groundLayerMask;

    public bool DetectWallNearBy()
    {
        foreach (Transform detectionPoint in _detectionPointsRight)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.right,
                _detectionLength,
                _groundLayerMask
            );
            if (hitResult.collider != null)
            {
                return true;
            }
        }
        foreach (Transform detectionPoint in _detectionPointsLeft)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.left,
                _detectionLength,
                _groundLayerMask
            );
            if (hitResult.collider != null)
            {
                return true;
            }
        }
        
        return false;
    }
}