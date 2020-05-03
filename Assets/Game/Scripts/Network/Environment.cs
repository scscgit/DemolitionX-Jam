using System.Collections.Generic;
using Game.Scripts.Util;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Network
{
    public class Environment : NetworkBehaviour
    {
        public GameObject spawnObjects;
        public GameObject oilPrefab;

        private readonly IList<GameObject> _spawnedObjects = new List<GameObject>();

        private void Start()
        {
            // The client also needs to disable these "development-intended" default spawn objects to prevent conflicts
            spawnObjects.SetActive(false);
            if (!isServer)
            {
                return;
            }

            // Spawn all objects on the server
            SpawnAll();
        }

        private void SpawnAll()
        {
            _spawnedObjects.Clear();
            foreach (var identity in spawnObjects.GetComponentsInChildren<NetworkIdentity>())
            {
                // TODO: generic way of determining which prefab is it. Only a prefab can be spawned.
                var spawn = Instantiate(oilPrefab);
                var spawnTransform = spawn.transform;
                var identityTransform = identity.transform;
                spawnTransform.position = identityTransform.position;
                spawnTransform.rotation = identityTransform.rotation;
                _spawnedObjects.Add(spawn);
                // Spawn as un-parented in order to avoid missing inherited position and prevent collisions on spawn
                // TODO: optional?
                spawn.ExecuteWithoutParent(o => NetworkServer.Spawn(o));
            }
        }

        public void RespawnAll()
        {
            foreach (var spawnedObject in _spawnedObjects)
            {
                Destroy(spawnedObject);
            }

            SpawnAll();
        }
    }
}
