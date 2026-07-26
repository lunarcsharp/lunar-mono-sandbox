using MonoSandbox.Behaviours;
using MonoSandbox.Behaviours.Networking;
using UnityEngine;

public class RagdollManager : PlacementHandling
{
    public bool UseGorilla;
    public GameObject Gorilla, Body;

    public void Start()
    {
        Offset = 4.5f;
    }

    public override GameObject CursorRef
    {
        get
        {
            GameObject cursor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cursor.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            Destroy(cursor.GetComponent<Collider>());
            return cursor;
        }
    }

    public override void DrawCursor(RaycastHit hitInfo)
    {
        base.DrawCursor(hitInfo);

        Cursor.transform.position = hitInfo.point + Vector3.up * 0.15f;
    }

    public override void Activated(RaycastHit hitInfo)
    {
        base.Activated(hitInfo);

        if (UseGorilla)
        {
            if (Gorilla == null) return;
            GameObject Ragdoll = Instantiate(Gorilla);
            Ragdoll.name += "MonoObject_Ragdoll";
            Ragdoll.transform.SetParent(SandboxContainer.transform, false);

            foreach (Transform g in Ragdoll.transform.GetChild(1).GetComponentsInChildren<Transform>())
            {
                g.gameObject.layer = 8;
                g.name = string.Concat(g.name, "MonoObject");
            }

            Ragdoll.transform.position = hitInfo.point + new Vector3(0f, 0.45f, 0f);

            var skinnedMeshRenderer = Ragdoll.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                Material ragdollMaterial = new Material(skinnedMeshRenderer.material)
                {
                    color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))
                };
                Ragdoll.GetComponentInChildren<SkinnedMeshRenderer>().material = ragdollMaterial;
            }
            NetworkManager.RegisterSpawned("ragdoll_gorilla", Ragdoll);
        }
        else
        {
            if (Body == null) return;
            GameObject Ragdoll = Instantiate(Body);
            Ragdoll.name += "MonoObject_Ragdoll";
            Ragdoll.transform.SetParent(SandboxContainer.transform, false);

            foreach (Transform g in Ragdoll.transform.GetChild(0).GetComponentsInChildren<Transform>())
            {
                g.gameObject.layer = 8;
                g.name = string.Concat(g.name, "MonoObject");
            }

            Ragdoll.transform.position = hitInfo.point + new Vector3(0f, 0.6f, 0f);
            Ragdoll.transform.localScale = new Vector3(0.4f, 0.4f, 0.5f);
            Ragdoll.transform.GetChild(1).GetComponent<Renderer>().material.color = Color.grey;

            Destroy(Ragdoll.GetComponent<MeshCollider>());
            NetworkManager.RegisterSpawned("ragdoll_body", Ragdoll);
        }
    }
}
