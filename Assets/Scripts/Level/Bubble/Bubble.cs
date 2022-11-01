using UnityEngine;

namespace BubbleShooter
{
    public class Bubble : MonoBehaviour
    {
        #region Properties
        public static bool IsLose { get; private set; }

        [SerializeField] private Sprite[] _bubbleSprites;
        public Sprite[] BubbleSprites => _bubbleSprites;

        [SerializeField] private float _popSpeed = 0.9f;
        [SerializeField] private float _explodeSpeed = 5f;
        [SerializeField] private float _collapsePointY = -30f;

        private int _bubbleColumn;
        public int BubbleColumns => _bubbleColumn;
        private int _bubbleRow;
        public int BubbleRow => _bubbleRow;
        private int _bubbleType;
        public int BubbleType => _bubbleType;
        private string _bubbleState;
        public string BubbleState => _bubbleState;

        private CircleCollider2D _circleCollider;
        #endregion

        #region Unity Events

        private void Start()
        {
            _circleCollider = GetComponent<CircleCollider2D>();
        }

        public void Update()
        {
            if (_bubbleState == "Pop")
            {
                if (_circleCollider != null)
                    _circleCollider.enabled = false;

                transform.localScale = transform.localScale * _popSpeed;
                if (transform.localScale.sqrMagnitude < 0.05f)
                    Destroy(gameObject);
            }
            else if (_bubbleState == "Explode")
            {
                if (_circleCollider != null)
                    _circleCollider.enabled = false;

                TryGetComponent(out Rigidbody2D rigidbody);
                if (rigidbody != null)
                {
                    rigidbody.gravityScale = 1f;
                    rigidbody.velocity = new Vector3(Random.Range(-_explodeSpeed, _explodeSpeed), Random.Range(-_explodeSpeed, _explodeSpeed), 0f);
                }
                _bubbleState = "Fall";
            }
            else if (_bubbleState == "Fall")
            {
                if (transform.position.y < _collapsePointY)
                    Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.name == "LoseZone")
            {
                IsLose = true;
            }
        }

        private void OnDestroy()
        {
            IsLose = false;
        }

        #endregion

        #region Public API
        public void SetGridPosition(int column, int row, int type)
        {
            _bubbleRow = row;
            _bubbleColumn = column;
            _bubbleType = type;
        }
        public void SetBubbleState(string state)
        {
            _bubbleState = state;
        }
        #endregion
    }
}
