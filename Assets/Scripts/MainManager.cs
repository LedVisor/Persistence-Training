using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MainManager : MonoBehaviour
{
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;
    public GameObject Paddle;

    public Text ScoreText, LevelText;
    public Text HighScoreText;

    public int brickCount;

    public GameObject GameOverText, StartText;

    private bool m_Started = false;
    private int m_Points;
    
    private bool m_GameOver = false;
    public Vector3 initialPos;



    private void Awake()
    {
        if(GameManager.Instance == null) // game not started from menu in editor 
        {
            SceneManager.LoadScene(0); // go to Menu
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.wave = 1;
        initialPos = Ball.transform.localPosition; // initial offset from paddle
        Setup(GameManager.Instance.wave);
        ShowHighScore();
        GameOverText.SetActive(false);
    }

    private void Setup(int wave)
    {
        const float initialStep = 0.6f;
        float step = initialStep + (wave-1) * 0.2f;
        int perLine = Mathf.FloorToInt(4.0f / step);

        int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
        brickCount = 0;
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
                brickCount++;
            }
        }

        StartText.SetActive(true);
        LevelText.text = "Level " + wave;
        LevelText.enabled = true;
    }

    private void ShowHighScore()
    {
        HighScoreText.text = $"High Score: {GameManager.Instance.highScore}   Name: { GameManager.Instance.highScoreName}";
    }

    private void Update()
    {
        if (!m_Started)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Started = true;
                float randomDirection = Random.Range(-1.0f, 1.0f);

               
                Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                forceDir.Normalize();

                Ball.transform.SetParent(null);
                Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
                StartText.SetActive(false);
                LevelText.enabled = false;

            }
        }
        else if (m_GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(0); // Menu
            }

        }
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"Score : {m_Points}";
        brickCount--;
        if(brickCount ==0) // end of wave
        {
            m_Started = false;
            attachBallToPaddle();
            GameManager.Instance.wave++;
            Setup(GameManager.Instance.wave);
        }
    }

    void attachBallToPaddle()
    {
        if (Ball.transform.parent ==null) // has been in play
        {
            Ball.velocity = Vector3.zero; // keep ball still
            Ball.angularVelocity = Vector3.zero;
            Ball.transform.SetParent(Paddle.transform);
            //Ball.transform.SetPositionAndRotation(Paddle.transform.position + initialPos, Quaternion.identity);
            Ball.transform.SetPositionAndRotation(Paddle.transform.position, Quaternion.identity);
        }
    }

    public void GameOver()
    {
        m_GameOver = true;
        GameOverText.SetActive(true);

        // update highscore values
        if(m_Points > GameManager.Instance.highScore)
        {
            GameManager.Instance.highScore = m_Points;
            GameManager.Instance.highScoreName = GameManager.Instance.playerName;
            GameManager.Instance.SaveGameData();
            ShowHighScore();
        }
    }



}
