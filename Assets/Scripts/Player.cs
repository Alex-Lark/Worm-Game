using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public Transform wormHead;
    public List<Transform> wormParts;

    private int wormSegmentCount = GameParameters.WormSegmentCount;
    private float moveSpeed = GameParameters.WormMoveSpeed;
    private float rotationSpeed = GameParameters.WormRotationSpeed;
    private float maxPartDistance = GameParameters.SegmentMaxPartDistance;
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
        GetComponent<WormPhysics>().AddCollidersToSegments();
        GetComponent<WormPhysics>().SetupWormCollisions();
    }

    private void FixedUpdate()
    {
        MoveWormBody();
        
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
    
        // Set rotation directly (freezeRotation allows this)
        if (camForward.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            wormHead.rotation = targetRotation;
        }
    }

    public void MoveForward()
    {
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        
        //Quaternion desiredRotation = Quaternion.LookRotation(camForward);
        
        //Quaternion constrainedRotation = ApplyTurnConstraint(wormHead.rotation, desiredRotation);
        
        //wormHead.rotation = Quaternion.Slerp(wormHead.rotation, constrainedRotation, rotationSpeed * Time.deltaTime);
        
        //Vector3 wormForward = wormHead.forward;
        
        //controller.Move(wormForward * moveSpeed * Time.deltaTime);

        Rigidbody rigidbody = wormHead.GetComponent<Rigidbody>();
        rigidbody.AddForce(moveSpeed * camForward);
        //ArticulationBody articulationBody = wormHead.GetComponent<ArticulationBody>();
        //articulationBody.AddForce(moveSpeed * camForward);

        // Keep yaw rotation, but kill pitch/roll drift
        //Vector3 angVel = articulationBody.angularVelocity;
        //articulationBody.angularVelocity = new Vector3(0f, angVel.y, 0f);

    }
    
    private void CreateWormSegments()
    {
        for (int i = 0; i < wormSegmentCount; i++)
        {
            GameObject newWormSegment = Instantiate(wormSegmentPrefab, transform);
            wormParts.Add(newWormSegment.transform);
        }
    }

    private void ConstructWorm()
    {
        
        Vector3 currentPos = wormHead.position;
        Vector3 backDir = -wormHead.forward; 

        for (int i = 0; i < wormParts.Count; i++)
        {
            currentPos += backDir * maxPartDistance;

            Transform part = wormParts[i];
            part.position = currentPos;
            
            part.rotation = wormHead.rotation;
        }
    }

    private Quaternion ApplyTurnConstraint(Quaternion currentRotation, Quaternion desiredRotation)
    {
        float angle = Quaternion.Angle(currentRotation, desiredRotation);
        
        if (angle <= maxAngle)
        {
            return desiredRotation;
        }
        
        float t = maxAngle / angle; 
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

