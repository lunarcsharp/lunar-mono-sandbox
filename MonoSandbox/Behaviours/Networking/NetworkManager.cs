using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MonoSandbox.Behaviours.Networking
{
    public class NetworkManager : MonoBehaviour, IOnEventCallback
    {
        public static NetworkManager Instance;

        private const byte SPAWN_EVENT = 1;
        private const byte DESPAWN_EVENT = 2;
        private const byte POSITION_SYNC_EVENT = 3;
        private const byte WEAPON_SYNC_EVENT = 4;

        private int _nextViewID;
        private readonly Dictionary<int, NetworkedObject> _trackedObjects = new Dictionary<int, NetworkedObject>();
        private readonly Dictionary<int, GameObject> _remoteWeapons = new Dictionary<int, GameObject>();

        private float _syncTimer;
        private const float SYNC_INTERVAL = 0.1f;

        private bool _isShutdown;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            PhotonNetwork.NetworkingClient.AddCallbackTarget(this);
        }

        void OnDisable()
        {
            PhotonNetwork.NetworkingClient.RemoveCallbackTarget(this);
        }

        public int AllocateViewID()
        {
            return _nextViewID++;
        }

        public void RegisterObject(NetworkedObject obj)
        {
            if (obj.ViewID == -1)
                obj.ViewID = AllocateViewID();

            _trackedObjects[obj.ViewID] = obj;
            SendSpawnEvent(obj);
        }

        public void UnregisterObject(int viewID, bool sendEvent = true)
        {
            if (_trackedObjects.Remove(viewID) && sendEvent)
                SendDespawnEvent(viewID);
        }

        public static void RegisterSpawned(string type, GameObject obj)
        {
            if (Instance == null || !PhotonNetwork.InRoom || obj == null) return;
            var netObj = obj.AddComponent<NetworkedObject>();
            netObj.ObjectType = type;
            Instance.RegisterObject(netObj);
        }

        public void SyncWeapon(int actorNumber, int weaponIndex, Vector3 position, Quaternion rotation)
        {
            if (!PhotonNetwork.InRoom) return;

            object[] data = new object[] { actorNumber, weaponIndex, position, rotation };
            var opts = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOpts = new SendOptions { Reliability = true };
            PhotonNetwork.NetworkingClient.OpRaiseEvent(WEAPON_SYNC_EVENT, data, opts, sendOpts);
        }

        public void ClearWeapon(int actorNumber)
        {
            SyncWeapon(actorNumber, -1, Vector3.zero, Quaternion.identity);
        }

        void Update()
        {
            if (!PhotonNetwork.InRoom) return;

            _syncTimer += Time.deltaTime;
            if (_syncTimer >= SYNC_INTERVAL)
            {
                _syncTimer = 0f;
                SendPositionSync();
            }
        }

        void SendSpawnEvent(NetworkedObject obj)
        {
            if (!PhotonNetwork.InRoom) return;

            object[] data = new object[]
            {
                obj.ViewID,
                obj.ObjectType,
                obj.transform.position,
                obj.transform.rotation,
                obj.transform.localScale,
                PhotonNetwork.LocalPlayer.ActorNumber
            };

            var opts = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOpts = new SendOptions { Reliability = true };
            PhotonNetwork.NetworkingClient.OpRaiseEvent(SPAWN_EVENT, data, opts, sendOpts);
        }

        void SendDespawnEvent(int viewID)
        {
            if (!PhotonNetwork.InRoom) return;

            object[] data = new object[] { viewID };
            var opts = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOpts = new SendOptions { Reliability = true };
            PhotonNetwork.NetworkingClient.OpRaiseEvent(DESPAWN_EVENT, data, opts, sendOpts);
        }

        void SendPositionSync()
        {
            if (_trackedObjects.Count == 0) return;

            List<object> data = new List<object>();
            foreach (var kvp in _trackedObjects)
            {
                if (kvp.Value == null || !kvp.Value.IsOwner) continue;

                data.Add(kvp.Value.ViewID);
                data.Add(kvp.Value.transform.position);
                data.Add(kvp.Value.transform.rotation);
            }

            if (data.Count == 0) return;

            var opts = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOpts = new SendOptions { Reliability = false };
            PhotonNetwork.NetworkingClient.OpRaiseEvent(POSITION_SYNC_EVENT, data.ToArray(), opts, sendOpts);
        }

        public void OnEvent(EventData eventData)
        {
            switch (eventData.Code)
            {
                case SPAWN_EVENT:
                    HandleSpawnEvent((object[])eventData.CustomData);
                    break;
                case DESPAWN_EVENT:
                    HandleDespawnEvent((object[])eventData.CustomData);
                    break;
                case POSITION_SYNC_EVENT:
                    HandlePositionSync((object[])eventData.CustomData);
                    break;
                case WEAPON_SYNC_EVENT:
                    HandleWeaponSync((object[])eventData.CustomData);
                    break;
            }
        }

        void HandleSpawnEvent(object[] data)
        {
            int viewID = (int)data[0];
            string objectType = (string)data[1];
            Vector3 position = DeserializeVector3(data[2]);
            Quaternion rotation = DeserializeQuaternion(data[3]);
            Vector3 scale = DeserializeVector3(data[4]);
            int actorNumber = (int)data[5];

            if (_trackedObjects.ContainsKey(viewID)) return;

            GameObject obj = ObjectSpawner.Create(objectType, position, rotation, scale);
            if (obj == null) return;

            NetworkedObject netObj = obj.AddComponent<NetworkedObject>();
            netObj.ViewID = viewID;
            netObj.ObjectType = objectType;
            netObj.IsOwner = false;
            netObj.ActorNumber = actorNumber;
            _trackedObjects[viewID] = netObj;
        }

        void HandleDespawnEvent(object[] data)
        {
            int viewID = (int)data[0];
            if (_trackedObjects.TryGetValue(viewID, out NetworkedObject obj))
            {
                _trackedObjects.Remove(viewID);
                if (obj != null) Destroy(obj.gameObject);
            }
        }

        void HandlePositionSync(object[] data)
        {
            for (int i = 0; i < data.Length; i += 3)
            {
                int viewID = (int)data[i];
                Vector3 position = DeserializeVector3(data[i + 1]);
                Quaternion rotation = DeserializeQuaternion(data[i + 2]);

                if (_trackedObjects.TryGetValue(viewID, out NetworkedObject obj))
                {
                    if (!obj.IsOwner && obj != null)
                    {
                        obj.TargetPosition = position;
                        obj.TargetRotation = rotation;
                    }
                }
            }
        }

        void HandleWeaponSync(object[] data)
        {
            int actorNumber = (int)data[0];
            int weaponIndex = (int)data[1];

            if (_remoteWeapons.TryGetValue(actorNumber, out GameObject existing))
            {
                Destroy(existing);
                _remoteWeapons.Remove(actorNumber);
            }

            if (weaponIndex < 0) return;

            GameObject weaponPrefab = WeaponSpawner.GetPrefab(weaponIndex);
            if (weaponPrefab == null) return;

            Transform hand = FindOtherPlayerHand(actorNumber);
            if (hand == null) return;

            GameObject weapon = Instantiate(weaponPrefab, hand);
            weapon.transform.localPosition = WeaponSpawner.GetLocalPosition(weaponIndex);
            weapon.transform.localEulerAngles = WeaponSpawner.GetLocalRotation(weaponIndex);
            if (weaponIndex == 5) weapon.transform.localScale = new Vector3(45, 45, 45);
            weapon.AddComponent<SineGunAnimation>().Efficiency = 1.3f;
            _remoteWeapons[actorNumber] = weapon;
        }

        Transform FindOtherPlayerHand(int actorNumber)
        {
            foreach (var pv in FindObjectsOfType<Photon.Pun.PhotonView>())
            {
                if (pv.Owner == null || pv.Owner.ActorNumber != actorNumber) continue;

                foreach (var tagger in FindObjectsOfType<GorillaTagger>())
                {
                    if (tagger.offlineVRRig != null)
                        return tagger.offlineVRRig.rightHandTransform;
                }

                foreach (Transform child in pv.transform)
                {
                    Transform hand = child.Find("rig/body/rightHand");
                    if (hand != null) return hand;
                }

                break;
            }
            return null;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                _isShutdown = true;
                Instance = null;
            }
        }

        static Vector3 DeserializeVector3(object data)
        {
            if (data is Vector3 v) return v;
            if (data is float[] arr && arr.Length >= 3)
                return new Vector3(arr[0], arr[1], arr[2]);
            return Vector3.zero;
        }

        static Quaternion DeserializeQuaternion(object data)
        {
            if (data is Quaternion q) return q;
            if (data is float[] arr && arr.Length >= 4)
                return new Quaternion(arr[0], arr[1], arr[2], arr[3]);
            return Quaternion.identity;
        }
    }
}
