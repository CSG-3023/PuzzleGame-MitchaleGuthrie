/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    TriggerZone.cs
 * DESCRIPTION:  Reusable trigger volume that raises an event when an
 *               object with a matching tag enters it.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace CSG.Physics
{
    /// <summary>
    /// Raises an event when a collider with the configured tag enters the trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        #region Constants

        #endregion

        #region Fields & Properties

        [Header("Filter")]
        [SerializeField]
        [Tooltip("Only objects with this tag trigger the zone. Leave empty to accept any object.")]
        private string _requiredTag = "Player";

        [Header("Events")]
        [SerializeField]
        [Tooltip("Raised when a matching object enters. Passes the object that entered.")]
        private UnityEvent<GameObject> _onEntered = new UnityEvent<GameObject>();

        /// <summary>
        /// Event raised when a matching object enters the zone.
        /// </summary>
        public UnityEvent<GameObject> OnEntered => _onEntered;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        } //end Awake()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        // OnTriggerEnter is called when a collider enters the zone
        private void OnTriggerEnter(Collider other)
        {
            // Use the Rigidbody's object when present so child colliders report their owner
            GameObject entered = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

            if (string.IsNullOrEmpty(_requiredTag) || entered.CompareTag(_requiredTag))
            {
                _onEntered.Invoke(entered);
            } //end if(tag matches)
        } //end OnTriggerEnter()

        #endregion

        #region Feature Logic

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        #endregion

    } //end TriggerZone
} //end namespace CSG.Physics
