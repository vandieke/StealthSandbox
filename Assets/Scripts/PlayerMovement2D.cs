using UnityEngine;
using UnityEngine.InputSystem;

namespace StealthSandbox
{
    [RequireComponent(typeof(ActorMotor2D))]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        private ActorMotor2D motor;
        private InputAction movement;
        private void Awake()
        {
            motor = GetComponent<ActorMotor2D>();
            movement = new InputAction("Move", InputActionType.Value);
            movement.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            movement.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            movement.AddBinding("<Gamepad>/leftStick");
        }
        private void OnEnable() => movement.Enable();
        private void Update() => motor.SetMoveInput(movement.ReadValue<Vector2>());
        private void OnDisable() { movement.Disable(); motor.Stop(); }
        private void OnDestroy() => movement.Dispose();
    }
}
