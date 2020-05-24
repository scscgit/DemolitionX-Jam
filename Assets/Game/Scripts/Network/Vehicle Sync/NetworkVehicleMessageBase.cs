using UnityEngine;
using Mirror;

namespace HardCoreGameDevs.Networking {

    public class NetworkVehicleMessageBase : MessageBase {

        public VehicleSync VehicleSync;
        public VehicleState vehicleState = new VehicleState();
        public NetworkVehicleMessageBase() { }

        public void CopyFromVehicleSync(VehicleSync VehicleSyncScript) {

            this.VehicleSync = VehicleSyncScript;
            vehicleState.CopyFromVehicleSync(VehicleSyncScript);

        }

        override public void Serialize(NetworkWriter writer) {

            bool sendPosition, sendRotation, sendVelocity, sendAngularVelocity, sendAtPositionalRestTag, sendAtRotationalRestTag;

            if (NetworkServer.active && !VehicleSync.hasAuthorityOrUnownedOnServer) {

                sendPosition = vehicleState.serverShouldRelayPosition;
                sendRotation = vehicleState.serverShouldRelayRotation;
                sendVelocity = vehicleState.serverShouldRelayVelocity;
                sendAngularVelocity = vehicleState.serverShouldRelayAngularVelocity;
                sendAtPositionalRestTag = vehicleState.atPositionalRest;
                sendAtRotationalRestTag = vehicleState.atRotationalRest;

            }
            else {

                sendPosition = VehicleSync.sendPosition;
                sendRotation = VehicleSync.sendRotation;
                sendVelocity = VehicleSync.sendVelocity;
                sendAngularVelocity = VehicleSync.sendAngularVelocity;
                sendAtPositionalRestTag = VehicleSync.sendAtPositionalRestMessage;
                sendAtRotationalRestTag = VehicleSync.sendAtRotationalRestMessage;

            }

            if (!NetworkServer.active) {

                if (sendPosition)
                VehicleSync.PreviousPosition = vehicleState.position;

                if (sendRotation)
                VehicleSync.previousRotation = vehicleState.rotation;

                if (sendVelocity)
                VehicleSync.previousLinearVelocity = vehicleState.velocity;

                if (sendAngularVelocity)
                VehicleSync.previousAngularVelocity = vehicleState.angularVelocity;

            }

            writer.WriteByte(MakeByteFromBool(sendPosition, sendRotation,
                sendVelocity, sendAngularVelocity, sendAtPositionalRestTag, sendAtRotationalRestTag));
            writer.WriteNetworkIdentity(VehicleSync.netID);
            writer.WritePackedUInt32((uint)VehicleSync.syncIndex);
            writer.WriteSingle(vehicleState.ownerTimestamp);

            if (sendPosition) {

                if (VehicleSync.isSyncingPosition) {

                    writer.WriteSingle(vehicleState.position.x);
                    writer.WriteSingle(vehicleState.position.y);
                    writer.WriteSingle(vehicleState.position.z);

                }

            }

            if (sendRotation)
            {
                Vector3 rot = vehicleState.rotation.eulerAngles;

                if (VehicleSync.isSyncingRotation) {

                    writer.WriteSingle(rot.x);
                    writer.WriteSingle(rot.y);
                    writer.WriteSingle(rot.z);

                }

            }

            if (sendVelocity)
            {

                if (VehicleSync.isSyncingVelocity) {

                    writer.WriteSingle(vehicleState.velocity.x);
                    writer.WriteSingle(vehicleState.velocity.y);
                    writer.WriteSingle(vehicleState.velocity.z);

                }

            }

            if (sendAngularVelocity)
            {

                if (VehicleSync.isSyncingAngularVelocity) {

                    writer.WriteSingle(vehicleState.angularVelocity.x);
                    writer.WriteSingle(vehicleState.angularVelocity.y);
                    writer.WriteSingle(vehicleState.angularVelocity.z);

                }

            }

            if (VehicleSync.isAuthorityChanged && NetworkServer.active)
            writer.WriteByte((byte)VehicleSync.ownerChangeIndicator);

        }

        byte p = 1, r = 2, v = 8, av = 16, apr = 64, arr = 128;

        byte MakeByteFromBool(bool sendPosition, bool sendRotation, bool sendVelocity, bool sendAngularVelocity, bool atPositionalRest, bool atRotationalRest) {

            byte b = 0;

            if (sendPosition)
            b = (byte)(b | p);

            if (sendRotation)
            b = (byte)(b | r);

            if (sendVelocity)
            b = (byte)(b | v);

            if (sendAngularVelocity)
            b = (byte)(b | av);

            if (atPositionalRest)
            b = (byte)(b | apr);

            if (atRotationalRest)
            b = (byte)(b | arr);

            return b;

        }

