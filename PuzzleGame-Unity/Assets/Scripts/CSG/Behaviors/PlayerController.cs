/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    PlayerController.cs
 * DESCRIPTION:  Top-down physics player controller driven by the Unity
 *               New Input System. The player pushes objects by moving
 *               into them.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

namespace CSG.Behaviors
{
    /// <summary>
    /// Moves the player's Rigidbody across the XZ plane from WASD input and
    /// turns the player to face the direction of travel.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Input magnitude below which the player is considered idle.
        /// </summary>
        private const float INPUT_DEADZONE = 0.01f;

        #endregion

        #region Fields & Properties

        [Header("Input")]
        [SerializeField]
        [Tooltip("Move action (Player/Move from InputSystem_Actions). A WASD fallback is created if empty.")]
        private InputActionReference _moveActionReference;

        [Header("Movement Settings")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Top movement speed in units per second.")]
        private float _moveSpeed = 6f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Maximum acceleration toward the target speed. Lower values feel heavier.")]
        private float _maxAcceleration = 40f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("How quickly the player turns to face the movement direction, in degrees per second.")]
        private float _turnSpeed = 720f;

        /// <summary>
        /// Cached Rigidbody used for physics movement.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// Action being read for movement (from the reference, or a fallback).
        /// </summary>
        private InputAction _moveAction;

        /// <summary>
        /// Latest movement input read this frame.
        /// </summary>
        private Vector2 _moveInput;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _moveAction = _moveActionReference != null ? _moveActionReference.action : CreateFallbackMoveAction();
        } //end Awake()

        // OnEnable turns on input when the player becomes active
        private void OnEnable()
        {
            _moveAction?.Enable();
        } //end OnEnable()

        #endregion

        #region Update Game Loop

        // Update reads input once per frame
        private void Update()
        {
            _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        } //end Update()

        #endregion

        #region Update Physics

        // FixedUpdate applies movement and rotation through physics
        private void FixedUpdate()
        {
            Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y);
            direction = Vector3.ClampMagnitude(direction, 1f);

            ApplyMovement(direction);
            ApplyRotation(direction);
        } //end FixedUpdate()

        #endregion

        #region Feature Logic

        #endregion

        #region Deactivation & Destruction

        // OnDisable stops reading input while inactive
        private void OnDisable()
        {
            // Only disable the fallback; the shared project action stays enabled for other users
            if (_moveActionReference == null) _moveAction?.Disable();
        } //end OnDisable()

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        /// <summary>
        /// Accelerates the Rigidbody toward the target horizontal velocity. Using a
        /// limited acceleration lets heavier objects resist being pushed.
        /// </summary>
        /// <param name="direction">Normalized movement direction on the XZ plane.</param>
        private void ApplyMovement(Vector3 direction)
        {
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            Vector3 targetVelocity = direction * _moveSpeed;

            Vector3 neededAcceleration = (targetVelocity - horizontalVelocity) / Time.fixedDeltaTime;
            neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, _maxAcceleration);

            _rigidbody.AddForce(neededAcceleration, ForceMode.Acceleration);
        } //end ApplyMovement(Vector3)

        /// <summary>
        /// Turns the player to face the direction they are moving.
        /// </summary>
        /// <param name="direction">Movement direction on the XZ plane.</param>
        private void ApplyRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude < INPUT_DEADZONE) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion nextRotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(nextRotation);
        } //end ApplyRotation(Vector3)

        /// <summary>
        /// Creates a WASD/arrow-key move action when no action reference is assigned.
        /// </summary>
        /// <returns>The fallback move action.</returns>
        private InputAction CreateFallbackMoveAction()
        {
            InputAction action = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            return action;
        } //end CreateFallbackMoveAction()

        #endregion

    } //end PlayerController
} //end namespace CSG.Behaviors
