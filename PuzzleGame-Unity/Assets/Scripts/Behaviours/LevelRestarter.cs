/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    LevelRestarter.cs
 * DESCRIPTION:  Lets the player instantly retry the level with a key
 *               press, so mistakes are never punishing.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using CSG.Puzzle.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CSG.Puzzle.Behaviours
{
    /// <summary>
    /// Reloads the current level when the restart key is pressed.
    /// </summary>
    [RequireComponent(typeof(SceneLoader))]
    public class LevelRestarter : MonoBehaviour
    {
        #region Constants

        #endregion

        #region Fields & Properties

        [Header("Input")]
        [SerializeField]
        [Tooltip("Input binding path that restarts the level.")]
        private string _restartBinding = "<Keyboard>/r";

        /// <summary>
        /// Loader used to reload the active scene.
        /// </summary>
        private SceneLoader _sceneLoader;

        /// <summary>
        /// Button action created from the restart binding.
        /// </summary>
        private InputAction _restartAction;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _sceneLoader = GetComponent<SceneLoader>();
            _restartAction = new InputAction("Restart", InputActionType.Button, _restartBinding);
        } //end Awake()

        // OnEnable starts listening for the restart key
        private void OnEnable()
        {
            _restartAction.performed += HandleRestart;
            _restartAction.Enable();
        } //end OnEnable()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        #endregion

        #region Deactivation & Destruction

        // OnDisable stops listening for the restart key
        private void OnDisable()
        {
            _restartAction.performed -= HandleRestart;
            _restartAction.Disable();
        } //end OnDisable()

        // OnDestroy releases the generated action
        private void OnDestroy()
        {
            _restartAction.Dispose();
        } //end OnDestroy()

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        /// <summary>
        /// Reloads the level when the restart action fires.
        /// </summary>
        /// <param name="context">Input callback context.</param>
        private void HandleRestart(InputAction.CallbackContext context)
        {
            _sceneLoader.ReloadActiveScene();
        } //end HandleRestart(InputAction.CallbackContext)

        #endregion

    } //end LevelRestarter
} //end namespace CSG.Puzzle.Behaviours
