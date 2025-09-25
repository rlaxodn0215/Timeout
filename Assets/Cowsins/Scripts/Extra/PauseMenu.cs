using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace cowsins
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private PlayerDependencies playerDependencies;
        [SerializeField] private GameObject playerUI;
        [SerializeField] private Selectable firstSelectedItem;
        [SerializeField] private bool disablePlayerUIWhilePaused;
        [SerializeField] private CanvasGroup menu;
        [SerializeField] private float fadeSpeed;
        private Coroutine fadeCoroutine;

        private IPlayerControlProvider playerControlProvider; // IPlayerControlProvider is implemented in PlayerControl.cs
        private IPlayerStatsProvider playerStatsProvider; // IPlayerStatsProvider is implemented in PlayerStats.cs

        public static PauseMenu Instance { get; private set; }

        /// <summary>
        /// Returns the Pause State of the game
        /// </summary>
        public static bool isPaused { get; private set; }

        public event Action OnPause;
        public event Action OnUnPause;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initially, the game is not paused
            isPaused = false;
            menu.gameObject.SetActive(false);
            menu.alpha = 0;

            InputManager.onTogglePause += TogglePause;

            if (EventSystem.current == null)
                Debug.LogError("<color=red>[COWSINS]</color> No <b><color=cyan>EventSystem</color></b> object found in the scene. " +
                    "Please create a new Empty GameObject and assign the EventSystem component to it to fix this error.");
        }

        private void Start()
        {
            playerControlProvider = playerDependencies.PlayerControl;
            playerStatsProvider = playerDependencies.PlayerStats;
        }

        private void OnDisable()
        {
            InputManager.onTogglePause -= TogglePause;
        }

        private IEnumerator HandlePause()
        {
            if (disablePlayerUIWhilePaused && !playerStatsProvider.IsDead)
                playerUI.SetActive(false);

            menu.gameObject.SetActive(true);
            while (menu.alpha < 1)
            {
                menu.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            menu.alpha = 1;
        }

        private IEnumerator HandleUnpause()
        {
            playerUI.SetActive(true);
            while (menu.alpha > 0)
            {
                menu.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            menu.alpha = 0;
            menu.gameObject.SetActive(false);
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            if (isPaused)
            {
                playerControlProvider.LoseControl();
                fadeCoroutine = StartCoroutine(HandlePause());
                OnPause?.Invoke();
                if(Gamepad.current !=  null)
                    EventSystem.current?.SetSelectedGameObject(firstSelectedItem.gameObject);
            }
            else
                UnPause();
        }

        public void UnPause()
        {
            isPaused = false;
            playerControlProvider.CheckIfCanGrantControl();
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(HandleUnpause());
            OnUnPause?.Invoke();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}
