using UnityEngine;
using Mirror;

namespace HardCoreGameDevs.Networking {

    public partial class VehicleSync : NetworkBehaviour
    {
        [Header("Extrapolation (Network Prediction)")]
        [SerializeField] private float interpolationBackTime = 0.1f;
        [SerializeField] private ExtrapolationMode extrapolationMode = ExtrapolationMode.extrapolate;
        [SerializeField] private bool useExtrapolationTimeLimit = true;
        [SerializeField] private float extrapolationTimeLimit = 5.0f;
        [SerializeField] private bool useExtrapolationDistanceLimit = false;
        [SerializeField] private float extrapolationDistanceLimit = 20.0f;

        [Header("Send Tresholds")]
        [Range(0f, 100f)] public float sendRate = 30;
        [Range(0, 1), SerializeField] private float sendPositionThreshold = 0.0f;
        [Range(0, 1), SerializeField] private float sendRotationThreshold = 0.0f;
        [Range(0, 1), SerializeField] private float sendVelocityThreshold = 0.0f;
        [Range(0, 1), SerializeField] private float sendAngularVelocityThreshold = 0.0f;
        [Range(0, 1), SerializeField] private float receivedPositionThreshold = 0.0f;
        [Range(0, 1), SerializeField] private float receivedRotationThreshold = 0.0f;
        [Range(0, 1), SerializeField] private float snapPositionThreshold = 0;
        [Range(0, 1), SerializeField] private float snapRotationThreshold = 0;
        [Range(0, 1), SerializeField] private float snapScaleThreshold = 0;
        [Range(0, 1), SerializeField] private float positionLerpSpeed = .85f;
        [Range(0, 1), SerializeField] private float rotationLerpSpeed = .85f;
        [Range(0, 1), SerializeField] private float scaleLerpSpeed = .85f;
        [Range(0, 5), SerializeField] private float timeCorrectionSpeed = .1f;
        [SerializeField] private float snapTimeThreshold = 3.0f;

        [Header("Preferences")]
        public bool setVelocityInsteadOfPositionOnNonOwners = false;
        public float maxPositionDifferenceForVelocitySyncing = 10;
        public bool useLocalTransformOnly = false;

        [Header("Syncronozation")]
        [SerializeField] private SyncMode syncPosition = SyncMode.sync;
        [SerializeField] private SyncMode syncRotation = SyncMode.sync;
        [SerializeField] private SyncMode syncVelocity = SyncMode.sync;
        [SerializeField] private SyncMode syncAngularVelocity = SyncMode.sync;

        [HideInInspector] public bool isAuthorityChanged = false;

        [Header("Network Channel")]
        public int networkChannel = Channels.DefaultUnreliable;

        public bool ValidateState(VehicleState lastState, VehicleState currentState) {

            return ((currentState.position - lastState.position).sqrMagnitude < 1000f &&
            (currentState.ownerTimestamp - lastState.receivedOnServerTimestamp > 0.5f));

        }

        VehicleState latestValidatedState;
        [HideInInspector] public VehicleState[] stateBuffer;
        [HideInInspector] public int stateCount;
        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public bool dontLerp = false;
        [HideInInspector] public float firstReceivedMessageZeroTime;

        [HideInInspector] public float lastTimeStateWasSent;
        [HideInInspector] public Vector3 PreviousPosition;
        [HideInInspector] public Quaternion previousRotation = Quaternion.identity;
        [HideInInspector] public Vector3 previousLinearVelocity;
        [HideInInspector] public Vector3 previousAngularVelocity;
        [HideInInspector] public NetworkIdentity netID;
        [HideInInspector] public GameObject objectToSync;
        [HideInInspector] public int syncIndex = 0;
        [HideInInspector] public VehicleSync[] childObjectVehicleSyncs = new VehicleSync[0];

        [HideInInspector] public bool forceStateSend = false;
        [HideInInspector] public bool sendAtPositionalRestMessage = false;
        [HideInInspector] public bool sendAtRotationalRestMessage = false;

