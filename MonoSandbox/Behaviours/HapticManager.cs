using UnityEngine;

namespace MonoSandbox.Behaviours
{
    public class HapticManager
    {
        public enum HapticType
        {
            Create, Use, Constant
        }

        public static void Haptic() => Haptic(HapticType.Create);

        public static void Haptic(HapticType hapticType)
        {
            var gorillaTagger = GorillaTagger.Instance;
            if (gorillaTagger == null) return;

            if (hapticType == HapticType.Constant)
            {
                gorillaTagger.StartVibration(false, gorillaTagger.tapHapticStrength / 10f, Time.deltaTime);
                return;
            }

            gorillaTagger.StartVibration(false, hapticType == HapticType.Create ? 0.1f : 0.5f, gorillaTagger.tapHapticDuration / 1.25f);
        }
    }
}
