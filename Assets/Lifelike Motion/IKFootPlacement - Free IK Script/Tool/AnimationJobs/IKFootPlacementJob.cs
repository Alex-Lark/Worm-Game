namespace LifelikeMotion.IKFootPlacement
{
    using UnityEngine;
    using UnityEngine.Animations;

    public struct IKFootPlacementJob : IAnimationJob
    {
        #region Struct Variables
        // IK Targets
        public TransformStreamHandle[] Targets;

        public float TargetPositionOffsetWeight;
        public float TargetRotationOffsetWeight;

        private Vector3[] targetPositions;
        private float[] targetPositionOffsets;
        private Quaternion[] targetRotations;
        private Quaternion[] targetRotationOffsets;

        // IK Hints
        public TransformStreamHandle[] Hints;
        private Vector3[] hintPositions;

        // Body
        public TransformStreamHandle Hips;
        public Vector3 RootPosition;

        public float BodyPositionOffsetWeight;
        public float BodyRotationOffsetWeight;
        public bool InvertBodyPositionOffset;
        public bool InvertBodyRotationOffset;

        private float bodyPositionOffset;
        private Quaternion bodyRotations;
        private Quaternion bodyRotationOffset;

        // Parameters
        public float FeetPositionOffsetSmoothing;
        public float FeetRotationOffsetSmoothing;

        public float BodyPositionOffsetSmoothing;
        public float BodyRotationOffsetSmoothing;

        public float StationaryToRotateSmoothing;
        public float StationaryToWalkSmoothing;

        public float MaxStationaryRotationAngle;
        public float DeltaTime;

        // Raycast
        public Vector3 BodyRaycastHitPoint;
        public Vector3 BodyRaycastHitNormal;
        public Vector3 BodyRaycastOrigin;

        public Vector3[] LegRaycastHitPoint;
        public Vector3[] LegRaycastHitNormal;
        public Vector3[] LegRaycastOrigin;

        private Vector3 lowestLegRaycastHitPoint;

        // Control bools
        public bool IsActive;
        public bool IsGrounded; // Important! "IKFootPlacement.isGrounded" variable should be controlled by another script for character movement!
        public bool IsMoving; // Important! "IKFootPlacement.isMoving" variable should be controlled by another script for character movement!
        public bool Jumped; // Important! "IKFootPlacement.jumped" variable should be controlled by another script for character movement!
        public bool AdjustFeet;
        public bool[] AdjustedFoot;
        private bool startup;

        // Other
        public string AdjustDirection;
        public float LerpSpeed;
        #endregion

        public void ProcessRootMotion(AnimationStream stream) { } // Leave empty

        public void ProcessAnimation(AnimationStream stream)
        {
            if (startup) { IsMoving = true; }

            lowestLegRaycastHitPoint = new Vector3(0, RootPosition.y, 0);

            CalculateLerpSpeed();

            if (IsActive)
            {
                OffsetTarget(stream);
                CheckFeetAdjustment();
                if (BodyRotationOffsetWeight > 0) { OffsetBodyRotation(stream); }

                startup = false;
            }
            else { startup = true; }

            if (BodyPositionOffsetWeight > 0) { OffsetBodyPosition(stream); }
        }


        private void OffsetTarget(AnimationStream stream)
        {
            if (AdjustDirection == "left")
            {
                for (int i = Targets.Length - 1; i >= 0; i--)
                {
                    CalculateTargetOffsetPosition(stream, i);
                    CalculateTargetOffsetRotation(stream, i);
                    CalculateHintOffsetPosition(stream, i);
                }
            }
            else
            {
                for (int i = 0; i < Targets.Length; i++)
                {
                    CalculateTargetOffsetPosition(stream, i);
                    CalculateTargetOffsetRotation(stream, i);
                    CalculateHintOffsetPosition(stream, i);
                }
            }
        }

        private void CalculateTargetOffsetPosition(AnimationStream stream, int i)
        {
            #region Get position from animation
            if (startup) { targetPositions[i] = Targets[i].GetPosition(stream); }
            if (IsMoving || !IsGrounded)
            {
                if (LerpSpeed != 0)
                {
                    targetPositions[i].y = Targets[i].GetPosition(stream).y;
                    targetPositions[i] = Vector3.Lerp(targetPositions[i],
                                                      Targets[i].GetPosition(stream),
                                                      DeltaTime / (StationaryToWalkSmoothing * LerpSpeed));
                }
                else { targetPositions[i] = Targets[i].GetPosition(stream); }
            }
            else if (!IsMoving && IsGrounded)
            {
                if (AdjustedFoot[i])
                {
                    targetPositions[i].y = Targets[i].GetPosition(stream).y;
                    float distanceToTaget = Vector3.Distance(targetPositions[i], Targets[i].GetPosition(stream));
                    if (distanceToTaget > 0.025f)
                    {
                        targetPositions[i] = Vector3.Lerp(targetPositions[i],
                                                          Targets[i].GetPosition(stream),
                                                          DeltaTime / StationaryToRotateSmoothing);
                        AdjustFeet = false;
                    }
                }
                else { targetPositions[i].y = Targets[i].GetPosition(stream).y; }
            }
            else { targetPositions[i] = Targets[i].GetPosition(stream); }
            #endregion

            #region Calculate and apply position offset
            float footPositionOffset = 0;
            if (TargetPositionOffsetWeight <= 0)
            {
                targetPositionOffsets[i] = 0;

                // Check if RaycastHit point and origin are not Vector3.zero
                if (LegRaycastHitPoint[i].y != 0 || LegRaycastOrigin[i].y != 0)
                {
                    if (LegRaycastHitPoint[i].y < lowestLegRaycastHitPoint.y) { lowestLegRaycastHitPoint = LegRaycastHitPoint[i]; }
                }
            }
            else
            {
                if (IsGrounded)
                {
                    // Check if RaycastHit point and origin are not Vector3.zero
                    if (LegRaycastHitPoint[i].y != 0 || LegRaycastOrigin[i].y != 0)
                    {
                        if (LegRaycastHitPoint[i].y < lowestLegRaycastHitPoint.y) { lowestLegRaycastHitPoint = LegRaycastHitPoint[i]; }

                        LegRaycastOrigin[i] = new Vector3(targetPositions[i].x, LegRaycastOrigin[i].y, targetPositions[i].z);
                        LegRaycastHitPoint[i] = new Vector3(targetPositions[i].x, LegRaycastHitPoint[i].y, targetPositions[i].z);

                        float distanceToIKFoot = Vector3.Distance(LegRaycastOrigin[i], targetPositions[i]) + (targetPositions[i].y - RootPosition.y);
                        float distanceToRaycastHit = Vector3.Distance(LegRaycastOrigin[i], LegRaycastHitPoint[i]);

                        if (distanceToRaycastHit < distanceToIKFoot - 0.001f ||
                           distanceToRaycastHit > distanceToIKFoot + 0.001f)
                        {
                            footPositionOffset = distanceToIKFoot - distanceToRaycastHit;
                        }
                    }
                }

                if (TargetPositionOffsetWeight != 1) { footPositionOffset = Mathf.Lerp(0, footPositionOffset, TargetPositionOffsetWeight); }
                if (FeetPositionOffsetSmoothing > 0)
                {
                    targetPositionOffsets[i] = Mathf.Lerp(targetPositionOffsets[i],
                                                          footPositionOffset,
                                                          DeltaTime / FeetPositionOffsetSmoothing);
                }
                else { targetPositionOffsets[i] = footPositionOffset; }
            }

            targetPositions[i].y += targetPositionOffsets[i];
            Targets[i].SetPosition(stream, targetPositions[i]);
            #endregion
        }

        private void CalculateTargetOffsetRotation(AnimationStream stream, int i)
        {
            #region Get rotation from animation
            if (startup) { targetRotationOffsets[i] = Targets[i].GetRotation(stream); }
            if (IsMoving || !IsGrounded)
            {
                if (LerpSpeed != 0)
                {
                    targetRotations[i] = Quaternion.Lerp(targetRotations[i],
                                                         Targets[i].GetRotation(stream),
                                                         DeltaTime / (StationaryToWalkSmoothing * LerpSpeed));
                }
                else { targetRotations[i] = Targets[i].GetRotation(stream); }
            }
            else if (!IsMoving && IsGrounded)
            {
                if (AdjustedFoot[i])
                {
                    if (!AdjustFeet)
                    {
                        targetRotations[i] = Quaternion.Lerp(targetRotations[i],
                                                             Targets[i].GetRotation(stream),
                                                             DeltaTime / StationaryToRotateSmoothing);
                    }
                }
            }
            else { targetRotationOffsets[i] = Targets[i].GetRotation(stream); }
            #endregion

            #region Calculate and apply rotation offset
            Quaternion targetRotationOffset;

            if (TargetRotationOffsetWeight <= 0) { targetRotationOffsets[i] = Quaternion.identity; }
            else
            {
                targetRotationOffset = Quaternion.FromToRotation(Vector3.up, LegRaycastHitNormal[i]);
                if (TargetRotationOffsetWeight != 1)
                {
                    targetRotationOffset = Quaternion.Slerp(Quaternion.identity,
                                                            targetRotationOffset,
                                                            TargetRotationOffsetWeight);
                }
                if (FeetRotationOffsetSmoothing > 0 && !startup)
                {
                    targetRotationOffsets[i] = Quaternion.Lerp(targetRotationOffsets[i],
                                                               targetRotationOffset,
                                                               DeltaTime / FeetRotationOffsetSmoothing);
                }
                else { targetRotationOffsets[i] = targetRotationOffset; }
            }

            targetRotationOffset = targetRotationOffsets[i] * targetRotations[i];
            Targets[i].SetRotation(stream, targetRotationOffset);
            #endregion
        }

        private void CalculateHintOffsetPosition(AnimationStream stream, int i)
        {
            #region Get position from animation
            if (startup) { hintPositions[i] = Hints[i].GetPosition(stream); }
            if (IsMoving || !IsGrounded)
            {
                if (LerpSpeed != 0)
                {
                    hintPositions[i] = Vector3.Lerp(hintPositions[i],
                                                    Hints[i].GetPosition(stream),
                                                    DeltaTime / (StationaryToWalkSmoothing * LerpSpeed));
                    hintPositions[i].y = Hints[i].GetPosition(stream).y;
                }
                else { hintPositions[i] = Hints[i].GetPosition(stream); }
            }
            else if (!IsMoving && IsGrounded)
            {
                if (AdjustedFoot[i])
                {
                    if (!AdjustFeet)
                    {
                        hintPositions[i] = Vector3.Lerp(hintPositions[i],
                                                        Hints[i].GetPosition(stream),
                                                        DeltaTime / StationaryToRotateSmoothing);
                    }
                }
                hintPositions[i].y = Hints[i].GetPosition(stream).y;
            }
            else { hintPositions[i] = Hints[i].GetPosition(stream); }
            #endregion

            #region Apply position offset
            Hints[i].SetPosition(stream, hintPositions[i]);
            #endregion
        }

        private void OffsetBodyPosition(AnimationStream stream)
        {
            #region Get position from animation
            float bodyPositionOffestTarget = 0;
            if (IsGrounded)
            {
                if (!InvertBodyPositionOffset) { bodyPositionOffestTarget = (RootPosition.y - lowestLegRaycastHitPoint.y); }
                else { bodyPositionOffestTarget = (RootPosition.y - lowestLegRaycastHitPoint.y) * -1; }
            }
            if (IsActive)
            {
                if (BodyPositionOffsetSmoothing > 0)
                {
                    bodyPositionOffset = Mathf.Lerp(bodyPositionOffset,
                                                    bodyPositionOffestTarget,
                                                    DeltaTime / BodyPositionOffsetSmoothing);
                }
                else { bodyPositionOffset = bodyPositionOffestTarget; }
            }
            else
            {
                if (BodyPositionOffsetSmoothing > 0 && bodyPositionOffset != 0)
                {
                    bodyPositionOffset = Mathf.Lerp(bodyPositionOffset,
                                                    0,
                                                    DeltaTime / BodyPositionOffsetSmoothing);
                }
                else { bodyPositionOffset = 0; }
            }
            #endregion

            #region Calculate and apply position offset
            Vector3 currentBodyPosition = Hips.GetPosition(stream);
            currentBodyPosition.y -= bodyPositionOffset * BodyPositionOffsetWeight;
            Hips.SetPosition(stream, currentBodyPosition);
            #endregion
        }

        private void OffsetBodyRotation(AnimationStream stream)
        {
            #region Get rotation from animation
            bodyRotations = Hips.GetRotation(stream);
            if (startup) { bodyRotationOffset = Quaternion.identity; }
            #endregion

            #region Calculate and apply rotation offset
            Quaternion targetRotationOffset = Quaternion.FromToRotation(Vector3.up, BodyRaycastHitNormal);
            if (InvertBodyRotationOffset) { targetRotationOffset = Quaternion.Inverse(targetRotationOffset); }

            if (FeetRotationOffsetSmoothing > 0)
            {
                bodyRotationOffset = Quaternion.Lerp(bodyRotationOffset,
                                                      targetRotationOffset,
                                                      DeltaTime / BodyRotationOffsetSmoothing);
            }
            else { bodyRotationOffset = targetRotationOffset; }

            targetRotationOffset = (bodyRotationOffset * bodyRotations);

            if (BodyRotationOffsetWeight != 1)
            {
                targetRotationOffset = Quaternion.Slerp(bodyRotations,
                                                        targetRotationOffset,
                                                        BodyRotationOffsetWeight);
            }

            Vector3 hipsEuler = Hips.GetLocalRotation(stream).eulerAngles;
            Hips.SetRotation(stream, targetRotationOffset);
            Vector3 currentHipsEuler = Hips.GetLocalRotation(stream).eulerAngles;
            currentHipsEuler.y = hipsEuler.y;
            currentHipsEuler.z = hipsEuler.z;
            Hips.SetLocalRotation(stream, Quaternion.Euler(currentHipsEuler));
            #endregion
        }

        // Feet adjustment logic when rotating but not moving
        private void CheckFeetAdjustment()
        {
            // When not moving but adjusting, check if any of the feet are still adjusting
            if (!AdjustFeet && !IsMoving)
            {
                for (int i = 0; i < Targets.Length; i++)
                {
                    if (AdjustedFoot[i]) { AdjustFeet = true; }
                }
            }
            // Check if adjustment should continue or stop
            else if (AdjustFeet && !IsMoving)
            {
                // Adjusting from last to first TwoBoneIKConstraint target
                if (AdjustDirection == "left")
                {
                    for (int i = Targets.Length - 1; i >= 0; i--)
                    {
                        if (AdjustedFoot[i])
                        {
                            AdjustedFoot[i] = false;
                            // Set next foot to adjust
                            if (i - 1 >= 0)
                            {
                                AdjustedFoot[i - 1] = true;
                                break;
                            }
                            // Stop adjustment
                            else { AdjustFeet = false; }
                        }
                    }
                }
                // Adjusting from first to last TwoBoneIKConstraint target
                else if (AdjustDirection == "right")
                {
                    for (int i = 0; i < Targets.Length; i++)
                    {
                        if (AdjustedFoot[i])
                        {
                            AdjustedFoot[i] = false;
                            // Set next foot to adjust
                            if (i + 1 < Targets.Length)
                            {
                                AdjustedFoot[i + 1] = true;
                                break;
                            }
                            // Stop adjustment
                            else { AdjustFeet = false; }
                        }
                    }
                }
                // Stop adjustment if all feet are done adjusting
                else
                {
                    for (int i = 0; i < Targets.Length; i++) { AdjustedFoot[i] = false; }
                    AdjustFeet = false;
                }
            }
        }

        // When blending from stationary to walking state, LerpSpeed is to help make sure that this blend will end and not lerp forever
        private void CalculateLerpSpeed()
        {
            if (IsMoving || !IsGrounded)
            {
                if (AdjustFeet)
                {
                    AdjustFeet = false;
                    LerpSpeed = 1;
                }
                if (StationaryToWalkSmoothing > 0 && LerpSpeed > 0.001f && IsGrounded && !Jumped)
                {
                    LerpSpeed = Mathf.Lerp(LerpSpeed, 0, DeltaTime / (StationaryToWalkSmoothing / 2f));
                }
                else if (StationaryToWalkSmoothing > 0 && LerpSpeed > 0.001 && !IsGrounded && Jumped)
                {
                    LerpSpeed = Mathf.Lerp(LerpSpeed, 0, DeltaTime / (StationaryToWalkSmoothing / 4f));
                }
                else LerpSpeed = 0;
            }
            else LerpSpeed = 1;
        }

        // Create arrays and set values to start with
        public bool Create(int length)
        {
            Hips = new TransformStreamHandle();
            Targets = new TransformStreamHandle[length];
            targetPositions = new Vector3[length];
            targetPositionOffsets = new float[length];
            targetRotations = new Quaternion[length];
            targetRotationOffsets = new Quaternion[length];

            LegRaycastOrigin = new Vector3[length];
            LegRaycastHitPoint = new Vector3[length];
            LegRaycastHitNormal = new Vector3[length];

            Hints = new TransformStreamHandle[length];
            hintPositions = new Vector3[length];

            bodyPositionOffset = 0;

            IsActive = true;
            IsGrounded = true;
            IsMoving = true;
            AdjustFeet = false;
            AdjustedFoot = new bool[length];

            startup = true;
            return true;
        }

        // Reset values to zero
        public void ResetValues()
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                LegRaycastOrigin[i] = Vector3.zero;
                LegRaycastHitPoint[i] = Vector3.zero;
                LegRaycastHitNormal[i] = Vector3.zero;
                BodyRaycastOrigin = Vector3.zero;
                BodyRaycastHitPoint = Vector3.zero;
                BodyRaycastHitNormal = Vector3.zero;
                RootPosition = Vector3.zero;
                bodyPositionOffset = 0;
                bodyRotationOffset = Quaternion.identity;
                LerpSpeed = 1;
            }
        }
    }

}