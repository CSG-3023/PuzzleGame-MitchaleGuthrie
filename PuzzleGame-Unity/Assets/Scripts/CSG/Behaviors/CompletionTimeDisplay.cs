/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    CompletionTimeDisplay.cs
 * DESCRIPTION:  Shows how long the player took to solve the puzzle on
 *               the End Scene.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace CSG.Behaviors
{
    /// <summary>
    /// Writes <see cref="PuzzleManager.LastCompletionTime"/> into a UI Text.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class CompletionTimeDisplay : MonoBehaviour
    {
        #region Constants

        #endregion

        #region Fields & Properties

        [Header("Display")]
        [SerializeField]
        [Tooltip("Format string. {0} is replaced by the completion time in seconds.")]
        private string _format = "Solved in {0:0.0} seconds";

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Start writes the time once the scene has loaded
        private void Start()
        {
            Text label = GetComponent<Text>();
            float time = PuzzleManager.LastCompletionTime;

            // Hide the label if the scene was opened directly without playing the level
            label.text = time > 0f ? string.Format(_format, time) : string.Empty;
        } //end Start()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        #endregion

    } //end CompletionTimeDisplay
} //end namespace CSG.Behaviors
