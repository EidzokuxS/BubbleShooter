using UnityEngine;

namespace BubbleShooter
{
    public class Launcher : MonoBehaviour
    {
        #region Properties
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private float _bubbleSpeed = 10f;
        [SerializeField] float _fireRate = 0.5F;

        private Projectile _nextProjectile;
        private Projectile _currentProjectile;
        private int _currentProjectileType;
        public int CurrentProjectileType => _currentProjectileType;

        #endregion

        #region UnityEvents
        private void Start()
        {
            LevelController.Instance.OnProjectileDestroy += LoadBubble;
        }

        private void Update()
        {
            GetMousePosition();

            if (Input.GetMouseButtonUp(0))
            {
                ShootBubble();
            }
        }
        #endregion

        #region Public API
        public void LoadBubble()
        {
            {
                ChangeCurrentProjectileType();
                _nextProjectile = null;
                _nextProjectile = Instantiate(_projectilePrefab, transform.position, transform.rotation);
                _nextProjectile.gameObject.SetActive(true);
                _currentProjectile = _nextProjectile;

                _nextProjectile.GetComponent<CircleCollider2D>().enabled = false;
            }
        }

        #endregion

        #region Private API

        private void GetMousePosition()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 delta = mousePosition - new Vector2(transform.position.x, transform.position.y);

            float clampValue = Mathf.Clamp(-Mathf.Rad2Deg * Mathf.Atan2(delta.x, delta.y), -60, 60);
            transform.rotation = Quaternion.Euler(0f, 0f, clampValue);
        }

        private void ShootBubble()
        {
            if (GameStateController.IsPaused == false && Time.time > _fireRate)
            {
                _currentProjectile.GetComponent<SpriteRenderer>().enabled = true;

                if (_nextProjectile != null)
                {
                    _nextProjectile.GetComponent<CircleCollider2D>().enabled = true;
                    _nextProjectile.GetComponent<Rigidbody2D>().velocity = transform.up * _bubbleSpeed;
                }
            }
        }

        private void ChangeCurrentProjectileType()
        {
            if (_currentProjectileType < 5)
            {
                _currentProjectileType++;
            }
            else
                _currentProjectileType = 1;
        }

        #endregion
    }

}
