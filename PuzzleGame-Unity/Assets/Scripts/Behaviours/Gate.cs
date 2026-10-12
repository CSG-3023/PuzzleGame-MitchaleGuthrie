/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    Gate.cs
 * DESCRIPTION:  Puzzle element that opens while all of its linked pressure
 *               plates are pressed, and closes again when any is released.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using System;
using CSG.Puzzle.Components;
using UnityEngine;

namespace CSG.Puzzle.Behaviours
{
    /// <summary>
    /// Opens a <see cref="SlidingMover"/> when every linked <see cref="PressurePlate"/>
    /// is pressed, showing progress with color and sound. Waits to close if
    /// something is standing in the doorway.
    /// </summary>
    [RequireComponent(typeof(SlidingMover))]
    public class Gate : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Shrinks the doorway check slightly so touching the frame does not count as blocking.
        /// </summary>
        private const float DOORWAY_CHECK_SCALE = 0.9f;

        #endregion

        #region Fields & Properties

        [Header("Requirements")]
        [SerializeField]
        [Tooltip("Every plate listed here must be pressed for the gate to open.")]
        private PressurePlate[] _requiredPlates;

        [Header("Feedback")]
        [SerializeField]
        [Tooltip("Tints the gate to show how many plates are pressed.")]
        private ColorFeedback _colorFeedback;

        [SerializeField]
        [Tooltip("Plays a tone when the gate opens or closes.")]
        private TonePlayer _tonePlayer;

        [SerializeField]
        [Tooltip("Tone when the gate opens (Hz).")]
        private float _openTone = 880f;

        [SerializeField]
        [Tooltip("Tone when the gate closes (Hz).")]
        private float _closeTone = 220f;

        /// <summary>
        /// Mover that physically slides the gate.
        /// </summary>
        private SlidingMover _mover;

        /// <summary>
        /// World-space center of the closed doorway.
        /// </summary>
        private Vector3 _doorwayCenter;

        /// <summary>
        /// Half-size of the closed doorway, used for the blocking check.
        /// </summary>
        private Vector3 _doorwayHalfExtents;

        /// <summary>
        /// True while the gate should close but is waiting for the doorway to clear.
        /// </summary>
        private bool _isClosePending;

        /// <summary>
        /// True while the gate is open (or opening).
        /// </summary>
        public bool IsOpen => _mover != null && _mover.IsOpen;

        /// <summary>
        /// Raised when the gate opens or closes.
        /// </summary>
        public event Action<bool> OpenChanged;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _mover = GetComponent<SlidingMover>();

            // Record the closed doorway volume before the gate ever moves
            if (TryGetComponent(out BoxCollider box))
            {
                _doorwayCenter = transform.TransformPoint(box.center);
                _doorwayHalfExtents = Vector3.Scale(box.size, transform.lossyScale) * (0.5f * DOORWAY_CHECK_SCALE);
            } //end if(has BoxCollider)
        } //end Awake()

        // OnEnable subscribes to plate events
        private void OnEnable()
        {
            foreach (PressurePlate plate in _requiredPlates)
            {
                if (plate != null) plate.PressedChanged += HandlePlateChanged;
            } //end foreach
        } //end OnEnable()

        // Start shows the initial state once all plates exist
        private void Start()
        {
            UpdateProgressColor();
        } //end Start()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        // FixedUpdate finishes a delayed close once the doorway is clear
        private void FixedUpdate()
        {
            if (_isClosePending && !IsDoorwayBlocked())
            {
                _isClosePending = false;
                SetOpen(false);
            } //end if(close pending and clear)
        } //end FixedUpdate()

        #endregion

        #region Feature Logic

        #endregion

        #region Deactivation & Destruction

        // OnDisable unsubscribes from plate events
        private void OnDisable()
        {
            foreach (PressurePlate plate in _requiredPlates)
            {
                if (plate != null) plate.PressedChanged -= HandlePlateChanged;
            } //end foreach
        } //end OnDisable()

        #endregion

        #region Gizmos

        // Draws lines to linked plates in the Scene view
        private void OnDrawGizmosSelected()
        {
            if (_requiredPlates == null) return;

            Gizmos.color = Color.yellow;
            foreach (PressurePlate plate in _requiredPlates)
            {
                if (plate != null) Gizmos.DrawLine(transform.position, plate.transform.position);
            } //end foreach
        } //end OnDrawGizmosSelected()

        #endregion

        #region Helpers

        /// <summary>
        /// Re-evaluates the gate whenever any linked plate changes.
        /// </summary>
        /// <param name="plate">The plate that changed.</param>
        /// <param name="isPressed">The plate's new state.</param>
        private void HandlePlateChanged(PressurePlate plate, bool isPressed)
        {
            UpdateProgressColor();

            if (AreAllPlatesPressed())
            {
                _isClosePending = false;
                SetOpen(true);
            }
            else if (IsOpen)
            {
                // Closing is handled in FixedUpdate so nothing gets crushed in the doorway
                _isClosePending = true;
            } //end if(all pressed)
        } //end HandlePlateChanged(PressurePlate, bool)

        /// <summary>
        /// Opens or closes the gate and plays feedback when the state changes.
        /// </summary>
        /// <param name="isOpen">True to open.</param>
        private void SetOpen(bool isOpen)
        {
            if (IsOpen == isOpen) return;

            _mover.SetOpen(isOpen);

            if (_tonePlayer != null)
            {
                _tonePlayer.PlayTone(isOpen ? _openTone : _closeTone);
            } //end if(_tonePlayer != null)

            OpenChanged?.Invoke(isOpen);
        } //end SetOpen(bool)

        /// <summary>
        /// Checks whether every linked plate is pressed.
        /// </summary>
        /// <returns>True if all plates are pressed.</returns>
        private bool AreAllPlatesPressed()
        {
            return CountPressedPlates() == _requiredPlates.Length;
        } //end AreAllPlatesPressed()

        /// <summary>
        /// Counts how many linked plates are currently pressed.
        /// </summary>
        /// <returns>The number of pressed plates.</returns>
        private int CountPressedPlates()
        {
            int count = 0;
            foreach (PressurePlate plate in _requiredPlates)
            {
                if (plate != null && plate.IsPressed) count++;
            } //end foreach
            return count;
        } //end CountPressedPlates()

        /// <summary>
        /// Tints the gate by the fraction of plates pressed.
        /// </summary>
        private void UpdateProgressColor()
        {
            if (_colorFeedback == null || _requiredPlates.Length == 0) return;

            _colorFeedback.SetProgress(CountPressedPlates() / (float)_requiredPlates.Length);
        } //end UpdateProgressColor()

        /// <summary>
        /// Checks whether a non-kinematic Rigidbody is standing in the doorway.
        /// </summary>
        /// <returns>True if the doorway is blocked.</returns>
        private bool IsDoorwayBlocked()
        {
            Collider[] hits = Physics.OverlapBox(_doorwayCenter, _doorwayHalfExtents, transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                Rigidbody body = hit.attachedRigidbody;
                if (body != null && !body.isKinematic) return true;
            } //end foreach
            return false;
        } //end IsDoorwayBlocked()

        #endregion

    } //end Gate
} //end namespace CSG.Puzzle.Behaviours
