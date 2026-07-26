using MonoSandbox;
using MonoSandbox.Behaviours;
using MonoSandbox.Behaviours.Networking;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterManager : MonoBehaviour
{
    public List<GameObject> objectList = new List<GameObject>();

    public bool primaryDown, canPlace, editMode;
    public float multiplier = 4f;

    public GameObject Cursor, itemsFolder, ThrusterModel, ThrustParticles;

    public void Start()
    {
        itemsFolder = gameObject;
    }

    public void Update()
    {
        RaycastHit hitInfo = RefCache.Hit;

        if (Cursor != null)
        {
            bool isAllowed = hitInfo.collider != null && hitInfo.collider.attachedRigidbody != null && hitInfo.transform != null && hitInfo.transform.gameObject.name.Contains("MonoObject");
            Cursor.GetComponent<Renderer>().material.color = isAllowed ? new Color(0.392f, 0.722f, 0.820f, 0.4509804f) : new Color(0.8314f, 0.2471f, 0.1569f, 0.4509804f);

            Cursor.transform.position = hitInfo.point;
            Cursor.transform.forward = -hitInfo.normal;
            primaryDown = InputHandling.RightPrimary;
            if (primaryDown)
            {
                if (canPlace && isAllowed)
                {
                    if (ThrusterModel == null) return;
                    GameObject Thruster = Instantiate(ThrusterModel);
                    Thruster.transform.localScale = new Vector3(10f, 10f, 10f);
                    Thruster.transform.SetParent(hitInfo.collider.transform, true);
                    Thruster.transform.position = hitInfo.point;
                    Thruster.name = "Thruster MonoObject";
                    objectList.Add(Thruster);
                    ThrusterControls control = Thruster.AddComponent<ThrusterControls>();
                    control.rb = hitInfo.collider.attachedRigidbody;
                    control.multiplier = multiplier;
                    if (ThrustParticles != null)
                    {
                        control.particle = Instantiate(ThrustParticles);
                    }
                    Thruster.GetComponent<Renderer>().material.color = Color.black;
                    Thruster.transform.forward = -hitInfo.normal;
                    NetworkManager.RegisterSpawned("thruster", Thruster);

                    HapticManager.Haptic(HapticManager.HapticType.Create);
                    canPlace = false;
                }
            }
            else
            {
                canPlace = true;
            }
        }
        else
        {
            if (editMode)
            {
                if (ThrusterModel == null) return;
                Cursor = Instantiate(ThrusterModel);
                Cursor.transform.localScale = new Vector3(10f, 10f, 10f);
                Cursor.GetComponent<Renderer>().material = new Material(RefCache.Selection);
            }
            else { if (Cursor != null) { Destroy(Cursor.gameObject); } }
        }
        if (!editMode) { if (Cursor != null) { Destroy(Cursor.gameObject); } }
    }
}

public class ThrusterControls : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject particle;
    float gripDown;
    public float multiplier = 4;

    void Start()
    {
        if (particle != null)
        {
            particle.transform.SetParent(transform, false);
            particle.transform.localEulerAngles = new Vector3(180, 0, 0);
            particle.transform.localPosition = new Vector3(0, 0, -0.014f);
            particle.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    void Update()
    {
        gripDown = InputHandling.RightGrip;
        if (gripDown > 0.3f)
        {
            if (particle != null)
            {
                var audioSource = particle.GetComponent<AudioSource>();
                var particleSystem = particle.GetComponent<ParticleSystem>();
                if (audioSource != null && particleSystem != null)
                {
                    if (!audioSource.isPlaying)
                    {
                        particleSystem.Play(true);
                        audioSource.Play();
                    }
                }
            }

            HapticManager.Haptic(HapticManager.HapticType.Constant);
            if (rb != null)
            {
                rb.AddForceAtPosition(transform.forward * 10 * multiplier, transform.position);
            }
        }
        else
        {
            if (particle != null)
            {
                var audioSource = particle.GetComponent<AudioSource>();
                var particleSystem = particle.GetComponent<ParticleSystem>();
                if (audioSource != null && particleSystem != null)
                {
                    if (particleSystem.isPlaying)
                    {
                        particleSystem.Stop(true);
                        audioSource.Stop();
                    }
                }
            }
        }
    }
}
