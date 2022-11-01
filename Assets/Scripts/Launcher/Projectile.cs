using UnityEngine;
using UnityEngine.Events;

namespace BubbleShooter
{
    public class Projectile : MonoBehaviour
    {
        public enum BubbleTypeQueue { Cycle, Random }

        public UnityEvent OnHit;

        [SerializeField] private SpriteRenderer _nextBallPreview;
        [SerializeField] private Sprite[] _bubbleVariants;
        [SerializeField] private BubbleTypeQueue _bubbleTypeQueue;

        private SpriteRenderer _projectileBubbleSprite;
        private BubbleGroupController _gridManager;
        private int _bubbleType;
        private bool _isHit = false;

        #region Unity Events
        private void Awake()
        {
            transform.parent = LevelController.Instance.transform;
        }

        void Start()
        {
            _projectileBubbleSprite = GetComponent<SpriteRenderer>();
            _nextBallPreview = GameObject.Find("BubblePreview").GetComponent<SpriteRenderer>();

            switch (_bubbleTypeQueue)
            {
                case BubbleTypeQueue.Cycle:
                    _bubbleType = LevelController.Instance.GetComponentInChildren<Launcher>().CurrentProjectileType;
                    break;
                case BubbleTypeQueue.Random:
                    _bubbleType = Random.Range(1, _bubbleVariants.Length + 1);
                    break;
            }

            _nextBallPreview.sprite = _bubbleVariants[_bubbleType - 1];
            _projectileBubbleSprite.enabled = false;

            _gridManager = transform.parent.GetComponent<BubbleGroupController>();

            TryGetComponent(out SpriteRenderer spriteRenderer);
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = _bubbleVariants[_bubbleType - 1];
            }
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

        #endregion

        #region Public API

        public void SwitchBubbleQueueType(bool isfixed)
        {
            if (isfixed)
                _bubbleTypeQueue = BubbleTypeQueue.Cycle;
            if (!isfixed)
                _bubbleTypeQueue = BubbleTypeQueue.Random;
        }

        #endregion

        #region Private API

        private void CollideImpact()
        {
            _isHit = true;

            GetComponent<CircleCollider2D>().enabled = false;

            Bubble newBubble = _gridManager.InitiateBubble(transform.position, _bubbleType);
            _gridManager.SearchThroughBubbleMap(newBubble.BubbleRow, -newBubble.BubbleColumns, newBubble.BubbleType);

            OnHit.Invoke();
            Destroy(gameObject);
        }
        #endregion
    }

}
