/************************************************************
- COPYRIGHT:  2026
- PROJECT:    CSG Main Menu
- FILE NAME:  MainMenuManager.cs
- DESCRIPTION: Manages UI Toolkit bindings, button interaction callbacks, 
              content updates, and scene transitions for the main menu.
*
- REVISION HISTORY:
- Date [YYYY/MM/DD] | Author | Comments
- ------------------------------------------------------------
- 2026/10/12 | Akram Taghavi-Burris | Created class

************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CSG.Manager
{
    /// <summary>
    /// Controls UI element bindings, Inspector property synchronization,
    /// instruction modal visibility, and scene transition callbacks.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuManager : MonoBehaviour
    {
        #region Constants & Magic Strings
        // UXML Element Names
        private const string ELEMENT_TITLE = "GameTitle";
        private const string ELEMENT_BYLINE = "Byline";
        private const string ELEMENT_INSTRUCTIONS_BODY = "TXT_Body";
        private const string ELEMENT_PANEL_INSTRUCTIONS = "PNL_Instructions";
        private const string ELEMENT_BTN_PLAY = "BTN_Play";
        private const string ELEMENT_BTN_INSTRUCTIONS = "BTN_Instructions";
        private const string ELEMENT_BTN_QUIT = "BTN_Quit";
        private const string ELEMENT_BTN_CLOSE = "BTN_Close";

        // USS Classes
        private const string CLASS_HIDDEN = "hidden";

        // Defaults
        private const string DEFAULT_GAME_LEVEL = "Level1";
        private const string DEFAULT_GAME_TITLE = "GAME TITLE";
        private const string DEFAULT_BYLINE = "Designed By: Your Name Here";
        private const string DEFAULT_INSTRUCTIONS = "Instruction information goes here...";
        #endregion

        #region Fields & Properties
        [Header("Scene Configuration")]
        [Tooltip("The exact string name of the first game scene to load when PLAY is pressed.")]
        [SerializeField] private string _gameLevel = DEFAULT_GAME_LEVEL;

        [Header("Menu Text Content")]
        [Tooltip("The title of your game.")]
        [SerializeField] private string _gameTitle = DEFAULT_GAME_TITLE;

        [Tooltip("Byline info, e.g., 'Designed By: Your Name Here'.")]
        [SerializeField] private string _byline = DEFAULT_BYLINE;

        [Tooltip("Full text instructions for playing the game.")]
        [TextArea(5, 10)]
        [SerializeField] private string _instructionTxt = DEFAULT_INSTRUCTIONS;
        
        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _lblTitle;
        private Label _lblByline;
        private Label _lblInstructions;

        private VisualElement _pnlInstructions;
        private Button _btnPlay;
        private Button _btnInstructions;
        private Button _btnQuit;
        private Button _btnClose;

        /// <summary>
        /// Gets or sets the target scene name to load on play.
        /// </summary>
        public string GameLevel
        {
            get => _gameLevel;
            set
            {
                _gameLevel = value;
                UpdateUIContent();
            }
        }

        /// <summary>
        /// Gets or sets the game title displayed on the menu header.
        /// </summary>
        public string GameTitle
        {
            get => _gameTitle;
            set
            {
                _gameTitle = value;
                UpdateUIContent();
            }
        }

        /// <summary>
        /// Gets or sets the author byline text.
        /// </summary>
        public string Byline
        {
            get => _byline;
            set
            {
                _byline = value;
                UpdateUIContent();
            }
        }

        /// <summary>
        /// Gets or sets the instructional body text displayed in the instructions panel.
        /// </summary>
        public string InstructionTxt
        {
            get => _instructionTxt;
            set
            {
                _instructionTxt = value;
                UpdateUIContent();
            }
        }
        
        #endregion

        #region Initialization & Activation
        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                return;
            }

            BindUIElements();
            RegisterCallbacks();
            UpdateUIContent();

            // Enforce instructions panel starting hidden on initialization
            HideInstructions();
        }

        /// <summary>
        /// Updates the UI view live in the Unity Editor when Inspector properties are modified.
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                BindUIElements();
                UpdateUIContent();
            }
        }
        #endregion

        #region Features Logic
        /// <summary>
        /// Updates element text fields and background styles to match current property values.
        /// </summary>
        public void UpdateUIContent()
        {
            if (_lblTitle != null) _lblTitle.text = _gameTitle;

            if (_lblByline != null) _lblByline.text = _byline;

            if (_lblInstructions != null) _lblInstructions.text = _instructionTxt;
            
        }//end UpdateUIContent()

        /// <summary>
        /// Queries and caches visual elements from the UI Document root.
        /// </summary>
        private void BindUIElements()
        {
            _root = _uiDocument.rootVisualElement;

            _lblTitle = _root.Q<Label>(ELEMENT_TITLE);
            _lblByline = _root.Q<Label>(ELEMENT_BYLINE);
            _lblInstructions = _root.Q<Label>(ELEMENT_INSTRUCTIONS_BODY);

            _pnlInstructions = _root.Q<VisualElement>(ELEMENT_PANEL_INSTRUCTIONS);
            _btnPlay = _root.Q<Button>(ELEMENT_BTN_PLAY);
            _btnInstructions = _root.Q<Button>(ELEMENT_BTN_INSTRUCTIONS);
            _btnQuit = _root.Q<Button>(ELEMENT_BTN_QUIT);
            _btnClose = _root.Q<Button>(ELEMENT_BTN_CLOSE);
        }//end BindElements

        /// <summary>
        /// Unregisters and re-registers click event callbacks to prevent duplicate subscriptions.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (_btnPlay != null)
            {
                _btnPlay.clicked -= PlayGame;
                _btnPlay.clicked += PlayGame;
            }

            if (_btnInstructions != null)
            {
                _btnInstructions.clicked -= ShowInstructions;
                _btnInstructions.clicked += ShowInstructions;
            }

            if (_btnClose != null)
            {
                _btnClose.clicked -= HideInstructions;
                _btnClose.clicked += HideInstructions;
            }

            if (_btnQuit != null)
            {
                _btnQuit.clicked -= QuitGame;
                _btnQuit.clicked += QuitGame;
            }
        }//end RegisterCallbacks()

        /// <summary>
        /// Loads the specified game level scene.
        /// </summary>
        private void PlayGame()
        {
            if (!string.IsNullOrEmpty(_gameLevel))
            {
                SceneManager.LoadScene(_gameLevel);
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] Game Level scene name is empty! Please assign a scene name in the Inspector.");
            }
        }//end PlayGame()

        /// <summary>
        /// Displays the instructions panel overlay.
        /// </summary>
        private void ShowInstructions()
        {
            if (_pnlInstructions != null)
            {
                _pnlInstructions.RemoveFromClassList(CLASS_HIDDEN);
            }
        }//end ShowInstructions()

        /// <summary>
        /// Hides the instructions panel overlay.
        /// </summary>
        private void HideInstructions()
        {
            if (_pnlInstructions != null)
            {
                _pnlInstructions.AddToClassList(CLASS_HIDDEN);
            }
        }//end HideInstructions()

        /// <summary>
        /// Quits the built application or exits Play Mode in the Unity Editor.
        /// </summary>
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }//end QuitGame()
        #endregion
        
    }//end MainMenuManager
}//end namespace CSG