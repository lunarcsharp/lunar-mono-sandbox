using System.Collections.Generic;
using UnityEngine;

namespace MonoSandbox.Behaviours.Networking
{
    public static class ObjectSpawner
    {
        internal static readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

        public static void RegisterPrefab(string key, GameObject prefab)
        {
            if (prefab != null)
                _prefabs[key] = prefab;
        }

        public static GameObject Create(string objectType, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject obj = null;

            switch (objectType)
            {
                case "box":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    AddRigidbody(obj, 2.5f);
                    AddUserCollision(obj, PrimitiveType.Cube);
                    break;

                case "plane":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.localScale = new Vector3(0.6f, 0.6f, 0.1f);
                    AddRigidbody(obj, 2.5f);
                    AddUserCollision(obj, PrimitiveType.Cube);
                    break;

                case "sphere":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    AddRigidbody(obj, 3.5f);
                    AddUserCollision(obj, PrimitiveType.Sphere);
                    break;

                case "bean":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    AddRigidbody(obj, 3.5f);
                    AddUserCollision(obj, PrimitiveType.Capsule);
                    break;

                case "wheel":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    obj.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);
                    AddRigidbody(obj, 3.5f);
                    AddUserCollision(obj, PrimitiveType.Cube);
                    break;

                case "barrel":
                    if (!_prefabs.TryGetValue("Barrel", out GameObject barrelPrefab)) return null;
                    obj = Object.Instantiate(barrelPrefab);
                    obj.transform.localScale = new Vector3(15f, 15f, 15f);
                    var barrelCol = obj.AddComponent<BoxCollider>();
                    barrelCol.size = new Vector3(0.025f, 0.025f, 0.025f);
                    AddRigidbody(obj, 3.5f);
                    var explode = obj.AddComponent<Explode>();
                    explode.Multiplier = 4f;
                    break;

                case "crate":
                    if (!_prefabs.TryGetValue("Crate", out GameObject cratePrefab)) return null;
                    obj = Object.Instantiate(cratePrefab);
                    AddRigidbody(obj, 2.5f);
                    obj.AddComponent<BoxCollider>();
                    AddUserCollisionBox(obj);
                    break;

                case "couch":
                    if (!_prefabs.TryGetValue("Couch", out GameObject couchPrefab)) return null;
                    obj = Object.Instantiate(couchPrefab);
                    obj.transform.localScale = new Vector3(100f, 100f, 100f);
                    AddRigidbody(obj, 8f);
                    obj.AddComponent<BoxCollider>();
                    break;

                case "bath":
                    if (!_prefabs.TryGetValue("Bath", out GameObject bathPrefab)) return null;
                    obj = Object.Instantiate(bathPrefab);
                    obj.transform.localScale = new Vector3(20f, 20f, 20f);
                    AddRigidbody(obj, 8f);
                    break;

                case "softbody":
                    if (!_prefabs.TryGetValue("BoneSphere", out GameObject softPrefab)) return null;
                    obj = Object.Instantiate(softPrefab);
                    obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    AddRigidbody(obj, 3.5f);
                    obj.AddComponent<BoneSphere>();
                    break;

                case "entity":
                    if (!_prefabs.TryGetValue("Demon", out GameObject entityPrefab)) return null;
                    obj = Object.Instantiate(entityPrefab);
                    obj.AddComponent<SphereCollider>();
                    var enemy = obj.AddComponent<Enemy>();
                    enemy.Health = 40f;
                    enemy.Defence = 1.75f;
                    break;

                case "c4":
                    if (!_prefabs.TryGetValue("C4_Weapon", out GameObject c4Prefab)) return null;
                    obj = Object.Instantiate(c4Prefab);
                    Object.Destroy(obj.GetComponent<MeshCollider>());
                    obj.AddComponent<BoxCollider>();
                    obj.transform.localScale = Vector3.one * 1.4f;
                    var bomb = obj.AddComponent<BombDetonate>();
                    bomb.multiplier = 4f;
                    break;

                case "mine":
                    if (!_prefabs.TryGetValue("Mine_02", out GameObject minePrefab)) return null;
                    obj = Object.Instantiate(minePrefab);
                    obj.transform.localScale = Vector3.one;
                    var mineDet = obj.AddComponent<MineDetonate>();
                    mineDet.Multiplier = 4f;
                    break;

                case "thruster":
                    if (!_prefabs.TryGetValue("Thruster 1", out GameObject thrusterPrefab)) return null;
                    obj = Object.Instantiate(thrusterPrefab);
                    obj.transform.localScale = new Vector3(10f, 10f, 10f);
                    break;

                case "balloon":
                    if (!_prefabs.TryGetValue("Balloon", out GameObject balloonPrefab)) return null;
                    obj = Object.Instantiate(balloonPrefab);
                    obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    break;

                case "ragdoll_body":
                    if (!_prefabs.TryGetValue("Body", out GameObject bodyPrefab)) return null;
                    obj = Object.Instantiate(bodyPrefab);
                    obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.5f);
                    foreach (Transform child in obj.transform)
                        child.gameObject.layer = 8;
                    break;

                case "ragdoll_gorilla":
                    if (!_prefabs.TryGetValue("GorillaBody", out GameObject gorillaPrefab)) return null;
                    obj = Object.Instantiate(gorillaPrefab);
                    foreach (Transform child in obj.transform)
                    {
                        child.gameObject.layer = 8;
                    }
                    break;

                default:
                    return null;
            }

            if (obj != null)
            {
                obj.layer = 8;
                if (!obj.name.Contains("MonoObject"))
                    obj.name += "MonoObject";

                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.transform.localScale = scale;

                if (obj.GetComponent<Renderer>() != null && RefCache.Default != null)
                    obj.GetComponent<Renderer>().material = RefCache.Default;

                if (RefCache.SandboxContainer != null)
                    obj.transform.SetParent(RefCache.SandboxContainer.transform, false);
            }

            return obj;
        }

        static void AddRigidbody(GameObject obj, float mass)
        {
            var rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = mass;
        }

        static void AddUserCollision(GameObject obj, PrimitiveType colType)
        {
            var userCollision = new GameObject();
            switch (colType)
            {
                case PrimitiveType.Cube:
                    userCollision.AddComponent<BoxCollider>();
                    break;
                case PrimitiveType.Sphere:
                    userCollision.AddComponent<SphereCollider>();
                    break;
                case PrimitiveType.Capsule:
                    var cap = userCollision.AddComponent<CapsuleCollider>();
                    cap.height = 2;
                    break;
            }
            userCollision.layer = 0;
            userCollision.transform.SetParent(obj.transform, false);
        }

        static void AddUserCollisionBox(GameObject obj)
        {
            var userCollision = new GameObject();
            userCollision.AddComponent<BoxCollider>();
            userCollision.layer = 0;
            userCollision.transform.SetParent(obj.transform, false);
        }
    }
}
