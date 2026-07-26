using UnityEngine;

namespace MonoSandbox.Behaviours
{
    public class SineGunAnimation : MonoBehaviour
    {
        public float Efficiency = 1.2f, Speed = 12f;
        public bool UseX;

        private bool _playing;
        private Vector3 _origin;
        private float _time, _scale;

        public void Start()
        {
            _origin = transform.localRotation.eulerAngles;
        }

        public void Play()
        {
            _playing = true;
            _time = 0f;
        }

        public void Update()
        {
            if (_playing)
            {
                _time += Time.deltaTime * Speed / 1.1f;
                _scale = Mathf.Sin(_time);

                if (UseX)
                {
                    transform.localEulerAngles = new Vector3(_origin.x - Speed * Mathf.Abs(1f - _scale), _origin.y, _origin.z);
                }
                else
                {
                    transform.localEulerAngles = new Vector3(_origin.x, _origin.y, _origin.z - Speed * Mathf.Abs(1f - _scale));
                }

                if (_time >= Mathf.PI / 2f)
                {
                    transform.localEulerAngles = _origin;
                    _playing = false;
                }
            }
        }
    }
}
