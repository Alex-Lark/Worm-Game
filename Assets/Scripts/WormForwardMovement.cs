using UnityEngine;

using UnityEngine;

public class WormForwardMovement : MonoBehaviour
{
    private GameObject _camera;
    private Transform _head;
    private Rigidbody _headRb;

    private readonly RaycastHit[] _stepHits = new RaycastHit[10];

    void Start()
    {
        var player = Player.Instance;
        _camera = player.thirdPersonCamera;
        _head = player.wormHead;
        _headRb = _head.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Player.Instance.IsWormMoving)
        {
            MoveHead();
        }
    }

    private void MoveHead()
    {
        float speedFactor = 1f + _headRb.linearVelocity.magnitude / GameParameters.WormMoveForce;
        float rotationSpeed = GameParameters.WormHeadRotationSpeed * speedFactor;

        var part = _head.GetComponent<WormPart>();
        if (part.IsGrounded)
        {
            RotateHeadGrounded(rotationSpeed);
            MoveHeadGrounded(part);
        }
        else if (Player.Instance.IsWormGrounded)
        {
            RotateHeadAirborne(rotationSpeed);
            MoveHeadAirborne();
        }
    }

    private void RotateHeadGrounded(float speed)
    {
        Vector3 targetDir = Flatten(_camera.transform.forward);
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        _head.rotation = Quaternion.Slerp(_head.rotation, targetRot, speed * Time.fixedDeltaTime);
    }

    private void RotateHeadAirborne(float speed)
    {
        Vector3 camDir = _camera.transform.forward.normalized;
        Vector3 camDirFlat = Flatten(camDir);
        Vector3 wormDirFlat = Flatten(_head.forward);

        Quaternion yawRot = Quaternion.Slerp(
            Quaternion.LookRotation(wormDirFlat),
            Quaternion.LookRotation(camDirFlat),
            speed * Time.fixedDeltaTime
        );

        float pitch = CalculatePitch(camDir);
        ApplyYawPitch(yawRot, pitch);
    }

    private float CalculatePitch(Vector3 camDir)
    {
        float camPitch = Vector3.SignedAngle(
            Vector3.ProjectOnPlane(camDir, Vector3.up),
            camDir,
            _camera.transform.right
        );

        float normalized = Mathf.InverseLerp(GameParameters.minCameraPitch, GameParameters.maxCameraPitch, camPitch);
        normalized = 1f - Mathf.Clamp01(normalized);

        return Mathf.Lerp(-85f, 85f, normalized);
    }

    private void ApplyYawPitch(Quaternion yawRot, float targetPitch)
    {
        float currentPitch = Mathf.Asin(Mathf.Clamp(_head.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        float speedFactor = 1f + _headRb.linearVelocity.magnitude / GameParameters.WormMoveForce;
        float rotSpeed = GameParameters.WormHeadVerticalRotationSpeed * speedFactor;
        float newPitch = Mathf.LerpAngle(currentPitch, targetPitch, rotSpeed * Time.fixedDeltaTime);

        Quaternion pitchRot = Quaternion.AngleAxis(-newPitch, yawRot * Vector3.right);
        _head.rotation = pitchRot * yawRot;
    }

    private void MoveHeadGrounded(WormPart part)
    {
        if (_headRb.linearVelocity.magnitude > GameParameters.WormMaxVelocity)
            return;

        var groundRb = part.GroundObject?.GetComponent<Rigidbody>();
        TryClimbStep(part);

        Vector3 moveDir = AlignToSlope(_head.forward, part.GroundNormal);

        if (groundRb)
            groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, _head.position);
        else
            _headRb.AddForce(GameParameters.WormMoveForce * moveDir);
    }

    private void MoveHeadAirborne()
    {
        _headRb.AddForce(GameParameters.WormMoveForce * _head.forward);
    }

    private void TryClimbStep(WormPart part)
    {
        if (DetectStep(_head.position, _head.forward, part.GetComponent<Collider>(), out float stepHeight))
        {
            float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
            _headRb.AddForce(Vector3.up * climbForce);
        }
    }

    private bool DetectStep(Vector3 position, Vector3 forward, Collider col, out float height)
    {
        height = 0f;
        Vector3 dir = Flatten(forward);
        if (dir.magnitude < 0.1f) return false;

        Vector3 origin = position + dir * (col.bounds.extents.x * 1.2f) - Vector3.up * (col.bounds.extents.y * 0.5f);
        int hits = Physics.RaycastNonAlloc(origin, dir, _stepHits, GameParameters.StepDetectionDistance);

        for (int i = 0; i < hits; i++)
        {
            var hit = _stepHits[i];
            if (hit.collider.transform.root == transform.root) continue;

            Vector3 topOrigin = origin + dir * GameParameters.StepDetectionDistance + Vector3.up * GameParameters.MaxStepHeight;
            if (!Physics.Raycast(topOrigin, Vector3.down, out RaycastHit topHit, GameParameters.MaxStepHeight * 1.5f)) continue;
            if (topHit.collider.transform.root == transform.root) continue;

            height = topHit.point.y - (position.y - col.bounds.extents.y);
            if (height > 0.05f && height <= GameParameters.MaxStepHeight) return true;
        }

        return false;
    }

    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    private Vector3 AlignToSlope(Vector3 forward, Vector3 normal)
    {
        Vector3 slopeDir = Vector3.ProjectOnPlane(forward, normal).normalized;
        float slopeAngle = Vector3.Angle(Vector3.up, normal);

        if (slopeAngle > GameParameters.MaxSlopeAngle)
        {
            float blend = Mathf.InverseLerp(GameParameters.MaxSlopeAngle, 90f, slopeAngle);
            slopeDir = Vector3.Lerp(slopeDir, forward, blend).normalized;
        }

        return slopeDir;
    }
}