        override public void Deserialize(NetworkReader reader) {

            byte syncInfoByte = reader.ReadByte();
            bool syncPosition = (syncInfoByte & p) == p;
            bool syncRotation = (syncInfoByte & r) == r;
            bool syncVelocity = (syncInfoByte & v) == v;
            bool syncAngularVelocity = (syncInfoByte & av) == av;
            vehicleState.atPositionalRest = (syncInfoByte & apr) == apr;
            vehicleState.atRotationalRest = (syncInfoByte & arr) == arr;

            NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();

            if (!networkIdentity) {

                Debug.LogWarning("Where is Network Identity on Vehicle?");
                return;

            }

            uint netID = networkIdentity.netId;
            int syncIndex = (int)reader.ReadPackedUInt32();
            vehicleState.ownerTimestamp = reader.ReadSingle();

            GameObject go = NetworkIdentity.spawned[netID].gameObject;

            if (!go) {

                Debug.LogWarning("Where is Vehicle?");
                return;

            }

            VehicleSync = go.GetComponent<VehicleSync>();

            if (NetworkServer.active && !VehicleSync.hasAuthorityOrUnownedOnServer) {

                vehicleState.serverShouldRelayPosition = syncPosition;
                vehicleState.serverShouldRelayRotation = syncRotation;
                vehicleState.serverShouldRelayVelocity = syncVelocity;
                vehicleState.serverShouldRelayAngularVelocity = syncAngularVelocity;

            }

            for (int i = 0; i < VehicleSync.childObjectVehicleSyncs.Length; i++) {

                if (VehicleSync.childObjectVehicleSyncs[i].syncIndex == syncIndex)
                VehicleSync = VehicleSync.childObjectVehicleSyncs[i];

            }

            if (VehicleSync.receivedStatesCounter < VehicleSync.sendRate)
            VehicleSync.receivedStatesCounter++;

            if (syncPosition) {

                if(VehicleSync.isSyncingPosition) {

                    vehicleState.position.x = reader.ReadSingle();
                    vehicleState.position.y = reader.ReadSingle();
                    vehicleState.position.z = reader.ReadSingle();

                }

            }
            else {

                if (VehicleSync.stateCount > 0)
                vehicleState.position = VehicleSync.stateBuffer[0].position;

                else
                vehicleState.position = VehicleSync.CurrentPosition();

            }

            if (syncRotation) {

                vehicleState.reusableRotationVector = Vector3.zero;

                if (VehicleSync.isSyncingRotation) {

                    vehicleState.reusableRotationVector.x = reader.ReadSingle();
                    vehicleState.reusableRotationVector.y = reader.ReadSingle();
                    vehicleState.reusableRotationVector.z = reader.ReadSingle();

                }

                vehicleState.rotation = Quaternion.Euler(vehicleState.reusableRotationVector);

            }
            else {

                if (VehicleSync.stateCount > 0)
                vehicleState.rotation = VehicleSync.stateBuffer[0].rotation;

                else
                vehicleState.rotation = VehicleSync.CurrentRotation();

            }

            if (syncVelocity) {

                if (VehicleSync.isSyncingVelocity) {

                    vehicleState.velocity.x = reader.ReadSingle();
                    vehicleState.velocity.y = reader.ReadSingle();
                    vehicleState.velocity.z = reader.ReadSingle();

                }

                VehicleSync.latestReceivedVelocity = vehicleState.velocity;

            }
            else {

                vehicleState.velocity = VehicleSync.latestReceivedVelocity;

            }

            if (syncAngularVelocity) {

                if (VehicleSync.isSyncingAngularVelocity) {

                    vehicleState.angularVelocity.x = reader.ReadSingle();
                    vehicleState.angularVelocity.y = reader.ReadSingle();
                    vehicleState.angularVelocity.z = reader.ReadSingle();

                }

                VehicleSync.latestReceivedAngularVelocity = vehicleState.angularVelocity;
            }
            else {

                vehicleState.angularVelocity = VehicleSync.latestReceivedAngularVelocity;

            }

            if (VehicleSync.isAuthorityChanged && !NetworkServer.active) {

                VehicleSync.ownerChangeIndicator = (int)reader.ReadByte();

            }
        }

    }

}
