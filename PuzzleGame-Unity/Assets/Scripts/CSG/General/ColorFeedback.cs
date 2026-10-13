/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    ColorFeedback.cs
 * DESCRIPTION:  Reusable component that tints renderers to show an
 *               inactive, partial, or active state.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using UnityEngine;

namespace CSG.General
{
    /// <summary>
    /// Gives clear visual feedback by tinting a set of renderers based on
    /// progress toward an active state.
    /// </summary>
    public class ColorFeedback : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// URP Lit base color shader property.
        /// </summary>
        private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");

        /// <summary>
        /// Built-in pipeline color shader property (fallback).
        /// </summary>
        private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

        /// <summary>
        /// Progress value at which the state counts as fully active.
        /// </summary>
        private const float FULL_PROGRESS = 1f;

        #endregion

        #region Fields & Properties

        [Header("Targets")]
        [SerializeField]
        [Tooltip("Renderers to tint. Uses renderers on this object if left empty.")]
        private Renderer[] _renderers;

        [Header("State Colors")]
        [SerializeField]
        [Tooltip("Color when nothing has been achieved yet.")]
        private Color _inactiveColor = new Color(0.85f, 0.25f, 0.25f);

        [SerializeField]
        [Tooltip("Color when some progress has been made.")]
        private Color _partialColor = new Color(1f, 0.75f, 0.2f);

        [SerializeField]
        [Tooltip("Color when fully active.")]
        private Color _activeColor = new Color(0.3f, 0.9f, 0.4f);

        /// <summary>
        /// Reusable property block so materials are not duplicated.
        /// </summary>
        private MaterialPropertyBlock _propertyBlock;

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            } //end if(no renderers assigned)

            SetProgress(0f);
        } //end Awake()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        /// <summary>
        /// Shows the inactive, partial, or active color based on normalized progress.
        /// </summary>
        /// <param name="progress">0 = inactive, between 0 and 1 = partial, 1 or more = active.</param>
        public void SetProgress(float progress)
        {
            if (progress >= FULL_PROGRESS)
            {
                SetColor(_activeColor);
            }
            else if (progress > 0f)
            {
                SetColor(_partialColor);
            }
            else
            {
                SetColor(_inactiveColor);
            } //end if(progress)
        } //end SetProgress(float)

        /// <summary>
        /// Shows the active or inactive color.
        /// </summary>
        /// <param name="isActive">True for the active color.</param>
        public void SetActiveState(bool isActive)
        {
            SetColor(isActive ? _activeColor : _inactiveColor);
        } //end SetActiveState(bool)

        /// <summary>
        /// Tints every target renderer with a specific color.
        /// </summary>
        /// <param name="color">The color to apply.</param>
        public void SetColor(Color color)
        {
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

            foreach (Renderer target in _renderers)
            {
                if (target == null) continue;

                target.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BASE_COLOR_ID, color);
                _propertyBlock.SetColor(COLOR_ID, color);
                target.SetPropertyBlock(_propertyBlock);
            } //end foreach
        } //end SetColor(Color)

        #endregion

        #region Deactivation & Destruction

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        #endregion

    } //end ColorFeedback
} //end namespace CSG.General
