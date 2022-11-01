using BubbleShooter;
using TMPro;
using UnityEngine;

public class UIScoreBoard : MonoBehaviour
{
    [SerializeField] private BubbleGroupController _bubbleGroupController;
    [SerializeField] private TMP_Text _scoreText;

    private void Start()
    {
        _bubbleGroupController.OnScoreChange += ChangeScore;
    }

    private void ChangeScore()
    {
        _scoreText.text = $"Score: {_bubbleGroupController.Points}";
    }
}
