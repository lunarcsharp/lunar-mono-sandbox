using UnityEngine;

namespace MonoSandbox.Behaviours.Networking
{
    public static class WeaponSpawner
    {
        private static readonly string[] _prefabNames = new string[]
        {
            "Pistol",       // 0 - Revolver
            "Shotgun",      // 1 - Shotgun
            "Cannon",       // 2 - Melon Cannon
            "SniperRifle",  // 3 - Sniper
            "LaserGun",     // 4 - Laser Gun
            "Banan",        // 5 - Banana Gun
            "ToolGun",      // 6 - Tool Gun
            "AssaultRifle", // 7 - Assault Rifle
        };

        private static readonly Vector3[] _localPositions = new Vector3[]
        {
            new Vector3(-0.02f, 0f, 0.035f),       // Revolver
            new Vector3(-0.02f, 0f, 0.035f),       // Shotgun
            new Vector3(-0.025f, 0.25f, -0.1f),    // Melon Cannon
            new Vector3(-0.02f, 0f, 0.035f),       // Sniper
            new Vector3(-0.02f, 0f, 0.035f),       // Laser Gun
            new Vector3(-0.04f, 0.085f, -0.055f),  // Banana Gun
            new Vector3(-0.03f, 0.02f, 0.035f),    // Tool Gun
            new Vector3(-0.02f, 0f, 0.035f),       // Assault Rifle
        };

        private static readonly Vector3[] _localRotations = new Vector3[]
        {
            new Vector3(0f, 90f, -90f),
            new Vector3(0f, 90f, -90f),
            new Vector3(0f, 90f, -90f),
            new Vector3(0f, 90f, -90f),
            new Vector3(0f, 90f, -90f),
            new Vector3(0f, 0f, 180f),
            new Vector3(0f, 90f, -90f),
            new Vector3(0f, 90f, -90f),
        };

        public static GameObject GetPrefab(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= _prefabNames.Length) return null;

            string name = _prefabNames[weaponIndex];
            if (ObjectSpawner._prefabs.TryGetValue(name, out GameObject prefab))
                return prefab;

            return null;
        }

        public static Vector3 GetLocalPosition(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= _localPositions.Length)
                return new Vector3(-0.02f, 0f, 0.035f);
            return _localPositions[weaponIndex];
        }

        public static Vector3 GetLocalRotation(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= _localRotations.Length)
                return new Vector3(0f, 90f, -90f);
            return _localRotations[weaponIndex];
        }
    }
}
