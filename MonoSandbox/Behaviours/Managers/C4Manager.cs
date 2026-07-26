using MonoSandbox;
using MonoSandbox.Behaviours;
using MonoSandbox.Behaviours.Networking;
using System.Threading.Tasks;
using UnityEngine;

public class C4Manager : MonoBehaviour
{
    public bool primaryDown, canPlace, editMode, IsMine;
    public float multiplier = 4;

    public GameObject Explosive, C4Model, Mine, ExplodeModel, itemsFolder = null;

    private bool _isMine;

    public void Start()
    {
        itemsFolder = gameObject;
    }

    public void DrawCursor()
    {
        if (Explosive != null) Destroy(Explosive);
        Explosive = Instantiate(IsMine ? Mine : C4Model);
        Explosive.transform.localScale = IsMine ? Vector3.one * 1f : Vector3.one * 1.4f;

        Explosive.GetComponent<Renderer>().material = new Material(RefCache.Selection);
        Destroy(Explosive.GetComponent<Collider>());
    }

    public void Update()
    {
        RaycastHit hitInfo = RefCache.Hit;

        if (Explosive != null)
        {
            if (IsMine != _isMine)
            {
                _isMine = IsMine;
                DrawCursor();
            }

            Explosive.transform.position = hitInfo.point;
            if (RefCache.HitExists && IsMine) Explosive.transform.up = hitInfo.normal;
            else if (RefCache.HitExists && !IsMine) Explosive.transform.forward = hitInfo.normal;

            Explosive.GetComponent<Renderer>().material.color = new Color(0.392f, 0.722f, 0.820f, 0.4509804f);

            primaryDown = InputHandling.RightPrimary;
            if (primaryDown && RefCache.HitExists)
            {
                if (canPlace && !IsMine)
                {
                    GameObject C4 = Instantiate(C4Model);
                    Destroy(C4.GetComponent<MeshCollider>());
                    C4.AddComponent<BoxCollider>();
                    C4.transform.SetParent(itemsFolder.transform, false);
                    C4.transform.position = hitInfo.point;
                    C4.transform.forward = hitInfo.normal;
                    C4.transform.localScale = Vector3.one * 1.4f;

                    BombDetonate bombDet = C4.AddComponent<BombDetonate>();
                    bombDet.ExplosionOBJ = ExplodeModel;
                    bombDet.multiplier = multiplier;
                    NetworkManager.RegisterSpawned("c4", C4);
                }
                else if (canPlace && IsMine)
                {
                    GameObject C4 = Instantiate(Mine);
                    C4.transform.SetParent(itemsFolder.transform, false);
                    C4.transform.position = hitInfo.point;
                    C4.transform.up = hitInfo.normal;
                    C4.transform.localScale = Vector3.one;

                    MineDetonate mineDet = C4.AddComponent<MineDetonate>();
                    mineDet.Explosion = ExplodeModel;
                    mineDet.Multiplier = multiplier;
                    NetworkManager.RegisterSpawned("mine", C4);
                }

                HapticManager.Haptic(HapticManager.HapticType.Create);
                canPlace = false;
            }
            else if (RefCache.HitExists)
            {
                canPlace = true;
            }
        }
        else
        {
            if (editMode)
            {
                DrawCursor();
            }
            else if (Explosive != null)
            {
                Destroy(Explosive.gameObject);
            }
        }
        if (!editMode && Explosive)
        {
            Destroy(Explosive.gameObject);
        }
    }
}

public class MineDetonate : MonoBehaviour
{
    public float Multiplier = 4, Radius = 10f;
    public GameObject Explosion;

    private bool _canExplode = true;

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null && collision.collider.attachedRigidbody != null)
        {
            Explode();
        }
    }

    public async void Explode()
    {
        if (!_canExplode) return;
        _canExplode = false;

        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.Play();
        await Task.Delay(Mathf.RoundToInt(audioSource != null ? audioSource.clip.length * 750f : 1000));

        if (audioSource != null) audioSource.Play();
        var renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null) boxCollider.enabled = false;

        if (Explosion != null)
        {
            GameObject ExplodeOBJ = Instantiate(Explosion);
            ExplodeOBJ.transform.SetParent(transform);
            ExplodeOBJ.transform.localPosition = Vector3.zero;
            ExplodeOBJ.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius);
        foreach (Collider nearyby in colliders)
        {
            Rigidbody rig = nearyby.GetComponent<Rigidbody>();
            rig?.AddExplosionForce(2500f * Multiplier, transform.position, 8f);
        }

        Collider[] bombs = Physics.OverlapSphere(transform.position, Radius);
        foreach (Collider nearyby in bombs)
        {
            nearyby.GetComponent<BombDetonate>()?.Explode();
            nearyby.GetComponent<MineDetonate>()?.Explode();
            nearyby.GetComponent<Explode>()?.ExplodeObject();
        }

        var player = GorillaLocomotion.GTPlayer.Instance;
        if (player != null)
        {
            Rigidbody PlayerRigidbody = player.GetComponent<Rigidbody>();
            if (PlayerRigidbody != null)
            {
                PlayerRigidbody.AddExplosionForce(2500f * Multiplier * Mathf.Sqrt(PlayerRigidbody.mass), transform.position, 5 + (0.75f * Multiplier));
            }
        }

        Destroy(gameObject, 3f);
    }
}

public class BombDetonate : MonoBehaviour
{
    bool secButtonDown;
    bool primaryDown;
    bool canExplode = true;
    public bool useDefault = true;
    public float multiplier = 4;
    public float Radius = 10f;
    public GameObject ExplosionOBJ;

    void Update()
    {
        secButtonDown = InputHandling.RightSecondary;
        primaryDown = InputHandling.RightPrimary;
        if (secButtonDown)
        {
            if (canExplode)
            {
                Explode();
            }
        }
    }

    public void Explode()
    {
        if (canExplode)
        {
            canExplode = false;
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null) audioSource.Play();
            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null) boxCollider.enabled = false;

            if (ExplosionOBJ != null)
            {
                GameObject ExplodeOBJ = Instantiate(ExplosionOBJ);
                ExplodeOBJ.transform.SetParent(transform);
                ExplodeOBJ.transform.localPosition = Vector3.zero;
                ExplodeOBJ.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, Radius);
            foreach (Collider nearyby in colliders)
            {
                Rigidbody rig = nearyby.GetComponent<Rigidbody>();
                rig?.AddExplosionForce(1500f * multiplier, transform.position, 8f);
            }
            Collider[] bombs = Physics.OverlapSphere(transform.position, Radius);
            foreach (Collider nearyby in bombs)
            {
                nearyby.GetComponent<BombDetonate>()?.Explode();
                nearyby.GetComponent<MineDetonate>()?.Explode();
                nearyby.GetComponent<Explode>()?.ExplodeObject();
            }

            var player = GorillaLocomotion.GTPlayer.Instance;
            if (player != null)
            {
                Rigidbody PlayerRigidbody = player.GetComponent<Rigidbody>();
                if (PlayerRigidbody != null)
                {
                    PlayerRigidbody.AddExplosionForce(1500f * multiplier * Mathf.Sqrt(PlayerRigidbody.mass), transform.position, 5 + (0.75f * multiplier));
                }
            }
            Invoke(nameof(Delete), 3);
        }
    }

    void Delete()
    {
        Destroy(gameObject);
    }
}
