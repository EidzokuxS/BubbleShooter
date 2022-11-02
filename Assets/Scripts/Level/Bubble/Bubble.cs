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

        public int BubbleColumns { get; private set; }
        public int BubbleRow { get; private set; }
        public int BubbleType { get; private set; }
        public string BubbleState { get; private set; }

        private CircleCollider2D _circleCollider;
        #endregion

        #region Unity Events

        private void Start()
        {
            _circleCollider = GetComponent<CircleCollider2D>();
        }

        public void Update()
        {
            ApplyStateBehaviour();
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
            BubbleRow = row;
            BubbleColumns = column;
            BubbleType = type;
        }
        public void SetBubbleState(string state)
        {
            BubbleState = state;
        }
        #endregion

        #region Private API
        private void ApplyStateBehaviour()
        {
            switch (BubbleState)
            {
                case "Pop":
                    if (_circleCollider != null)
                        _circleCollider.enabled = false;

                    transform.localScale = transform.localScale * _popSpeed;
                    if (transform.localScale.sqrMagnitude < 0.05f)
                        Destroy(gameObject);
                    break;

                case "Explode":
                    if (_circleCollider != null)
                        _circleCollider.enabled = false;

                    TryGetComponent(out Rigidbody2D rigidbody);
                    if (rigidbody != null)
                    {
                        rigidbody.gravityScale = 1f;
                        rigidbody.velocity = new Vector3(Random.Range(-_explodeSpeed, _explodeSpeed), Random.Range(-_explodeSpeed, _explodeSpeed), 0f);
                    }
                    BubbleState = "Fall";
                    break;

                case "Fall":
                    if (transform.position.y < _collapsePointY)
                        Destroy(gameObject);
                    break;
            }
        }

        #endregion
    }
}
