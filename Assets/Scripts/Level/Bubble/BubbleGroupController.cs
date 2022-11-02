using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public class BubbleGroupController : MonoBehaviour
    {
        #region Properties

        private enum BubbleArrangementMethod { Load, Generate }

        [Header("Grid")]
        [SerializeField] private int _rows;
        [SerializeField] private int _columns;
        [SerializeField] private Bubble _bubblePrefab;
        [SerializeField] private Transform _bubbleGroup;
        public Transform BubbleGroup => _bubbleGroup;
        [Range(0, 1), SerializeField] private float gap;
        [SerializeField] private Vector3 _initialPosition;

        private Bubble[,] _bubbleMap;
        private string _bubbleGridData;

        [Header("Level")]

        [SerializeField] private int _comboCounter = 3;
        [SerializeField] private BubbleArrangementMethod _arrangementMethod;
        private LevelController _levelController;
        private int _chosenLevel;
        private int _points;
        public int Points => _points;
        public int ChooseLevel => _chosenLevel;

        #endregion

        #region Events

        public event System.Action OnScoreChange;

        #endregion

        #region Unity Events

        private void Awake()
        {
            GameStateController.OnLevelRestart += ResetBubbleMap;
        }

        private void Start()
        {
            InitializeGroup();
        }

        #endregion

        #region Public API

        public void ChooseBubbleGroupLevel(int level)
        {
            if (level > 0)
            {
                _arrangementMethod = BubbleArrangementMethod.Load;
                _chosenLevel = level;
            }

            else if (level == 0)
            {
                _arrangementMethod = BubbleArrangementMethod.Generate;
            }

        }

        public Vector3 Snap(Vector3 position)
        {
            Vector3 objectOffset = position - _initialPosition;
            Vector3 objectSnap = new(Mathf.Round(objectOffset.x / gap), Mathf.Round(objectOffset.y / gap), 0f);

            if (objectSnap.y % 2 != 0)
            {
                if (objectOffset.x > objectSnap.x * gap)
                    objectSnap.x += 0.5f;

                else
                    objectSnap.x -= 0.5f;
            }

            return _initialPosition + objectSnap * gap;
        }

        public Bubble InitiateBubble(Vector2 position, int bubbleType)
        {
            DefineBubblePosition(position, out Vector3 snappedPosition, out int column, out int row);

            Bubble bubbleClone = Instantiate(_bubblePrefab, snappedPosition, Quaternion.identity);
            bubbleClone.transform.parent = _bubbleGroup;
            try
            {
                _bubbleMap[row, -column] = bubbleClone;
            }
            catch (System.IndexOutOfRangeException)
            {
                Destroy(bubbleClone);
                return null;
            }

            ConfigureBubble(bubbleType, column, row, bubbleClone);

            bubbleClone.gameObject.SetActive(true);
            try
            {
                _bubbleMap[row, -column] = bubbleClone;
            }
            catch (System.IndexOutOfRangeException) { }

            return bubbleClone;
        }

        public void SearchThroughBubbleMap(int row, int column, int type)
        {
            int[] coordinatesPair = new int[2] { row, column };

            CheckBubbleChainConnections(row, column, type, coordinatesPair, out Queue<GameObject> objectQueue, out int elementCounter);

            ApplyValidBubbleConnectionsDestruction(objectQueue, elementCounter);

            CheckBubbleAttachment(0);
        }

        private void CheckBubbleAttachment(int ceiling)
        {
            InitializeCoordinateQueue(out bool[,] visitedCoordinates, out int[] deltaX, out int[] deltaXPrime, out int[] deltaY, out Queue<int[]> coordinatePairQueue);

            for (int i = 0; i < _rows; i++)
            {
                int[] coordinatePair = new int[2] { i, ceiling };
                if (_bubbleMap[i, ceiling] != null)
                {
                    visitedCoordinates[i, ceiling] = true;
                    coordinatePairQueue.Enqueue(coordinatePair);
                }
            }

            while (coordinatePairQueue.Count != 0)
            {
                int[] firstElement = coordinatePairQueue.Dequeue();
                CycleThroughBubbleQueue(visitedCoordinates, deltaX, deltaXPrime, deltaY, coordinatePairQueue, firstElement);
            }

            ApplyValidBubbleConnectionsDestruction(visitedCoordinates);
        }

        public void InitializeGroup()
        {

            _levelController = LevelController.Instance;

            if (_arrangementMethod == BubbleArrangementMethod.Load && _chosenLevel - 1 < _levelController.LevelGridData.Length)
                _bubbleGridData = Resources.Load<TextAsset>(LevelController.Instance.LevelGridData[(_chosenLevel - 1)]).ToString();

            if (_arrangementMethod == BubbleArrangementMethod.Generate)
                _bubbleGridData = GenerateBubbleGrid(6, 4);

            _bubbleMap = new Bubble[_rows, _columns];

            ArragementBubble(_bubbleGridData);
        }

        #endregion

        #region Private API

        private void ResetBubbleMap()
        {
            _bubbleMap = null;
            _points = 0;
            OnScoreChange.Invoke();
        }

        private void DefineBubblePosition(Vector2 position, out Vector3 snappedPosition, out int column, out int row)
        {
            snappedPosition = Snap(position);
            column = (int)Mathf.Round((snappedPosition.y - _initialPosition.y) / gap);
            if (column % 2 != 0)
                row = (int)Mathf.Round((snappedPosition.x - _initialPosition.x) / gap - 0.5f);
            else
                row = (int)Mathf.Round((snappedPosition.x - _initialPosition.x) / gap);
        }

        private void ConfigureBubble(int bubbleType, int column, int row, Bubble bubbleClone)
        {
            TryGetComponent(out CircleCollider2D collider);
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            bubbleClone.TryGetComponent(out Bubble gridMember);
            if (gridMember != null)
            {
                if (bubbleType < _bubblePrefab.BubbleSprites.Length)
                    gridMember.SetGridPosition(column, row, bubbleType);

                bubbleClone.TryGetComponent(out SpriteRenderer spriteRenderer);
                if (spriteRenderer != null)
                    spriteRenderer.sprite = gridMember.BubbleSprites[gridMember.BubbleType];
            }
        }

        private void CheckBubbleChainConnections(int row, int column, int type, int[] coordinatesPair, out Queue<GameObject> objectQueue, out int elementCounter)
        {
            InitializeCoordinateQueue(out bool[,] visitedCoordinates, out int[] deltaX, out int[] deltaXPrime, out int[] deltaY, out Queue<int[]> coordinatePairQueue);

            visitedCoordinates[row, column] = true;

            objectQueue = new();
            coordinatePairQueue.Enqueue(coordinatesPair);

            elementCounter = 0;
            while (coordinatePairQueue.Count != 0)
            {
                int[] firstElement = coordinatePairQueue.Dequeue();
                Bubble firstBubble = _bubbleMap[firstElement[0], firstElement[1]];
                if (firstBubble != null)
                {
                    objectQueue.Enqueue(firstBubble.gameObject);
                }
                elementCounter += 1;
                CycleThroughBubbleQueue(type, visitedCoordinates, deltaX, deltaXPrime, deltaY, coordinatePairQueue, firstElement);
            }
        }

        private void InitializeCoordinateQueue(out bool[,] visitedCoordinates, out int[] deltaX, out int[] deltaXPrime, out int[] deltaY, out Queue<int[]> coordinatePairQueue)
        {
            visitedCoordinates = new bool[_rows, _columns];
            deltaX = new int[] { -1, 0, -1, 0, -1, 1 };
            deltaXPrime = new int[] { 1, 0, 1, 0, -1, 1 };
            deltaY = new int[] { -1, -1, 1, 1, 0, 0 };
            coordinatePairQueue = new();
        }

        private void CycleThroughBubbleQueue(int type, bool[,] visitedCoordinates, int[] deltaX, int[] deltaXPrime, int[] deltaY, Queue<int[]> coordinatePairQueue, int[] firstElement)
        {
            for (int i = 0; i < 6; i++)
            {
                int[] neighborElement = new int[2];
                if (firstElement[1] % 2 == 0)
                    neighborElement[0] = firstElement[0] + deltaX[i];
                else
                    neighborElement[0] = firstElement[0] + deltaXPrime[i];

                neighborElement[1] = firstElement[1] + deltaY[i];
                try
                {
                    CheckBubbleCorrect(type, visitedCoordinates, coordinatePairQueue, neighborElement);
                }
                catch (System.IndexOutOfRangeException) { }
            }
        }

        private void CycleThroughBubbleQueue(bool[,] visitedCoordinates, int[] deltaX, int[] deltaXPrime, int[] deltaY, Queue<int[]> coordinatePairQueue, int[] firstElement)
        {
            for (int i = 0; i < 6; i++)
            {
                int[] neighborElement = new int[2];
                if (firstElement[1] % 2 == 0)
                    neighborElement[0] = firstElement[0] + deltaX[i];
                else
                    neighborElement[0] = firstElement[0] + deltaXPrime[i];

                neighborElement[1] = firstElement[1] + deltaY[i];
                try
                {
                    CheckBubbleCorrect(visitedCoordinates, coordinatePairQueue, neighborElement);
                }
                catch (System.IndexOutOfRangeException) { }
            }
        }

        private void CheckBubbleCorrect(int type, bool[,] visitedCoordinates, Queue<int[]> coordinatePairQueue, int[] neighborElement)
        {
            Bubble bubble = _bubbleMap[neighborElement[0], neighborElement[1]];
            if (bubble != null && bubble.BubbleType == type)
            {
                if (!visitedCoordinates[neighborElement[0], neighborElement[1]])
                {
                    visitedCoordinates[neighborElement[0], neighborElement[1]] = true;
                    coordinatePairQueue.Enqueue(neighborElement);
                }
            }
        }

        private void CheckBubbleCorrect(bool[,] visitedCoordinates, Queue<int[]> coordinatePairQueue, int[] neighborElement)
        {
            Bubble bubble = _bubbleMap[neighborElement[0], neighborElement[1]];
            if (bubble != null)
            {
                if (!visitedCoordinates[neighborElement[0], neighborElement[1]])
                {
                    visitedCoordinates[neighborElement[0], neighborElement[1]] = true;
                    coordinatePairQueue.Enqueue(neighborElement);
                }
            }
        }

        private void ApplyValidBubbleConnectionsDestruction(Queue<GameObject> objectQueue, int elementCounter)
        {
            if (elementCounter >= _comboCounter)
            {
                while (objectQueue.Count != 0)
                {
                    objectQueue.Dequeue().TryGetComponent(out Bubble bubble);
                    if (bubble != null)
                    {
                        _bubbleMap[bubble.BubbleRow, -bubble.BubbleColumns] = null;
                        bubble.SetBubbleState("Pop");
                        _points++;
                        OnScoreChange.Invoke();
                    }
                }
            }
        }

        private void ApplyValidBubbleConnectionsDestruction(bool[,] visitedCoordinates)
        {
            for (int x = 0; x < _rows; x++)
            {
                for (int y = 0; y < _columns; y++)
                {
                    if (_bubbleMap[x, y] != null && !visitedCoordinates[x, y])
                    {
                        Bubble bubble = _bubbleMap[x, y];

                        if (bubble != null)
                        {
                            _bubbleMap[bubble.BubbleRow, -bubble.BubbleColumns] = null;
                            bubble.SetBubbleState("Explode");
                            _points++;
                            OnScoreChange.Invoke();
                        }
                    }
                }
            }
        }

        private void ArragementBubble(string level)
        {
            int levelPosition = 0;
            for (int y = 0; y < _columns; y++)
            {
                if (y % 2 != 0) _rows -= 1;
                for (int x = 0; x < _rows; x++)
                {
                    Vector3 position = new Vector3(x * gap, (-y) * gap, 0f) + _initialPosition;
                    if (y % 2 != 0)
                        position.x += 0.5f * gap;

                    int newBubbleType = 0;


                    if (level.Length <= levelPosition)
                    {
                        continue;
                    }
                    while (level[levelPosition] == '\r' || level[levelPosition] == '\n')
                    {
                        levelPosition++;
                    }

                    switch (level[levelPosition])
                    {
                        case '0':
                            levelPosition++;
                            continue;
                        case '1':
                            newBubbleType = 1;
                            break;
                        case '2':
                            newBubbleType = 2;
                            break;
                        case '3':
                            newBubbleType = 3;
                            break;
                        case '4':
                            newBubbleType = 4;
                            break;
                        case '5':
                            newBubbleType = 5;
                            break;
                    }

                    InitiateBubble(position, newBubbleType);
                    levelPosition++;
                }
                if (y % 2 != 0) _rows += 1;
            }
        }

        private string GenerateBubbleGrid(int columns, int rows)
        {
            string level = null;

            for (int i = 0; i < (columns * rows) + Random.Range(-5, 5); i++)
            {
                level += Random.Range(1, _bubblePrefab.BubbleSprites.Length).ToString();
            }

            return level;
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(_initialPosition.x + 2f, _initialPosition.y, 0), new Vector3(5, .1f, 1));
        }
#endif
    }
}