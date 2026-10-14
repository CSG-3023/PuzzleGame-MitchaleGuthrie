/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    SceneLoader.cs
 * DESCRIPTION:  Reusable component for loading scenes by name, usable
 *               from code or from UI button events.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CSG.General
{
    /// <summary>
    /// Loads scenes by name or reloads the active scene.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        #region Constants

        #endregion

        #region Fields & Properties

        [Header("Scene Settings")]
        [SerializeField]
        [Tooltip("Scene loaded by LoadTargetScene(). Must be in the Build Profile scene list.")]
        private string _targetSceneName;

        /// <summary>
        /// Name of the scene loaded by <see cref="LoadTargetScene"/>.
        /// </summary>
        public string TargetSceneName => _targetSceneName;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        /// <summary>
        /// Loads the scene configured in the Inspector.
        /// </summary>
        public void LoadTargetScene()
        {
            LoadScene(_targetSceneName);
        } //end LoadTargetScene()

        /// <summary>
        /// Loads a scene by name. Suitable for UI button events.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning($"{name}: No scene name provided to SceneLoader.", this);
                return;
            } //end if(no scene name)

            SceneManager.LoadScene(sceneName);
        } //end LoadScene(string)

        /// <summary>
        /// Reloads the currently active scene, resetting it to its starting state.
        /// </summary>
        public void ReloadActiveScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        } //end ReloadActiveScene()

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        #endregion

    } //end SceneLoader
} //end namespace CSG.General
