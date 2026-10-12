/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    PuzzleManager.cs
 * DESCRIPTION:  Monitors the level's puzzle conditions, decides when the
 *               puzzle is complete, and transitions to the End Scene.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using System.Collections;
using CSG.Puzzle.Components;
using UnityEngine;

namespace CSG.Puzzle.Behaviours
{
    /// <summary>
    /// Completes the puzzle when the player reaches the exit while every required
    /// gate is open, then loads the End Scene after a short celebration delay.
    /// </summary>
    [RequireComponent(typeof(SceneLoader))]
    public class PuzzleManager : MonoBehaviour
    {
        #region Constants

        #endregion

        #region Fields & Properties

        [Header("Puzzle Conditions")]
        [SerializeField]
        [Tooltip("Every gate listed here must be open for the puzzle to count as solved.")]
        private Gate[] _requiredGates;

        [SerializeField]
        [Tooltip("Zone the player must reach to finish the level.")]
        private TriggerZone _exitZone;

        [Header("Completion")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Seconds to wait after solving before loading the End Scene.")]
        private float _completionDelay = 1.5f;

        [SerializeField]
        [Tooltip("Plays the victory tones.")]
        private TonePlayer _tonePlayer;

        [SerializeField]
        [Tooltip("Ascending victory jingle frequencies (Hz).")]
        private float[] _victoryTones = { 523f, 659f, 784f, 1047f };

        [SerializeField]
        [Min(0f)]
        [Tooltip("Seconds between victory jingle notes.")]
        private float _victoryNoteInterval = 0.15f;

        /// <summary>
        /// Loads the End Scene once the puzzle is solved.
        /// </summary>
        private SceneLoader _sceneLoader;

        /// <summary>
        /// Time (since level load) when the level started.
        /// </summary>
        private float _startTime;

        /// <summary>
        /// True once the puzzle has been solved.
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// Seconds the player took to solve the most recent attempt. Read by the End Scene.
        /// </summary>
        public static float LastCompletionTime { get; private set; }

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _sceneLoader = GetComponent<SceneLoader>();
        } //end Awake()

        // OnEnable subscribes to the exit zone
        private void OnEnable()
        {
            if (_exitZone != null) _exitZone.OnEntered.AddListener(HandleExitReached);
        } //end OnEnable()

        // Start records when the attempt began
        private void Start()
        {
            _startTime = Time.time;
        } //end Start()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        /// <summary>
        /// Checks whether every required gate is currently open.
        /// </summary>
        /// <returns>True when all puzzle conditions are met.</returns>
        public bool AreConditionsMet()
        {
            foreach (Gate gate in _requiredGates)
            {
                if (gate == null || !gate.IsOpen) return false;
            } //end foreach
            return true;
        } //end AreConditionsMet()

        #endregion

        #region Deactivation & Destruction

        // OnDisable unsubscribes from the exit zone
        private void OnDisable()
        {
            if (_exitZone != null) _exitZone.OnEntered.RemoveListener(HandleExitReached);
        } //end OnDisable()

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        /// <summary>
        /// Completes the puzzle when the player reaches the exit with all conditions met.
        /// </summary>
        /// <param name="enteredObject">The object that reached the exit.</param>
        private void HandleExitReached(GameObject enteredObject)
        {
            if (IsComplete || !AreConditionsMet()) return;

            IsComplete = true;
            LastCompletionTime = Time.time - _startTime;
            StartCoroutine(CompleteRoutine());
        } //end HandleExitReached(GameObject)

        /// <summary>
        /// Plays the victory jingle, waits, then loads the End Scene.
        /// </summary>
        /// <returns>Coroutine enumerator.</returns>
        private IEnumerator CompleteRoutine()
        {
            if (_tonePlayer != null)
            {
                foreach (float tone in _victoryTones)
                {
                    _tonePlayer.PlayTone(tone);
                    yield return new WaitForSeconds(_victoryNoteInterval);
                } //end foreach
            } //end if(_tonePlayer != null)

            yield return new WaitForSeconds(_completionDelay);
            _sceneLoader.LoadTargetScene();
        } //end CompleteRoutine()

        #endregion

    } //end PuzzleManager
} //end namespace CSG.Puzzle.Behaviours
