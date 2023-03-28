using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public bool inPlay = false;
    bool gameOver = false;

    public int scoreOne;
    public int scoreTwo;
    [SerializeField] int scoreToWin;

    public Text textOne;
    public Text textTwo;

    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Text winnerText;

    private void OnEnable()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update () {
        Winner();
    }

    void Winner()
    {
        if(!gameOver)
        {
            if(scoreOne >= scoreToWin)
            {
                gameOver = true;
                winnerText.text = "Player 1 Wins";
                gameOverPanel.SetActive(true);
            }
            if(scoreTwo >= scoreToWin)
            {
                gameOver = true;
                winnerText.text = "Player 2 Wins";
                gameOverPanel.SetActive(true);
            }
        }
    }
}