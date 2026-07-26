using UnityEngine;

namespace MonoSandbox.Behaviours
{
    public class Enemy : MonoBehaviour
    {
        public float Health = 20, Defence = 2;
        private bool _isAttacking;

        public void Update()
        {
            var gorillaTagger = GorillaTagger.Instance;
            if (gorillaTagger == null) return;

            var headCollider = gorillaTagger.headCollider;
            if (headCollider == null) return;

            transform.LookAt(headCollider.transform.position);
            if (Vector3.Distance(transform.position, headCollider.transform.position) > 1.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, headCollider.transform.position, 4 * Time.deltaTime);
                _isAttacking = false;
            }
            else if (!_isAttacking)
            {
                _isAttacking = true;
                var audioSource = GetComponent<AudioSource>();
                if (audioSource != null) audioSource.Play();

                var player = GorillaLocomotion.GTPlayer.Instance;
                if (player != null)
                {
                    Rigidbody PlayerRigidbody = player.GetComponent<Rigidbody>();
                    if (PlayerRigidbody != null)
                    {
                        PlayerRigidbody.AddExplosionForce(1500f * 6f * Mathf.Sqrt(PlayerRigidbody.mass), transform.position, 7.5f + 6f / 1.25f);
                    }
                }
            }
            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void Damage(float damage, float criticalChance, float criticalMultiplier)
        {
            if (Random.Range(1, 100) < criticalChance)
            {
                Health = Mathf.Clamp(Health - (damage * criticalMultiplier + Random.Range(-2, 2)) / Defence, 0, Mathf.Infinity);
            }
            else
            {
                Health = Mathf.Clamp(Health - (damage + Random.Range(-4, 4)) / Defence, 0, Mathf.Infinity);
            }
        }
    }
}
