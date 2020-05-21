//#pragma warning disable 0414 // private field assigned but not used.

using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Streaming player input, or receiving data from server. And then feeds the RCC.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class VehicleSync : NetworkBehaviour
{
    // Network Identity of the vehicle. All networked gameobjects must have this component.
    private NetworkIdentity networkID;

    // Main RCC, Rigidbody, and WheelColliders.
    public VehiclePhysics carController;
    public VehiclePhysicsWheelCollider[] wheelColliders;
    public Rigidbody rigid;
    public bool l; // => isLocalPlayer;

    // Syncing transform, inputs, configurations, and lights of the vehicle. Not using any SyncVar. All synced data is organized via structs and lists.

    #region STRUCTS

    #region Transform

    public struct VehicleTransform
    {
        public Vector3 position;
        public Quaternion rotation;

        public Vector3 rigidLinearVelocity;
        public Vector3 rigidAngularVelocity;
    }

    public class SyncListVehicleTransform : SyncList<VehicleTransform>
    {
    }

    public SyncListVehicleTransform m_Transform = new SyncListVehicleTransform();

    #endregion Transform

    #region Inputs

    public struct VehicleInput
    {
        public float gasInput;
        public float brakeInput;
        public float steerInput;
        public float handbrakeInput;
        public float boostInput;
        public float clutchInput;
        public float idleInput;
        public int gear;
        public int direction;
        public bool changingGear;
        public float fuelInput;
        public bool engineRunning;
        public bool canGoReverseNow;
    }

    public class SyncListVehicleInput : SyncList<VehicleInput>
    {
    }

    public SyncListVehicleInput m_Inputs = new SyncListVehicleInput();

    #endregion Inputs

    #endregion

    private float updateTime = 0;

    VehicleInput currentVehicleInputs = new VehicleInput();

    VehicleTransform currentVehicleTransform = new VehicleTransform();

    // TODO: there was = new VehicleLights();
    VehicleLights currentVehicleLights;

    bool CB_running = false;

    void Start()
    {
        //l = true;

        // Getting RCC, wheelcolliders, rigidbody, and Network Identity of the vehicle.
        carController = GetComponent<VehiclePhysics>();
        wheelColliders = GetComponentsInChildren<VehiclePhysicsWheelCollider>();
        rigid = GetComponent<Rigidbody>();
        networkID = GetComponent<NetworkIdentity>();

        // If we are the owner of this vehicle, disable external controller and enable controller of the vehicle. Do opposite if we don't own this.
        /*if (isLocalPlayer){

            carController.canControl = true;
            //carController.StartEngine();

        }else{

            carController.canControl = false;
        }*/

        // Setting name of the gameobject with Network ID.
        gameObject.name = gameObject.name + networkID.netId;

        currentVehicleTransform = new VehicleTransform();
    }

    void FixedUpdate()
    {
        // If we are the owner of this vehicle, disable external controller and enable controller of the vehicle. Do opposite if we don't own this.
        //carController.canControl = isLocalPlayer;

        if (l)
        {
            // If we are owner of this vehicle, stream all inputs from vehicle.
//			VehicleIsMein ();
            if (!CB_running)
                StartCoroutine(VehicleIsMein());
        }
        else
        {
            // If we are not owner of this vehicle, receive all inputs from server.
            VehicleIsClient();
        }
    }

    IEnumerator VehicleIsMein()
    {
        CB_running = true;

        currentVehicleInputs.gasInput = carController.gasInput;
        currentVehicleInputs.brakeInput = carController.brakeInput;
        currentVehicleInputs.steerInput = carController.steerInput;
        currentVehicleInputs.handbrakeInput = carController.handbrakeInput;
        currentVehicleInputs.boostInput = carController.boostInput;
        currentVehicleInputs.clutchInput = carController.clutchInput;
        currentVehicleInputs.idleInput = carController.idleInput;
        currentVehicleInputs.gear = carController.currentGear;
        currentVehicleInputs.direction = carController.direction;
        currentVehicleInputs.changingGear = carController.changingGear;
        currentVehicleInputs.fuelInput = carController.fuelInput;
        currentVehicleInputs.engineRunning = carController.engineRunning;
        currentVehicleInputs.canGoReverseNow = carController.canGoReverseNow;

        CmdVehicleInputs(currentVehicleInputs);

        yield return new WaitForSeconds(.02f);

        currentVehicleTransform.position = transform.position;
        currentVehicleTransform.rotation = transform.rotation;
        currentVehicleTransform.rigidLinearVelocity = rigid.velocity;
        currentVehicleTransform.rigidAngularVelocity = rigid.angularVelocity;

        CmdVehicleTransform(currentVehicleTransform);

        yield return new WaitForSeconds(.02f);

        CB_running = false;
    }

    void VehicleIsClient()
    {
        if (m_Inputs != null && m_Inputs.Count >= 1)
        {
            carController.gasInput = m_Inputs[0].gasInput;
            carController.brakeInput = m_Inputs[0].brakeInput;
            carController.steerInput = m_Inputs[0].steerInput;
            carController.handbrakeInput = m_Inputs[0].handbrakeInput;
            carController.boostInput = m_Inputs[0].boostInput;
            carController.clutchInput = m_Inputs[0].clutchInput;
            carController.idleInput = m_Inputs[0].idleInput;
            carController.currentGear = m_Inputs[0].gear;
            carController.direction = m_Inputs[0].direction;
            carController.changingGear = m_Inputs[0].changingGear;
            carController.fuelInput = m_Inputs[0].fuelInput;
            carController.engineRunning = m_Inputs[0].engineRunning;
            carController.canGoReverseNow = m_Inputs[0].canGoReverseNow;
        }

        if (m_Transform != null && m_Transform.Count >= 1)
        {
            Vector3 projectedPosition =
                m_Transform[0].position + m_Transform[0].rigidLinearVelocity * (Time.time - updateTime);

            if (Vector3.Distance(transform.position, m_Transform[0].position) < 15f)
            {
                transform.position = Vector3.Lerp(transform.position, projectedPosition, Time.deltaTime * 5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, m_Transform[0].rotation, Time.deltaTime * 5f);
            }
            else
            {
                transform.position = m_Transform[0].position;
                transform.rotation = m_Transform[0].rotation;
            }
        }

        updateTime = Time.time;

        // DıSABLED
//		if (m_Configurations != null && m_Configurations.Count >= 1) {
//
//			carController.currentGear = m_Configurations[m_Configurations.Count - 1].gear;
//			carController.direction = m_Configurations[m_Configurations.Count - 1].direction;
//			carController.changingGear = m_Configurations[m_Configurations.Count - 1].changingGear;
//			carController.semiAutomaticGear = m_Configurations[m_Configurations.Count - 1].semiAutomaticGear;
//
//			carController.fuelInput = m_Configurations[m_Configurations.Count - 1].fuelInput;
//			carController.engineRunning = m_Configurations[m_Configurations.Count - 1].engineRunning;
//
//			for (int i = 0; i < wheelColliders.Length; i++) {
//
//				wheelColliders [i].camber = m_Configurations[m_Configurations.Count - 1].cambers [i];
//
//			}
//
//			carController.applyEngineTorqueToExtraRearWheelColliders = m_Configurations[m_Configurations.Count - 1].applyEngineTorqueToExtraRearWheelColliders;
//			carController._wheelTypeChoise = m_Configurations[m_Configurations.Count - 1].wheelTypeChoise;
//			carController.biasedWheelTorque = m_Configurations[m_Configurations.Count - 1].biasedWheelTorque;
//			carController.canGoReverseNow = m_Configurations[m_Configurations.Count - 1].canGoReverseNow;
//			carController.engineTorque = m_Configurations[m_Configurations.Count - 1].engineTorque;
//			carController.brakeTorque = m_Configurations[m_Configurations.Count - 1].brakeTorque;
//			carController.minEngineRPM = m_Configurations[m_Configurations.Count - 1].minEngineRPM;
//			carController.maxEngineRPM = m_Configurations[m_Configurations.Count - 1].maxEngineRPM;
//			carController.engineInertia = m_Configurations[m_Configurations.Count - 1].engineInertia;
//			carController.useRevLimiter = m_Configurations[m_Configurations.Count - 1].useRevLimiter;
//			carController.useExhaustFlame = m_Configurations[m_Configurations.Count - 1].useExhaustFlame;
//			carController.useClutchMarginAtFirstGear = m_Configurations[m_Configurations.Count - 1].useClutchMarginAtFirstGear;
//			carController.highspeedsteerAngle = m_Configurations[m_Configurations.Count - 1].highspeedsteerAngle;
//			carController.highspeedsteerAngleAtspeed = m_Configurations[m_Configurations.Count - 1].highspeedsteerAngleAtspeed;
//			carController.antiRollFrontHorizontal = m_Configurations[m_Configurations.Count - 1].antiRollFrontHorizontal;
//			carController.antiRollRearHorizontal = m_Configurations[m_Configurations.Count - 1].antiRollRearHorizontal;
//			carController.antiRollVertical = m_Configurations[m_Configurations.Count - 1].antiRollVertical;
//			carController.maxspeed = m_Configurations[m_Configurations.Count - 1].maxspeed;
//			carController.engineHeat = m_Configurations[m_Configurations.Count - 1].engineHeat;
//			carController.engineHeatRate = m_Configurations[m_Configurations.Count - 1].engineHeatMultiplier;
//			carController.totalGears = m_Configurations[m_Configurations.Count - 1].totalGears;
//			carController.gearShiftingDelay = m_Configurations[m_Configurations.Count - 1].gearShiftingDelay;
//			carController.gearShiftingThreshold = m_Configurations[m_Configurations.Count - 1].gearShiftingThreshold;
//			carController.clutchInertia = m_Configurations[m_Configurations.Count - 1].clutchInertia;
//			carController.NGear = m_Configurations[m_Configurations.Count - 1].NGear;
//			carController.launched = m_Configurations[m_Configurations.Count - 1].launched;
//			carController.ABS = m_Configurations[m_Configurations.Count - 1].ABS;
//			carController.TCS = m_Configurations[m_Configurations.Count - 1].TCS;
//			carController.ESP = m_Configurations[m_Configurations.Count - 1].ESP;
//			carController.steeringHelper = m_Configurations[m_Configurations.Count - 1].steeringHelper;
//			carController.tractionHelper = m_Configurations[m_Configurations.Count - 1].tractionHelper;
//			carController.applyCounterSteering = m_Configurations[m_Configurations.Count - 1].applyCounterSteering;
//			carController.useNOS = m_Configurations[m_Configurations.Count - 1].useNOS;
//			carController.useTurbo = m_Configurations[m_Configurations.Count - 1].useTurbo;
//
//		}
    }

    [Command]
    public void CmdVehicleInputs(VehicleInput _input)
    {
        m_Inputs.Insert(0, _input);
    }

    [Command]
    public void CmdVehicleTransform(VehicleTransform _input)
    {
        m_Transform.Insert(0, _input);
    }
}
