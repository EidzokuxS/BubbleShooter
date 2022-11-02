using UnityEngine;
using UnityEngine.Events;

namespace BubbleShooter
{
    public class Projectile : MonoBehaviour
    {
        public UnityEvent OnHit;

        [SerializeField] private SpriteRenderer _nextBallPreview;
        [SerializeField] private Sprite[] _bubbleVariants;
        [SerializeField] private bool _isBubbleTypeQueueFixed;

        private SpriteRenderer _projectileBubbleSprite;
        private BubbleGroupController _gridManager;
        private int _bubbleType;
        private bool _isHit = false;

        #region Unity Events
        private void Awake()
        {
            transform.parent = LevelController.Instance.transform;
            GameStateController.OnLevelRestart += DestroyCurrentProjectile;
        }

        void Start()
        {
            _projectileBubbleSprite = GetComponent<SpriteRenderer>();
            _nextBallPreview = GameObject.Find("BubblePreview").GetComponent<SpriteRenderer>();
            _gridManager = transform.parent.GetComponent<BubbleGroupController>();

            if (_isBubbleTypeQueueFixed)
                _bubbleType = LevelController.Instance.GetComponentInChildren<Launcher>().CurrentProjectileType;
            else if (!_isBubbleTypeQueueFixed)
                _bubbleType = Random.Range(1, _bubbleVariants.Length + 1);

            ChangeBubbleType();
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isHit && collision.gameObject.name == "Ceiling")
            {
                CollideImpact();
            }
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider)
            {
                if (!_isHit && collider.GetComponent<Bubble>())
                {
                    CollideImpact();
                }

            }

        }

        private void OnDestroy()
        {
            GameStateController.OnLevelRestart -= DestroyCurrentProjectile;
        }
        #endregion

        #region Public API

        public void SwitchBubbleQueueType(bool isFixed)
        {
            _isBubbleTypeQueueFixed = isFixed;
        }

        #endregion

        #region Private API
        private void ChangeBubbleType()
        {
            _nextBallPreview.sprite = _bubbleVariants[_bubbleType - 1];
            _projectileBubbleSprite.enabled = false;

            TryGetComponent(out SpriteRenderer spriteRenderer);
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = _bubbleVariants[_bubbleType - 1];
            }
        }

        private void CollideImpact()
        {
            _isHit = true;

            GetComponent<CircleCollider2D>().enabled = false;

            Bubble newBubble = _gridManager.InitiateBubble(transform.position, _bubbleType);
            _gridManager.SearchThroughBubbleMap(newBubble.BubbleRow, -newBubble.BubbleColumns, newBubble.BubbleType);

            OnHit.Invoke();
            DestroyCurrentProjectile();
        }

        private void DestroyCurrentProjectile()
        {
            Destroy(gameObject);
        }
        #endregion
    }


}
