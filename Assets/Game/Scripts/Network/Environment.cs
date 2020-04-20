using Mirror;

namespace Game.Scripts.Network
{
    public class Environment : NetworkBehaviour
    {
        public NetworkIdentity[] managedObjects;

        void Start()
        {
            if (isServer)
            {
                foreach (var managedObject in managedObjects)
                {
                    NetworkServer.Spawn(managedObject.gameObject);
                }
            }
        }
    }
}
