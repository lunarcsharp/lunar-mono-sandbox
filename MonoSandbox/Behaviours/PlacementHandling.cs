using UnityEngine;

namespace MonoSandbox.Behaviours
{
    public class PlacementHandling : MonoBehaviour
    {
        public float Offset = 4f;
        public bool IsEditing, IsActivated, Placed;
        public GameObject Cursor, SandboxContainer;

        public virtual GameObject CursorRef { get; }

        public virtual void Activated(RaycastHit hitInfo)
        {
            HapticManager.Haptic(HapticManager.HapticType.Create);
        }

        public virtual void DrawCursor(RaycastHit hitInfo)
        {
        }

        private void Start()
        {
            SandboxContainer = RefCache.SandboxContainer;
        }

        private void Update()
        {
            if (!SandboxContainer)
            {
                SandboxContainer = RefCache.SandboxContainer;
            }

            if (IsEditing && !Cursor)
            {
                Cursor = CursorRef;
                if (Cursor != null)
                {
                    var renderer = Cursor.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = new Material(RefCache.Selection);
                    }
                }
            }
            else if (!IsEditing && Cursor)
            {
                Destroy(Cursor);
            }

            RaycastHit hitInfo = RefCache.Hit;
            if (IsEditing && Cursor)
            {
                if (Cursor.activeSelf != RefCache.HitExists) Cursor.SetActive(RefCache.HitExists);
                var renderer = Cursor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.392f, 0.722f, 0.820f, 0.4509804f);
                }

                if (RefCache.HitExists)
                    DrawCursor(hitInfo);

                IsActivated = InputHandling.RightPrimary;
                if (IsActivated && !Placed)
                {
                    Placed = true;
                    Activated(hitInfo);
                }
                else if (!IsActivated && Placed)
                {
                    Placed = false;
                }
            }
        }
    }
}
