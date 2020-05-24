using Mirror;
using UnityEngine;

namespace HardCoreGameDevs.Networking
{
    public partial class VehicleSync : NetworkBehaviour
    {
        bool triedToExtrapolateTooFar = false;

        void InterpolateOrExtrapolate() {

            if (stateCount == 0)
            return;

            if (!extrapolatedLastFrame)
            tempState.ResetTheVariables();

            triedToExtrapolateTooFar = false;

            float interpolationTime = approximateNetworkTimeOnOwner - interpolationBackTime;

            if (stateCount > 1 && stateBuffer[0].ownerTimestamp > interpolationTime) {

                Interpolate(interpolationTime);
                extrapolatedLastFrame = false;

            }
            else if (stateBuffer[0].atPositionalRest && stateBuffer[0].atRotationalRest) {

                tempState.CopyFromState(stateBuffer[0]);
                extrapolatedLastFrame = false;

                if (setVelocityInsteadOfPositionOnNonOwners)
                triedToExtrapolateTooFar = true;

            }
            else if ((isAuthorityChanged && Time.realtimeSinceStartup - latestAuthorityChangeZeroTime > interpolationBackTime * 2.0f) || !isAuthorityChanged) {

                Extrapolate(interpolationTime);
                extrapolatedLastFrame = true;

                if (setVelocityInsteadOfPositionOnNonOwners) {

                    float timeSinceLatestReceive = interpolationTime - stateBuffer[0].ownerTimestamp;
                    tempState.velocity = stateBuffer[0].velocity;
                    tempState.position = stateBuffer[0].position + tempState.velocity * timeSinceLatestReceive;
                    Vector3 predictedPos = transform.position + tempState.velocity * Time.deltaTime;
                    float percent = (tempState.position - predictedPos).sqrMagnitude / (maxPositionDifferenceForVelocitySyncing * maxPositionDifferenceForVelocitySyncing);
                    tempState.velocity = Vector3.Lerp(tempState.velocity, (tempState.position - transform.position) / Time.deltaTime, percent);

                }

            }
            else {

                return;

            }

            float actualPositionLerpSpeed = positionLerpSpeed;
            float actualRotationLerpSpeed = rotationLerpSpeed;
            float actualScaleLerpSpeed = scaleLerpSpeed;

            if (dontLerp) {

                actualPositionLerpSpeed = 1;
                actualRotationLerpSpeed = 1;
                actualScaleLerpSpeed = 1;
                dontLerp = false;

            }

            if (!triedToExtrapolateTooFar) {

                bool changedPositionEnough = false;
                float sqrDistance = 0;

                if (CurrentPosition() != tempState.position) {

                    if (snapPositionThreshold != 0 || receivedPositionThreshold != 0)
                    sqrDistance = (tempState.position - CurrentPosition()).sqrMagnitude;

                }

                if (receivedPositionThreshold != 0) {

                    if (sqrDistance > receivedPositionThreshold * receivedPositionThreshold)
                    changedPositionEnough = true;

                }
                else {

                    changedPositionEnough = true;

                }

                bool changedRotationEnough = false;
                float angleDifference = 0;

                if (CurrentRotation() != tempState.rotation) {

                    if (snapRotationThreshold != 0 || receivedRotationThreshold != 0)
                    angleDifference = Quaternion.Angle(CurrentRotation(), tempState.rotation);

                }

                if (receivedRotationThreshold != 0) {

                    if (angleDifference > receivedRotationThreshold)
                    changedRotationEnough = true;

                }
                else {

                    changedRotationEnough = true;

                }

                if (syncPosition != SyncMode.dontSync) {

                    if (changedPositionEnough) {

                        bool shouldTeleport = false;

                        if (sqrDistance > snapPositionThreshold * snapPositionThreshold) {

                            actualPositionLerpSpeed = 1;
                            shouldTeleport = true;
                        }

                        Vector3 newPosition = CurrentPosition();

                        if (isSyncingPosition) {

                            newPosition = tempState.position;

                        }

                        if (setVelocityInsteadOfPositionOnNonOwners && !shouldTeleport) {

                            if (rb)
                            rb.velocity = tempState.velocity;

                        }
                        else {

                            Position(Vector3.Lerp(CurrentPosition(), newPosition, actualPositionLerpSpeed), shouldTeleport);

                        }
                    }
                }

                if (syncRotation != SyncMode.dontSync) {

                    if (changedRotationEnough) {

                        bool shouldTeleport = false;
                        if (angleDifference > snapRotationThreshold) {

                            actualRotationLerpSpeed = 1;
                            shouldTeleport = true;

                        }

                        Vector3 newRotation = CurrentRotation().eulerAngles;

                        if(isSyncingRotation) {

                            newRotation = tempState.rotation.eulerAngles;

                        }

                        Quaternion newQuaternion = Quaternion.Euler(newRotation);
                        Rotation(Quaternion.Lerp(CurrentRotation(), newQuaternion, actualRotationLerpSpeed), shouldTeleport);

                    }

                }

            }
            else if (triedToExtrapolateTooFar) {

                if (rb) {

                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                }

            }

        }

