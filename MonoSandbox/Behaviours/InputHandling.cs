using UnityEngine;

namespace MonoSandbox.Behaviours
{
    public class InputHandling : MonoBehaviour
    {
        public static float LeftTrigger, RightTrigger, LeftGrip, RightGrip;
        public static bool LeftPrimary, RightPrimary, LeftSecondary, RightSecondary;

        public void Update()
        {
            var instance = ControllerInputPoller.instance;
            if (instance == null) return;

            LeftTrigger = instance.leftControllerIndexFloat;
            LeftGrip = instance.leftControllerGripFloat;
            RightTrigger = instance.rightControllerIndexFloat;
            RightGrip = instance.rightControllerGripFloat;
            LeftPrimary = instance.leftControllerPrimaryButton;
            LeftSecondary = instance.leftControllerSecondaryButton;
            RightPrimary = instance.rightControllerPrimaryButton;
            RightSecondary = instance.rightControllerSecondaryButton;
        }
    }
}
