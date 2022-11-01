using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BubbleShooter
{
    public class GameStateController : SingletonBase<GameStateController>
    {
        #region Events
        public static event Action OnLevelRestart;
        public static bool IsPaused { get; private set; }

        [SerializeField] private UnityEvent OnWinEvent;
        [SerializeField] private UnityEvent OnLoseEvent;

        #endregion

        #region Public API

        public void ReloadScene()
        {
            print("reloaded");
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        public void RestartLevel()
        {
            TriggerRestartEvent();

            if (LevelController.Instance)
            {
                LevelController.Instance.GetComponentInChildren<BubbleGroupController>().InitializeGroup();
            }


        }
        public void SubscribeGameEvents()
        {
            LevelController.Instance.OnLose += EnableLosePanel;
            LevelController.Instance.OnWin += EnableWinPanel;
        }

        public void TriggerRestartEvent()
        {
            OnLevelRestart?.Invoke();
        }

        public void PauseGame(bool ispause)
        {
            if (ispause)
            {
                IsPaused = true;
                Time.timeScale = 0f;
            }

            if (!ispause)
            {
                IsPaused = false;

                Time.timeScale = 1f;
            }
        }

        public void QuitApplication()
        {
            Application.Quit();
        }

        #endregion

        #region Private API
        private void EnableLosePanel()
        {
            OnLoseEvent.Invoke();
        }

        private void EnableWinPanel()
        {
            OnWinEvent.Invoke();
        }

        #endregion

    }
}
