using UnityEngine;

namespace MonoSandbox.Behaviours.Networking
{
    public class NetworkedObject : MonoBehaviour
    {
        public int ViewID = -1;
        public string ObjectType;
        public bool IsOwner = true;
        public int ActorNumber;

        public Vector3 TargetPosition;
        public Quaternion TargetRotation;

        private float _lerpSpeed = 15f;
        private bool _destroying;

        void Start()
        {
            TargetPosition = transform.position;
            TargetRotation = transform.rotation;
        }

        void Update()
        {
            if (IsOwner) return;

            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * _lerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, Time.deltaTime * _lerpSpeed);
        }

        void OnDestroy()
        {
            if (_destroying) return;
            _destroying = true;

            if (NetworkManager.Instance != null && ViewID != -1)
                NetworkManager.Instance.UnregisterObject(ViewID, IsOwner);
        }
    }
}
