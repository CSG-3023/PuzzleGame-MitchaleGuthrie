/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    SlidingMover.cs
 * DESCRIPTION:  Reusable component that slides an object between a closed
 *               (starting) position and an open offset position.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;

namespace CSG.Puzzle.Components
{
    /// <summary>
    /// Smoothly moves an object between its starting position and an offset
    /// position. Uses a kinematic Rigidbody when present so physics stays stable.
    /// </summary>
    public class SlidingMover : MonoBehaviour
    {
        #region Constants

        #endregion

        #region Fields & Properties

        [Header("Movement Settings")]
        [SerializeField]
        [Tooltip("World-space offset from the starting position to the open position.")]
        private Vector3 _openOffset = new Vector3(0f, -2f, 0f);

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Movement speed in units per second.")]
        private float _speed = 3f;

        /// <summary>
        /// Optional kinematic Rigidbody used to move the object through physics.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// World position the object starts at (the closed position).
        /// </summary>
        private Vector3 _closedPosition;

        /// <summary>
        /// True when the mover is heading toward the open position.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// World position the object is currently moving toward.
        /// </summary>
        public Vector3 TargetPosition => IsOpen ? _closedPosition + _openOffset : _closedPosition;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _closedPosition = transform.position;
        } //end Awake()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        // FixedUpdate moves the object a step toward its target each physics tick
        private void FixedUpdate()
        {
            Vector3 current = _rigidbody != null ? _rigidbody.position : transform.position;
            if (current == TargetPosition) return;

            Vector3 next = Vector3.MoveTowards(current, TargetPosition, _speed * Time.fixedDeltaTime);

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(next);
            }
            else
            {
                transform.position = next;
            } //end if(_rigidbody != null)
        } //end FixedUpdate()

        #endregion

        #region Feature Logic

        /// <summary>
        /// Starts moving toward the open or closed position.
        /// </summary>
        /// <param name="isOpen">True to open, false to close.</param>
        public void SetOpen(bool isOpen)
        {
            IsOpen = isOpen;
        } //end SetOpen(bool)

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        // Shows where the object will move when opened
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = Application.isPlaying ? _closedPosition : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + _openOffset);
            Gizmos.DrawWireCube(origin + _openOffset, transform.lossyScale);
        } //end OnDrawGizmosSelected()

        #endregion

        #region Helpers

        #endregion

    } //end SlidingMover
} //end namespace CSG.Puzzle.Components
