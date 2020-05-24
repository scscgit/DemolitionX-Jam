using Mirror;
using UnityEngine;

namespace HardCoreGameDevs.Networking {

    public partial class VehicleSync : NetworkBehaviour
    {
        float ownerTime;
        float lastTimeOwnerTimeWasSet;

        public float approximateNetworkTimeOnOwner
        {
            get
            {
                return ownerTime + (Time.realtimeSinceStartup - lastTimeOwnerTimeWasSet);
            }
            set
            {
                ownerTime = value;
                lastTimeOwnerTimeWasSet = Time.realtimeSinceStartup;
            }
        }

        float latestAuthorityChangeZeroTime;
        int previousReceivedOwnerInt = 1;
        public int ownerChangeIndicator = 1;
        public int receivedStatesCounter;
        void AdjustOwnerTime() {

            if (stateBuffer[0] == null || (stateBuffer[0].atPositionalRest && stateBuffer[0].atRotationalRest))
            return;

            float newTime = stateBuffer[0].ownerTimestamp;
            float timeCorrection = timeCorrectionSpeed * Time.deltaTime;

            if (firstReceivedMessageZeroTime == 0)
            firstReceivedMessageZeroTime = Time.realtimeSinceStartup;


            float timeChangeMagnitude = Mathf.Abs(approximateNetworkTimeOnOwner - newTime);

            if (receivedStatesCounter < sendRate || timeChangeMagnitude < timeCorrection || timeChangeMagnitude > snapTimeThreshold)
            approximateNetworkTimeOnOwner = newTime;
            else if (approximateNetworkTimeOnOwner < newTime)
            approximateNetworkTimeOnOwner += timeCorrection;
            else
            approximateNetworkTimeOnOwner -= timeCorrection;

        }

    }

}
