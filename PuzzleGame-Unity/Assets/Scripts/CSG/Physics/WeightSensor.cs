/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    WeightSensor.cs
 * DESCRIPTION:  Reusable trigger component that totals the mass of every
 *               Rigidbody inside its volume and reports when a required
 *               weight has been reached.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CSG.Physics
{
    /// <summary>
    /// Measures the combined Rigidbody mass resting inside a trigger volume and
    /// raises events when that weight changes or crosses the required weight.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeightSensor : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Small tolerance so floating-point mass totals still count as "enough".
        /// </summary>
        private const float WEIGHT_TOLERANCE = 0.01f;

        #endregion

        #region Fields & Properties

        [Header("Weight Settings")]
        [SerializeField]
        [Min(0.1f)]
        [Tooltip("Total Rigidbody mass needed inside the trigger for the sensor to activate.")]
        private float _requiredWeight = 1f;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Raised whenever the total weight changes. Passes (current weight, required weight).")]
        private UnityEvent<float, float> _onWeightChanged = new UnityEvent<float, float>();

        [SerializeField]
        [Tooltip("Raised when the sensor switches between activated and deactivated.")]
        private UnityEvent<bool> _onActivationChanged = new UnityEvent<bool>();

        /// <summary>
        /// Number of colliders each Rigidbody currently has inside the trigger,
        /// so compound objects are only counted once.
        /// </summary>
        private readonly Dictionary<Rigidbody, int> _colliderCounts = new Dictionary<Rigidbody, int>();

        /// <summary>
        /// Total Rigidbody mass needed for activation.
        /// </summary>
        public float RequiredWeight => _requiredWeight;

        /// <summary>
        /// Total Rigidbody mass currently inside the trigger.
        /// </summary>
        public float CurrentWeight { get; private set; }

        /// <summary>
        /// True while the current weight meets or exceeds the required weight.
        /// </summary>
        public bool IsActivated { get; private set; }

        /// <summary>
        /// Event raised when the weight changes. Passes (current weight, required weight).
        /// </summary>
        public UnityEvent<float, float> OnWeightChanged => _onWeightChanged;

        /// <summary>
        /// Event raised when the activation state changes.
        /// </summary>
        public UnityEvent<bool> OnActivationChanged => _onActivationChanged;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            // The sensor only works as a trigger volume
            GetComponent<Collider>().isTrigger = true;
        } //end Awake()

        // Start is called once before the first Update, after all Awake calls; set up logic that depends on other objects here
        private void Start()
        {
            // Broadcast the starting state so listeners (labels, colors) are initialized
            _onWeightChanged.Invoke(CurrentWeight, _requiredWeight);
        } //end Start()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        // OnTriggerEnter is called when a collider enters the sensor volume
        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null) return;

            _colliderCounts.TryGetValue(body, out int count);
            _colliderCounts[body] = count + 1;

            // Only a newly arrived body changes the total weight
            if (count == 0)
            {
                RecalculateWeight();
            } //end if(count == 0)
        } //end OnTriggerEnter()

        // OnTriggerExit is called when a collider leaves the sensor volume
        private void OnTriggerExit(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || !_colliderCounts.TryGetValue(body, out int count)) return;

            if (count <= 1)
            {
                _colliderCounts.Remove(body);
                RecalculateWeight();
            }
            else
            {
                _colliderCounts[body] = count - 1;
            } //end if(count <= 1)
        } //end OnTriggerExit()

        #endregion

        #region Feature Logic

        /// <summary>
        /// Changes the weight required for activation and re-evaluates the sensor.
        /// </summary>
        /// <param name="requiredWeight">The new total mass required.</param>
        public void SetRequiredWeight(float requiredWeight)
        {
            _requiredWeight = Mathf.Max(WEIGHT_TOLERANCE, requiredWeight);
            RecalculateWeight();
        } //end SetRequiredWeight(float)

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        // Draws the sensor volume in the Scene view
        private void OnDrawGizmos()
        {
            if (!TryGetComponent(out BoxCollider box)) return;

            Gizmos.color = IsActivated ? Color.green : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        } //end OnDrawGizmos()

        #endregion

        #region Helpers

        /// <summary>
        /// Totals the mass of every tracked Rigidbody and raises events for any change.
        /// </summary>
        private void RecalculateWeight()
        {
            float total = 0f;
            foreach (Rigidbody body in _colliderCounts.Keys)
            {
                // Destroyed bodies are skipped rather than counted
                if (body != null) total += body.mass;
            } //end foreach

            CurrentWeight = total;
            _onWeightChanged.Invoke(CurrentWeight, _requiredWeight);

            bool isNowActivated = CurrentWeight >= _requiredWeight - WEIGHT_TOLERANCE;
            if (isNowActivated != IsActivated)
            {
                IsActivated = isNowActivated;
                _onActivationChanged.Invoke(IsActivated);
            } //end if(isNowActivated != IsActivated)
        } //end RecalculateWeight()

        #endregion

    } //end WeightSensor
} //end namespace CSG.Physics