        [HideInInspector] public bool sendPosition;
        [HideInInspector] public bool sendRotation;
        [HideInInspector] public bool sendVelocity;
        [HideInInspector] public bool sendAngularVelocity;
        [HideInInspector] public VehicleState tempState;
        [HideInInspector] public NetworkVehicleMessageBase sentState;
        [HideInInspector] public Vector3 latestReceivedVelocity;
        [HideInInspector] public Vector3 latestReceivedAngularVelocity;
        [HideInInspector] public float timeSpentExtrapolating = 0;
        [HideInInspector] public bool extrapolatedLastFrame = false;
        [HideInInspector] public Vector3 positionLastFrame;
        [HideInInspector] public Quaternion rotationLastFrame;
        [HideInInspector] public int restThresholdCount = 3;

        private int samePositionCount;
        private int sameRotationCount;
        private CurrentState CurrentStatePosition = CurrentState.moving;
        private CurrentState CurrentStateRotation = CurrentState.moving;
        private VehicleState latestEndStateUsed;
        private Vector3 latestTeleportedFromPosition;
        private Quaternion latestTeleportedFromRotation;

        public bool hasAuthorityOrUnownedOnServer {

            get {

                return base.hasAuthority || (NetworkServer.active && netIdentity.connectionToClient == null);

            }

        }

        public void Awake() {

            int bufferSize = ((int)(sendRate * interpolationBackTime) + 1) * 2;
            stateBuffer = new VehicleState[Mathf.Max(bufferSize, 30)];

            objectToSync = gameObject;
            netID = objectToSync.GetComponent<NetworkIdentity>();

            tempState = new VehicleState();
            sentState = new NetworkVehicleMessageBase();

        }

        public void OnEnable() {

            if (!NetworkServer.active)
            NetworkClient.RegisterHandler<NetworkVehicleMessageBase>(Sync);

        }

        //Need to do stuff in update for objects without rigidbody, and we don't have anything like that in our game

        void FixedUpdate() {

            if (!hasAuthorityOrUnownedOnServer) {

                AdjustOwnerTime();
                InterpolateOrExtrapolate();

            }

            SendState();

            positionLastFrame = CurrentPosition();
            rotationLastFrame = CurrentRotation();

            ResetVars();
        }

        public override void OnStartAuthority() {

            base.OnStartAuthority();
            TeleportOwnerObject();

        }

