/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    TopDownCamera.cs
 * DESCRIPTION:  Top-down follow camera that smoothly tracks a target and
 *               lets the player look ahead by moving the mouse.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

namespace CSG.Puzzle.Player
{
    /// <summary>
    /// Follows a target from a fixed top-down angle. Moving the mouse toward a
    /// screen edge shifts the view in that direction so players can scout the room.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Converts a 0..1 screen position into a -1..1 offset from screen center.
        /// </summary>
        private const float SCREEN_TO_CENTERED = 2f;

        #endregion

        #region Fields & Properties

        [Header("Target")]
        [SerializeField]
        [Tooltip("The transform the camera follows (usually the player).")]
        private Transform _target;

        [Header("Framing")]
        [SerializeField]
        [Tooltip("Camera position relative to the target.")]
        private Vector3 _offset = new Vector3(0f, 14f, -10f);

        [SerializeField]
        [Min(0f)]
        [Tooltip("Approximate time in seconds for the camera to catch up to its goal.")]
        private float _smoothTime = 0.2f;

        [Header("Mouse Look-Ahead")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("How far the view can shift toward the mouse, in world units.")]
        private float _lookAheadDistance = 4f;

        /// <summary>
        /// Current velocity used by SmoothDamp.
        /// </summary>
        private Vector3 _velocity;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Start snaps to the target so the first frame is already framed correctly
        private void Start()
        {
            if (_target != null)
            {
                transform.position = _target.position + _offset;
                transform.LookAt(_target.position);
            } //end if(_target != null)
        } //end Start()

        #endregion

        #region Update Game Loop

        // LateUpdate runs after movement so the camera never lags a frame behind
        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 goal = _target.position + _offset + GetLookAheadOffset();
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
        } //end LateUpdate()

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        /// <summary>
        /// Sets a new transform for the camera to follow.
        /// </summary>
        /// <param name="target">The transform to follow.</param>
        public void SetTarget(Transform target)
        {
            _target = target;
        } //end SetTarget(Transform)

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        /// <summary>
        /// Converts the mouse position into a world-space XZ offset.
        /// </summary>
        /// <returns>The look-ahead offset, or zero when no mouse is available.</returns>
        private Vector3 GetLookAheadOffset()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || Screen.width == 0 || Screen.height == 0) return Vector3.zero;

            Vector2 mousePosition = mouse.position.ReadValue();

            // -1..1 on each axis, centered on the screen
            float x = (mousePosition.x / Screen.width) * SCREEN_TO_CENTERED - 1f;
            float y = (mousePosition.y / Screen.height) * SCREEN_TO_CENTERED - 1f;
            Vector2 centered = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

            return new Vector3(centered.x, 0f, centered.y) * _lookAheadDistance;
        } //end GetLookAheadOffset()

        #endregion

    } //end TopDownCamera
} //end namespace CSG.Puzzle.Player
