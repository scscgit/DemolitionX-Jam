using System.Collections.Generic;
using Game.Scripts.Util;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Network
{
    public class Environment : NetworkBehaviour
    {
        public static Environment Instance;

        public GameObject spawnObjects;
        public GameObject oilPrefab;

        private readonly IList<GameObject> _spawnedObjects = new List<GameObject>();

        private void Start()
        {
            Instance = this;
            // The client also needs to disable these "development-intended" default spawn objects to prevent conflicts
            spawnObjects.SetActive(false);
            if (!isServer)
            {
                return;
            }

            // Spawn all objects on the server
            SpawnAll();
        }

        public void RegisterObjectForCleanup(GameObject spawnedObject)
        {
            _spawnedObjects.Add(spawnedObject);
        }

        private void SpawnAll()
        {
            foreach (var identity in spawnObjects.GetComponentsInChildren<NetworkIdentity>())
            {
                // TODO: generic way of determining which prefab is it. Only a prefab can be spawned.
                var spawn = Instantiate(oilPrefab);
                var spawnTransform = spawn.transform;
                var identityTransform = identity.transform;
                spawnTransform.position = identityTransform.position;
                spawnTransform.rotation = identityTransform.rotation;
                RegisterObjectForCleanup(spawn);
                // Spawn as un-parented in order to avoid missing inherited position and prevent collisions on spawn
                // TODO: optional?
                spawn.ExecuteWithoutParent(o => NetworkServer.Spawn(o));
            }
        }

        public void RespawnAll()
        {
            RpcDespawnAllOnClient();
            DespawnAll();
            SpawnAll();
        }

        [ClientRpc]
        public void RpcDespawnAllOnClient()
        {
            // Clients need to clean up their locally spawned objects
            if (isServer)
            {
                return;
            }

            DespawnAll();
        }

        /// <summary>
        /// When called on server, de-spawns all barrels.
        /// When called on a client, de-spawns all dynamically registered local detachable objects.
        /// </summary>
        private void DespawnAll()
        {
            foreach (var spawnedObject in _spawnedObjects)
            {
                if (spawnedObject)
                {
                    Destroy(spawnedObject);
                }
            }

            _spawnedObjects.Clear();
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
