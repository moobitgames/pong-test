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
        if(inPlay == false && gameOver != true)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                inPlay = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
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

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}