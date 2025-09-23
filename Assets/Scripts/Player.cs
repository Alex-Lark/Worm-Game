using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public CharacterController controller;

    public Transform wormHead;
    public List<Transform> wormParts;

    private int wormSegmentCount = 10;
    private float moveSpeed = 5f;
    private float rotationSpeed = 10f;
    private float maxPartDistance = 0.5f;

    private float maxAngle = GameParameters.MaxWormTurnAngle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        wormParts.Clear();
        CreateWormSegments();
        ConstructWorm();
    }

    private void CreateWormSegments()
    {
        for (int i = 0; i < wormSegmentCount; i++)
        {
            GameObject newWormSegment = Instantiate(wormSegmentPrefab);
            wormParts.Add(newWormSegment.transform);
        }
    }

    private void ConstructWorm()
    {
        // Start from the head
        Vector3 currentPos = wormHead.position;
        Vector3 backDir = -wormHead.forward; // opposite of head's facing direction

        for (int i = 0; i < wormParts.Count; i++)
        {
            // Position each part maxPartDistance behind the previous one
            currentPos += backDir * maxPartDistance;

            Transform part = wormParts[i];
            part.position = currentPos;

            // Optional: align rotation with head
            part.rotation = wormHead.rotation;
        }
    }

    public void MoveForward()
    {
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        // Calculate the desired rotation
        Quaternion desiredRotation = Quaternion.LookRotation(camForward);

        // Apply angle constraint to prevent sharp turns
        Quaternion constrainedRotation = ApplyTurnConstraint(wormHead.rotation, desiredRotation);

        // Smoothly rotate toward the constrained target
        wormHead.rotation = Quaternion.Slerp(wormHead.rotation, constrainedRotation, rotationSpeed * Time.deltaTime);

        // Move in the direction the worm head is actually facing (not camera direction)
        Vector3 wormForward = wormHead.forward;
        controller.Move(wormForward * moveSpeed * Time.deltaTime);

        MoveWormBody();
    }

    private Quaternion ApplyTurnConstraint(Quaternion currentRotation, Quaternion desiredRotation)
    {
        // Calculate the angle between current and desired rotation
        float angle = Quaternion.Angle(currentRotation, desiredRotation);

        // If the angle is within our constraint, return the desired rotation
        if (angle <= maxAngle)
        {
            return desiredRotation;
        }

        // Otherwise, limit the rotation to maxAngle degrees from current rotation
        float t = maxAngle / angle; // This gives us the fraction of rotation we can apply
        return Quaternion.Slerp(currentRotation, desiredRotation, t);
    }

    private void MoveWormBody()
    {
        Vector3 previousPosition = wormHead.transform.position;
        float maxMovePerFrame = moveSpeed * Time.deltaTime;

        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform part = wormParts[i];
            Vector3 toPrev = previousPosition - part.position;
            float distance = toPrev.magnitude;

            if (distance > maxPartDistance)
            {
                float moveDistance = distance - maxPartDistance;
                moveDistance = Mathf.Min(moveDistance, maxMovePerFrame);

                part.position += toPrev.normalized * moveDistance;

                // Apply the same turn constraint to body parts
                if (toPrev.sqrMagnitude > 0.001f)
                {
                    Quaternion desiredBodyRotation = Quaternion.LookRotation(toPrev);
                    Quaternion constrainedBodyRotation = ApplyTurnConstraint(part.rotation, desiredBodyRotation);

                    part.rotation = Quaternion.Slerp(part.rotation,
                        constrainedBodyRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            previousPosition = part.position;
        }
    }
}