        void Interpolate(float interpolationTime) {

            int stateIndex = 0;

            for (; stateIndex < stateCount; stateIndex++)
            if (stateBuffer[stateIndex].ownerTimestamp <= interpolationTime)
            break;

            if (stateIndex == stateCount)
            stateIndex--;

            VehicleState end = stateBuffer[Mathf.Max(stateIndex - 1, 0)];
            VehicleState start = stateBuffer[stateIndex];

            float t = (interpolationTime - start.ownerTimestamp) / (end.ownerTimestamp - start.ownerTimestamp);

            ShouldTeleport(start, ref end, interpolationTime, ref t);

            tempState = VehicleState.Lerp(tempState, start, end, t);

            if (setVelocityInsteadOfPositionOnNonOwners) {

                Vector3 predictedPos = transform.position + tempState.velocity * Time.deltaTime;
                float percent = (tempState.position - predictedPos).sqrMagnitude / (maxPositionDifferenceForVelocitySyncing * maxPositionDifferenceForVelocitySyncing);
                tempState.velocity = Vector3.Lerp(tempState.velocity, (tempState.position - transform.position) / Time.deltaTime, percent);

            }

        }

        void ShouldTeleport(VehicleState start, ref VehicleState end, float interpolationTime, ref float t) {

            if (start.ownerTimestamp > interpolationTime && start.teleport && stateCount == 2) {

                end = start;
                t = 1;
                dontLerp = true;

            }

            for (int i = 0; i < stateCount; i++) {

                if (stateBuffer[i] == latestEndStateUsed && latestEndStateUsed != end && latestEndStateUsed != start) {

                    for (int j = i - 1; j >= 0; j--) {

                        if (stateBuffer[j].teleport == true) {

                            t = 1;
                            dontLerp = true;

                        }

                        if (stateBuffer[j] == start)
                        break;

                    }

                    break;

                }

            }

            latestEndStateUsed = end;

            if (end.teleport == true) {

                t = 1;
                dontLerp = true;

            }

        }

        void Extrapolate(float interpolationTime) {

            if (!extrapolatedLastFrame || tempState.ownerTimestamp < stateBuffer[0].ownerTimestamp) {

                tempState.CopyFromState(stateBuffer[0]);
                timeSpentExtrapolating = 0;

            }

            if (extrapolationMode != ExtrapolationMode.dontExtrapolate && stateCount >= 2) {

                if (syncVelocity == SyncMode.dontSync && !stateBuffer[0].atPositionalRest)
                tempState.velocity = (stateBuffer[0].position - stateBuffer[1].position) / (stateBuffer[0].ownerTimestamp - stateBuffer[1].ownerTimestamp);

                if (syncAngularVelocity == SyncMode.dontSync && !stateBuffer[0].atRotationalRest) {

                    Quaternion deltaRot = stateBuffer[0].rotation * Quaternion.Inverse(stateBuffer[1].rotation);
                    Vector3 eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
                    Vector3 angularVelocity = eulerRot / (stateBuffer[0].ownerTimestamp - stateBuffer[1].ownerTimestamp);
                    tempState.angularVelocity = angularVelocity;

                }

            }

            if (extrapolationMode == ExtrapolationMode.dontExtrapolate) {

                triedToExtrapolateTooFar = true;
                return;

            }

            bool hasVelocity = Mathf.Abs(tempState.velocity.x) >= 0.01f || Mathf.Abs(tempState.velocity.y) >= 0.01f || Mathf.Abs(tempState.velocity.z) >= 0.01f;
            bool hasAngularVelocity = Mathf.Abs(tempState.angularVelocity.x) >= 0.01f || Mathf.Abs(tempState.angularVelocity.y) >= 0.01f || Mathf.Abs(tempState.angularVelocity.z) >= 0.01f;

            if (!hasVelocity && !hasAngularVelocity) {

                triedToExtrapolateTooFar = true;
                return;

            }

            float timeDif = timeSpentExtrapolating == 0 ? timeDif = interpolationTime - tempState.ownerTimestamp : Time.deltaTime;

            timeSpentExtrapolating += timeDif;

            if (hasVelocity) {

                tempState.position += tempState.velocity * timeDif;

                if (Mathf.Abs(tempState.velocity.y) >= 0.01f)
                {

                    if (rb && rb.useGravity)
                    tempState.velocity += Physics.gravity * timeDif;

                }

                if (rb)
                tempState.velocity -= tempState.velocity * timeDif * rb.drag;

            }

            if (hasAngularVelocity) {

                float axisLength = timeDif * tempState.angularVelocity.magnitude;
                Quaternion angularRotation = Quaternion.AngleAxis(axisLength, tempState.angularVelocity);
                tempState.rotation = angularRotation * tempState.rotation;

                if (rb) {

                    if (rb.angularDrag > 0)
                    tempState.angularVelocity -= tempState.angularVelocity * timeDif * rb.angularDrag;

                }

            }

            if (useExtrapolationDistanceLimit && (tempState.position - stateBuffer[0].position).sqrMagnitude >= extrapolationDistanceLimit * extrapolationDistanceLimit)
            triedToExtrapolateTooFar = true;

        }
    }

}
