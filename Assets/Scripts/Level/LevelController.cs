using System;
using UnityEngine;

namespace BubbleShooter
{
    public class LevelController : SingletonBase<LevelController>
    {
        #region Properties
        [SerializeField] GameStateController _gameStateController;
        [SerializeField] private Launcher _launcher;
        [SerializeField] private string[] _levelGridData;
        public string[] LevelGridData => _levelGridData;

        #region Events

        public event Action OnProjectileDestroy;
        public event Action OnWin;
        public event Action OnLose;


        #endregion

        #endregion

        #region Unity Events
        protected override void Awake()
        {
            base.Awake();

            GameStateController.OnLevelRestart += RemoveBubbles;
            _gameStateController.SubscribeGameEvents();

        }

        private void Update()
        {
            if (GetComponentInChildren<Projectile>() == null)
                TriggerOnProjectileDestroy();
        }

        private void FixedUpdate()
        {
            if (GetComponent<BubbleGroupController>().BubbleGroup.childCount == 0)
                OnWin.Invoke();

            if (Bubble.IsLose)
                OnLose.Invoke();
        }

        private void OnDestroy()
        {
            GameStateController.OnLevelRestart -= RemoveBubbles;
        }
        #endregion

        #region Private API
        private void TriggerOnProjectileDestroy()
        {
            OnProjectileDestroy.Invoke();
            GetComponentInChildren<Projectile>().OnHit.AddListener(TriggerOnProjectileDestroy);
        }
        private void RemoveBubbles()
        {
            var bubbles = GetComponentsInChildren<Bubble>();

            foreach (var bubble in bubbles)
            {
                Destroy(bubble.gameObject);
            }
        }
        #endregion
    }
}