        void SendState() {

            if (!hasAuthorityOrUnownedOnServer || (!NetworkServer.active && !ClientScene.ready) || sendRate == 0)
            return;

            if (syncPosition != SyncMode.dontSync) {

                if (positionLastFrame == CurrentPosition()) {

                    if (CurrentStatePosition != CurrentState.rest)
                    samePositionCount++;

                    if (samePositionCount == restThresholdCount) {

                        samePositionCount = 0;
                        CurrentStatePosition = CurrentState.rest;
                        forceStateSend = true;

                    }

                }
                else {

                    if (CurrentStatePosition == CurrentState.rest && CurrentPosition() != latestTeleportedFromPosition) {

                        CurrentStatePosition = CurrentState.startedMovement;
                        forceStateSend = true;

                    }
                    else if (CurrentStatePosition == CurrentState.startedMovement) {

                        CurrentStatePosition = CurrentState.moving;

                    }
                    else {

                        samePositionCount = 0;

                    }
                }
            }
            else {

                CurrentStatePosition = CurrentState.rest;

            }

            if (syncRotation != SyncMode.dontSync) {

                if (rotationLastFrame == CurrentRotation()) {

                    if (CurrentStateRotation != CurrentState.rest)
                    sameRotationCount++;

                    if (sameRotationCount == restThresholdCount) {

                        sameRotationCount = 0;
                        CurrentStateRotation = CurrentState.rest;
                        forceStateSend = true;

                    }
                }
                else {

                    if (CurrentStateRotation == CurrentState.rest && CurrentRotation() != latestTeleportedFromRotation) {

                        CurrentStateRotation = CurrentState.startedMovement;
                        forceStateSend = true;

                    }
                    else if (CurrentStateRotation == CurrentState.startedMovement) {

                        CurrentStateRotation = CurrentState.moving;

                    }
                    else {

                        sameRotationCount = 0;

                    }
                }
            }
            else {

                CurrentStateRotation = CurrentState.rest;

            }

            if (Time.realtimeSinceStartup - lastTimeStateWasSent < GetNetworkSendInterval() && !forceStateSend)
            return;

            sendPosition = SendPosition();
            sendRotation = SendRotation();
            sendVelocity = SendVelocity();
            sendAngularVelocity = SendAngularVelocity();

            if (!sendPosition && !sendRotation && !sendVelocity && !sendAngularVelocity)
            return;

            sentState.CopyFromVehicleSync(this);

            if (CurrentStatePosition == CurrentState.rest)
            sendAtPositionalRestMessage = true;

            if (CurrentStateRotation == CurrentState.rest)
            sendAtRotationalRestMessage = true;

            if (CurrentStatePosition == CurrentState.startedMovement)
            sentState.vehicleState.position = PreviousPosition;

            if (CurrentStateRotation == CurrentState.startedMovement)
            sentState.vehicleState.rotation = previousRotation;

            if (CurrentStatePosition == CurrentState.startedMovement || CurrentStateRotation == CurrentState.startedMovement) {

                sentState.vehicleState.ownerTimestamp = Time.realtimeSinceStartup - Time.deltaTime;

                if (CurrentStatePosition != CurrentState.startedMovement)
                sentState.vehicleState.position = positionLastFrame;

                if (CurrentStateRotation != CurrentState.startedMovement)
                sentState.vehicleState.rotation = rotationLastFrame;

            }

            lastTimeStateWasSent = Time.realtimeSinceStartup;

            if (NetworkServer.active) {

                SendStateToNonOwners(sentState);

                if (sendPosition)
                PreviousPosition = sentState.vehicleState.position;

                if (sendRotation)
                previousRotation = sentState.vehicleState.rotation;

                if (sendVelocity)
                previousLinearVelocity = sentState.vehicleState.velocity;

                if (sendAngularVelocity)
                previousAngularVelocity = sentState.vehicleState.angularVelocity;

            }
            else if (NetworkClient.active) {

                NetworkClient.Send<NetworkVehicleMessageBase>(sentState, networkChannel);

            }

        }

        public Vector3 CurrentPosition() {

            return (useLocalTransformOnly) ?
            objectToSync.transform.localPosition :
            objectToSync.transform.position;

        }

        public Quaternion CurrentRotation() {

            return (useLocalTransformOnly) ?
            objectToSync.transform.localRotation :
            objectToSync.transform.rotation;

        }

        public void Position(Vector3 position, bool isTeleporting) {

            if (useLocalTransformOnly) {

                objectToSync.transform.localPosition = position;

            }
            else {

                if (rb && !isTeleporting) {

                    rb.MovePosition(position);

                }
                else {

                    objectToSync.transform.position = position;

                }

            }

        }

        public void Rotation(Quaternion rotation, bool isTeleporting) {

            if (useLocalTransformOnly) {

                objectToSync.transform.localRotation = rotation;

            }
            else {

                if (rb && !isTeleporting) {

                    rb.MoveRotation(rotation);

                }

                else {

                    objectToSync.transform.rotation = rotation;

                }
            }
        }

        void ResetVars() {

            forceStateSend = false;
            sendAtPositionalRestMessage = false;
            sendAtRotationalRestMessage = false;

        }

        public void AddState(VehicleState state) {

            if (stateCount > 1 && state.ownerTimestamp <= stateBuffer[0].ownerTimestamp)
            return;

            for (int i = stateBuffer.Length - 1; i >= 1; i--)
            stateBuffer[i] = stateBuffer[i - 1];

            stateBuffer[0] = state;
            stateCount = Mathf.Min(stateCount + 1, stateBuffer.Length);

        }

        public void ClearBuffer() {

            stateCount = 0;
            firstReceivedMessageZeroTime = 0;
            CurrentStatePosition = CurrentState.moving;
            CurrentStateRotation = CurrentState.moving;

        }

