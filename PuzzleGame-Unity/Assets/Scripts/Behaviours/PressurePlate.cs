/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    PressurePlate.cs
 * DESCRIPTION:  Puzzle element that coordinates a WeightSensor with color,
 *               sound, and a weight label so players can see and hear
 *               how much weight a plate needs.
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
    /// A floor plate that is pressed when enough weight rests on it. Translates
    /// sensor readings into player feedback and notifies listeners such as gates.
    /// </summary>
    [RequireComponent(typeof(WeightSensor))]
    public class PressurePlate : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Format for the "current / required" weight label.
        /// </summary>
        private const string LABEL_FORMAT = "{0:0.#} / {1:0.#}";

        #endregion

        #region Fields & Properties

        [Header("Feedback")]
        [SerializeField]
        [Tooltip("Tints the plate red / yellow / green based on weight.")]
        private ColorFeedback _colorFeedback;

        [SerializeField]
        [Tooltip("Plays tones when weight changes and when the plate is pressed.")]
        private TonePlayer _tonePlayer;

        [SerializeField]
        [Tooltip("World-space label showing current and required weight.")]
        private TextMesh _weightLabel;

        [Header("Tones (Hz)")]
        [SerializeField]
        [Tooltip("Tone when weight is added but is not yet enough.")]
        private float _partialTone = 440f;

        [SerializeField]
        [Tooltip("Tone when the plate becomes pressed.")]
        private float _pressedTone = 660f;

        [SerializeField]
        [Tooltip("Tone when the plate is released.")]
        private float _releasedTone = 300f;

        /// <summary>
        /// Sensor that measures weight on the plate.
        /// </summary>
        private WeightSensor _sensor;

        /// <summary>
        /// Weight from the previous reading, used to detect weight being added.
        /// </summary>
        private float _previousWeight;

        /// <summary>
        /// True while enough weight is on the plate.
        /// </summary>
        public bool IsPressed => _sensor != null && _sensor.IsActivated;

        /// <summary>
        /// Raised when the plate is pressed or released. Passes this plate and its pressed state.
        /// </summary>
        public event Action<PressurePlate, bool> PressedChanged;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _sensor = GetComponent<WeightSensor>();
        } //end Awake()

        // OnEnable subscribes to sensor events
        private void OnEnable()
        {
            _sensor.OnWeightChanged.AddListener(HandleWeightChanged);
            _sensor.OnActivationChanged.AddListener(HandleActivationChanged);
        } //end OnEnable()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        #endregion

        #region Deactivation & Destruction

        // OnDisable unsubscribes from sensor events
        private void OnDisable()
        {
            _sensor.OnWeightChanged.RemoveListener(HandleWeightChanged);
            _sensor.OnActivationChanged.RemoveListener(HandleActivationChanged);
        } //end OnDisable()

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        /// <summary>
        /// Updates the label and color, and plays a hint tone when weight is added
        /// but is still not enough.
        /// </summary>
        /// <param name="current">Current weight on the plate.</param>
        /// <param name="required">Weight required to press the plate.</param>
        private void HandleWeightChanged(float current, float required)
        {
            if (_weightLabel != null)
            {
                _weightLabel.text = string.Format(LABEL_FORMAT, current, required);
            } //end if(_weightLabel != null)

            if (_colorFeedback != null)
            {
                _colorFeedback.SetProgress(current / required);
            } //end if(_colorFeedback != null)

            // "Almost there" feedback: weight went up but the plate is not pressed yet
            bool isWeightAdded = current > _previousWeight;
            if (isWeightAdded && current < required && _tonePlayer != null)
            {
                _tonePlayer.PlayTone(_partialTone);
            } //end if(partial weight added)

            _previousWeight = current;
        } //end HandleWeightChanged(float, float)

        /// <summary>
        /// Plays the pressed or released tone and notifies listeners.
        /// </summary>
        /// <param name="isPressed">True if the plate is now pressed.</param>
        private void HandleActivationChanged(bool isPressed)
        {
            if (_tonePlayer != null)
            {
                _tonePlayer.PlayTone(isPressed ? _pressedTone : _releasedTone);
            } //end if(_tonePlayer != null)

            PressedChanged?.Invoke(this, isPressed);
        } //end HandleActivationChanged(bool)

        #endregion

    } //end PressurePlate
} //end namespace CSG.Puzzle.Behaviours
