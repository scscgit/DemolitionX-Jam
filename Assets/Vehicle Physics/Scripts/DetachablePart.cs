using Game.Scripts.Network;
using UnityEngine;
using Mirror;


[DisallowMultipleComponent]
[AddComponentMenu("Detachable Part", 1)]

//Class for parts that can detach
public class DetachablePart : NetworkBehaviour
{
    //Transform tr;
    Rigidbody rb;
    //public Rigidbody parentBody;
    //Transform initialParent;
    //Vector3 initialLocalPos;
    //Quaternion initialLocalRot;

    //[System.NonSerialized]
    //public HingeJoint hinge;
    [System.NonSerialized] public bool detached;

    //[System.NonSerialized]
    //public Vector3 initialPos;
    public float mass = 0.1f;
    public float drag;

    public float angularDrag = 0.05f;

    //public float looseForce = -1;
    public float breakForce = 25;

    //[Tooltip("A hinge joint is randomly chosen from the list to use")]
    //public PartJoint[] joints;
    //Vector3 initialAnchor;
    //[System.NonSerialized]
    //public Vector3 displacedAnchor;
    Vector3 initPos;
    Quaternion initRot;
    float linearVel;
    float angularVel;
    Vector3 projectedPosition;
    Quaternion projectedRot;
    float updateTime = 0;

    private void Start()
    {
        initPos = transform.position;
        initRot = transform.rotation;
    }

    void FixedUpdate()
    {
/*
        if(detached){

            projectedPosition = transform.position + GetComponent<Rigidbody>().velocity * (Time.time - updateTime);
            projectedRot = transform.rotation;

            if(PositionChanged()){

                CmdSyncPosition();

            } 

            if(RotationChanged()) {

                CmdSyncRotation();

            }

            updateTime = Time.time;           
        }*/
    }

    bool PositionChanged()
    {
        bool changed = (transform.position - initPos).sqrMagnitude > 0.01f;

        if (changed)
        {
            initPos = transform.position;
        }

        return changed;
    }

    bool RotationChanged()
    {
        bool changed = Quaternion.Angle(initRot, transform.localRotation) > 0.01f;

        if (changed)
        {
            initRot = transform.rotation;
        }

        return changed;
    }

    [Command]
    void CmdSyncPosition()
    {
        if ((transform.position - initPos).sqrMagnitude < 10)
        {
            transform.position = Vector3.Lerp(transform.position, projectedPosition, Time.deltaTime * 5f);
        }
        else
        {
            transform.position = projectedPosition;
        }
    }

    [Command]
    void CmdSyncRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, projectedRot, Time.deltaTime * 5f);
    }

    public void Detach()
    {
        if (!detached)
        {
            // TODO: spawn the object as a persistent copy visible by newly connected clients
            // var net = gameObject.AddComponent<NetworkIdentity>();
            // gameObject.name = gameObject.name + net.netId;

            // Both server and client will remove the object for themselves after the match ends, without syncing that
            Environment.Instance.RegisterObjectForCleanup(gameObject);

            transform.parent = null;
            //tr.parent = null;
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            detached = true;

            /*if (parentBody)
            {
                parentBody.mass -= mass;
                rb.velocity = parentBody.GetPointVelocity(tr.position);
                rb.angularVelocity = parentBody.angularVelocity;

                //Pick a random hinge joint to use
                if (makeJoint && joints.Length > 0)
                {
                    PartJoint chosenJoint = joints[Random.Range(0, joints.Length)];
                    initialAnchor = chosenJoint.hingeAnchor;
                    displacedAnchor = initialAnchor;

                    hinge = gameObject.AddComponent<HingeJoint>();
                    hinge.autoConfigureConnectedAnchor = false;
                    hinge.connectedBody = parentBody;
                    hinge.anchor = chosenJoint.hingeAnchor;
                    hinge.axis = chosenJoint.hingeAxis;
                    hinge.connectedAnchor = initialPos + chosenJoint.hingeAnchor;
                    hinge.enableCollision = false;
                    hinge.useLimits = chosenJoint.useLimits;

                    JointLimits limits = new JointLimits();
                    limits.min = chosenJoint.minLimit;
                    limits.max = chosenJoint.maxLimit;
                    limits.bounciness = chosenJoint.bounciness;
                    hinge.limits = limits;
                    hinge.useSpring = chosenJoint.useSpring;

                    JointSpring spring = new JointSpring();
                    spring.targetPosition = chosenJoint.springTargetPosition;
                    spring.spring = chosenJoint.springForce;
                    spring.damper = chosenJoint.springDamper;
                    hinge.spring = spring;
                    hinge.breakForce = breakForce * 1000;
                    hinge.breakTorque = breakForce * 1000;
                }
            }*/
        }
    }

    //Draw joint gizmos
    /*void OnDrawGizmosSelected()
    {
        if (!tr)
        {
            tr = transform;
        }

        if (looseForce >= 0 && joints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (PartJoint curJoint in joints)
            {
                Gizmos.DrawRay(tr.TransformPoint(curJoint.hingeAnchor), tr.TransformDirection(curJoint.hingeAxis).normalized * 0.2f);
                Gizmos.DrawWireSphere(tr.TransformPoint(curJoint.hingeAnchor), 0.02f);
            }
        }
    }*/
}

//Class for storing hinge joint information in the joints list
/*[System.Serializable]
public class PartJoint
{
    public Vector3 hingeAnchor;
    public Vector3 hingeAxis = Vector3.right;
    public bool useLimits;
    public float minLimit;
    public float maxLimit;
    public float bounciness;
    public bool useSpring;
    public float springTargetPosition;
    public float springForce;
    public float springDamper;
}*/