        public void TeleportOwnerObject() {

            if (!hasAuthorityOrUnownedOnServer) {

                if (NetworkServer.active)
                Debug.LogWarning("Server doesn't own the object... Can't teleport sorry!!");
                else
                Debug.LogWarning("Server is not actice... Can't teleport sorry!!");

                return;

            }

            latestTeleportedFromPosition = CurrentPosition();
            latestTeleportedFromRotation = CurrentRotation();

            if (NetworkServer.active)
            RpcTeleport(CurrentPosition(), CurrentRotation().eulerAngles, Time.realtimeSinceStartup);
            else if (hasAuthority)
            CmdTeleport(CurrentPosition(), CurrentRotation().eulerAngles, Time.realtimeSinceStartup);

        }

        [ClientRpc]
        public void RpcNonServerOwnedTeleportFromServer(Vector3 newPosition, Vector3 newRotation) {

            if (hasAuthorityOrUnownedOnServer) {

                Position(newPosition, true);
                Rotation(Quaternion.Euler(newRotation), true);
                TeleportOwnerObject();

            }

        }

        [Command]
        public void CmdTeleport(Vector3 position, Vector3 rotation, float tempOwnerTime) {

            RpcTeleport(position, rotation, tempOwnerTime);

            VehicleState teleportState = new VehicleState();
            teleportState.CopyFromVehicleSync(this);
            teleportState.position = position;
            teleportState.rotation = Quaternion.Euler(rotation);
            teleportState.ownerTimestamp = tempOwnerTime;
            teleportState.teleport = true;

            TeleportStateAdd(teleportState);

        }

        [ClientRpc]
        public void RpcTeleport(Vector3 position, Vector3 rotation, float tempOwnerTime) {

            if (hasAuthorityOrUnownedOnServer || NetworkServer.active)
            return;

            VehicleState teleportState = new VehicleState();
            teleportState.CopyFromVehicleSync(this);
            teleportState.position = position;
            teleportState.rotation = Quaternion.Euler(rotation);
            teleportState.ownerTimestamp = tempOwnerTime;
            teleportState.teleport = true;

            TeleportStateAdd(teleportState);

        }

        void TeleportStateAdd(VehicleState teleportState) {

            if (stateCount == 0)
            approximateNetworkTimeOnOwner = teleportState.ownerTimestamp;

            if (stateCount == 0 || teleportState.ownerTimestamp >= stateBuffer[0].ownerTimestamp) {

                for (int k = stateBuffer.Length - 1; k >= 1; k--)
                    stateBuffer[k] = stateBuffer[k - 1];

                stateBuffer[0] = teleportState;

            }
            else {

                for (int i = stateBuffer.Length - 2; i >= 0; i--) {

                    if (stateBuffer[i].ownerTimestamp > teleportState.ownerTimestamp) {

                        for (int j = stateBuffer.Length - 1; j >= 1; j--) {
                            if (j == i)
                            break;

                            stateBuffer[j] = stateBuffer[j - 1];

                        }

                        stateBuffer[i + 1] = teleportState;
                        break;

                    }

                }

            }

            stateCount = Mathf.Min(stateCount + 1, stateBuffer.Length);
        }

        public override void OnStartServer() {

            NetworkServer.RegisterHandler<NetworkVehicleMessageBase>(Sync);
            NetworkClient.RegisterHandler<NetworkVehicleMessageBase>(Sync);

        }

        public override void OnStartClient() {

            if (!NetworkServer.active)
            NetworkClient.RegisterHandler<NetworkVehicleMessageBase>(Sync);

        }

        public bool SendPosition() {

            return (syncPosition != SyncMode.dontSync &&
            (forceStateSend || (CurrentPosition() != PreviousPosition && (sendPositionThreshold == 0 ||
            (CurrentPosition() - PreviousPosition).sqrMagnitude > sendPositionThreshold * sendPositionThreshold))));

        }

        public bool SendRotation() {

            return
            (syncRotation != SyncMode.dontSync &&
            (forceStateSend || (CurrentRotation() != previousRotation && (sendRotationThreshold == 0 ||
            Quaternion.Angle(previousRotation, CurrentRotation()) > sendRotationThreshold))));

        }

