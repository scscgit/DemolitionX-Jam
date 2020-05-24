using UnityEngine;
using Mirror;

namespace HardCoreGameDevs.Networking {

    public class VehicleState {

        public float ownerTimestamp;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool teleport;
        public bool atPositionalRest;
        public bool atRotationalRest;
        public float receivedOnServerTimestamp;
        public Vector3 reusableRotationVector;
        public bool serverShouldRelayPosition = false;
        public bool serverShouldRelayRotation = false;
        public bool serverShouldRelayScale = false;
        public bool serverShouldRelayVelocity = false;
        public bool serverShouldRelayAngularVelocity = false;

        public VehicleState() { }

        public VehicleState CopyFromState(VehicleState state) {

            ownerTimestamp = state.ownerTimestamp;
            position = state.position;
            rotation = state.rotation;
            velocity = state.velocity;
            angularVelocity = state.angularVelocity;

            return this;
        }

        public static VehicleState Lerp(VehicleState targetTempVehicleState, VehicleState start, VehicleState end, float t) {

            targetTempVehicleState.position = Vector3.Lerp(start.position, end.position, t);
            targetTempVehicleState.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
            targetTempVehicleState.velocity = Vector3.Lerp(start.velocity, end.velocity, t);
            targetTempVehicleState.angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t);

            targetTempVehicleState.ownerTimestamp = Mathf.Lerp(start.ownerTimestamp, end.ownerTimestamp, t);

            return targetTempVehicleState;

        }

        public void ResetTheVariables() {

            ownerTimestamp = 0;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
            atPositionalRest = false;
            atRotationalRest = false;
            teleport = false;

        }

        public void CopyFromVehicleSync(VehicleSync VehicleSyncScript) {

            ownerTimestamp = Time.realtimeSinceStartup;
            position = VehicleSyncScript.CurrentPosition();
            rotation = VehicleSyncScript.CurrentRotation();

            if (VehicleSyncScript.rb) {

                velocity = VehicleSyncScript.rb.velocity;
                angularVelocity = VehicleSyncScript.rb.angularVelocity * Mathf.Rad2Deg;

            }
            else {

                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;

            }

        }

    }

}
