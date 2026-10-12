/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    TonePlayer.cs
 * DESCRIPTION:  Reusable audio feedback component that synthesizes short
 *               tones in code, so no audio asset files are required.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace CSG.Puzzle.Components
{
    /// <summary>
    /// Plays short generated sine-wave tones for audio feedback.
    /// Generated clips are cached by frequency.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class TonePlayer : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Samples per second used when generating tones.
        /// </summary>
        private const int SAMPLE_RATE = 44100;

        /// <summary>
        /// Fraction of the tone used to fade in and out, preventing audible clicks.
        /// </summary>
        private const float FADE_FRACTION = 0.1f;

        #endregion

        #region Fields & Properties

        [Header("Tone Settings")]
        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Length of each tone in seconds.")]
        private float _duration = 0.15f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Playback volume of the tones.")]
        private float _volume = 0.3f;

        /// <summary>
        /// Audio source used for playback.
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        /// Generated clips keyed by frequency, so each tone is only built once.
        /// </summary>
        private readonly Dictionary<float, AudioClip> _clipCache = new Dictionary<float, AudioClip>();

        #endregion

        #region Editor Callbacks

        #endregion

        #region Initialization & Activation

        // Awake is called once on initialization, before any Start; cache component references here
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        } //end Awake()

        #endregion

        #region Update Game Loop

        #endregion

        #region Update Physics

        #endregion

        #region Feature Logic

        /// <summary>
        /// Plays a tone at the given frequency.
        /// </summary>
        /// <param name="frequency">Pitch of the tone in hertz.</param>
        public void PlayTone(float frequency)
        {
            if (!_clipCache.TryGetValue(frequency, out AudioClip clip))
            {
                clip = CreateToneClip(frequency);
                _clipCache[frequency] = clip;
            } //end if(clip not cached)

            _audioSource.PlayOneShot(clip, _volume);
        } //end PlayTone(float)

        #endregion

        #region Deactivation & Destruction

        // OnDestroy releases generated clips
        private void OnDestroy()
        {
            foreach (AudioClip clip in _clipCache.Values)
            {
                Destroy(clip);
            } //end foreach
        } //end OnDestroy()

        #endregion

        #region Gizmos

        #endregion

        #region Helpers

        /// <summary>
        /// Builds a sine-wave AudioClip with a short fade in and out.
        /// </summary>
        /// <param name="frequency">Pitch of the tone in hertz.</param>
        /// <returns>The generated clip.</returns>
        private AudioClip CreateToneClip(float frequency)
        {
            int sampleCount = Mathf.CeilToInt(SAMPLE_RATE * _duration);
            int fadeSamples = Mathf.Max(1, Mathf.CeilToInt(sampleCount * FADE_FRACTION));
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                // Envelope ramps up at the start and down at the end
                float envelope = Mathf.Min(1f, Mathf.Min(i, sampleCount - 1 - i) / (float)fadeSamples);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / SAMPLE_RATE) * envelope;
            } //end for

            AudioClip clip = AudioClip.Create($"Tone_{frequency}", sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        } //end CreateToneClip(float)

        #endregion

    } //end TonePlayer
} //end namespace CSG.Puzzle.Components