        public bool SendVelocity() {

            return rb ? (syncVelocity != SyncMode.dontSync && (forceStateSend ||
            (rb.velocity != previousLinearVelocity && (sendVelocityThreshold == 0 ||
            (previousLinearVelocity - rb.velocity).sqrMagnitude > sendVelocityThreshold * sendVelocityThreshold))))
            : false;

        }

        public bool SendAngularVelocity() {

            return rb ? (syncAngularVelocity != SyncMode.dontSync && (forceStateSend ||
            (rb.angularVelocity != previousAngularVelocity && (sendAngularVelocityThreshold == 0 ||
            (previousAngularVelocity - rb.angularVelocity * Mathf.Rad2Deg).sqrMagnitude > sendAngularVelocityThreshold * sendAngularVelocityThreshold))))
            : false;

        }

        public bool isSyncingPosition {

            get {

                return syncPosition == SyncMode.sync;

            }

        }

        public bool isSyncingRotation {

            get {

                return syncRotation == SyncMode.sync;

            }

        }

        public bool isSyncingVelocity {

            get {

                return syncVelocity == SyncMode.sync;

            }

        }

        public bool isSyncingAngularVelocity {

            get {

                return syncAngularVelocity == SyncMode.sync;

            }

        }

        [Server]
        void SendStateToNonOwners(NetworkVehicleMessageBase state) {

            foreach (var kv in netID.observers) {

                NetworkConnection conn = kv.Value;

                if (conn != null && conn != netID.connectionToClient &&
                conn.GetType() == typeof(NetworkConnectionToClient) && conn.isReady)
                conn.Send<NetworkVehicleMessageBase>(state, networkChannel);

            }

        }

        static void Sync(NetworkConnection conn, NetworkVehicleMessageBase networkState) {

            if (!NetworkServer.active) {

                if (networkState != null && networkState.VehicleSync != null && !networkState.VehicleSync.hasAuthorityOrUnownedOnServer) {

                    networkState.VehicleSync.AddState(networkState.vehicleState);
                    networkState.VehicleSync.CheckOwnerChange(networkState.vehicleState);

                }

            }
            else {

                if (networkState.VehicleSync == null || networkState.VehicleSync.netID.connectionToClient != conn)
                return;

                if (networkState.VehicleSync.latestValidatedState == null || networkState.VehicleSync.ValidateState(networkState.vehicleState, networkState.VehicleSync.latestValidatedState)) {

                    networkState.VehicleSync.latestValidatedState = networkState.vehicleState;
                    networkState.VehicleSync.latestValidatedState.receivedOnServerTimestamp = Time.realtimeSinceStartup;
                    networkState.VehicleSync.SendStateToNonOwners(networkState);
                    networkState.VehicleSync.AddState(networkState.vehicleState);
                    networkState.VehicleSync.CheckOwnerChange(networkState.vehicleState);

                }

            }

        }

        public void CheckOwnerChange(VehicleState newState) {

            if (isAuthorityChanged && ownerChangeIndicator != previousReceivedOwnerInt) {

                approximateNetworkTimeOnOwner = newState.ownerTimestamp;
                latestAuthorityChangeZeroTime = Time.realtimeSinceStartup;
                stateCount = 0;
                firstReceivedMessageZeroTime = 1.0f;
                CurrentStatePosition = CurrentState.moving;
                CurrentStateRotation = CurrentState.moving;

                VehicleState simulatedState = new VehicleState();
                simulatedState.position = CurrentPosition();
                simulatedState.rotation = CurrentRotation();
                simulatedState.ownerTimestamp = stateBuffer[0].ownerTimestamp - interpolationBackTime;
                AddState(simulatedState);

                previousReceivedOwnerInt = ownerChangeIndicator;

            }

        }

        public float GetNetworkSendInterval() {

            return sendRate == 0 ? 0 : 1/sendRate;

        }

    }

    [System.Serializable]
    public enum ExtrapolationMode {

        extrapolate, dontExtrapolate

    }

    [System.Serializable]
    public enum SyncMode {

        sync, dontSync

    }

    [System.Serializable]
    enum CurrentState {

        rest, startedMovement, moving

    }
}
