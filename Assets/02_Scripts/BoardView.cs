using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] Text _scoreText;
    [SerializeField] Image _feverTimeBar;

    int _score = 0;

    public void Initialize()
    {
        _score = 0;
        _scoreText.text = $"Score : {_score}";
        _feverTimeBar.fillAmount = 0f;
    }

    public void UpdateScore(int removedTileCount)
    {
        _scoreText.text = $"Score : {_score += 10}";

        _feverTimeBar.fillAmount = removedTileCount * 0.05f;
    }

    public void DecreaseFeverTimeBar()
    {
        _feverTimeBar.fillAmount -= Time.deltaTime * 0.1f;
    }

    public void ResetFeverTimeBar()
    {
        _feverTimeBar.fillAmount = 0f;
    }
}
