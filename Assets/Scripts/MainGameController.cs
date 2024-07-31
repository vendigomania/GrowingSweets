using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameController : MonoBehaviour
{
    [SerializeField] private Image BackgroundImg;
    [SerializeField] private Sprite[] backgrounds;

    [SerializeField] private GameObject startScreen;

    [SerializeField] private GameObject modesScreen;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestText;

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private Text resultText;
    [SerializeField] private Text resultStat;

    [SerializeField] private GameObject optionScreen;
    [SerializeField] private GameObject againBtn;

    [Header("ingame")]
    [SerializeField] private LoseTrigger loseLine;

    [SerializeField] private GameObject gameRoot;
    [SerializeField] private List<BallItem> balls = new List<BallItem>();
    [SerializeField] private Transform playerRoot;
    [SerializeField] private BallItem currentBall;
    [SerializeField] private BallItem nextBall;
     
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private ParticleSystem burst;
    [SerializeField] private GateSkinController gateSkinController;

    public int LastScore
    {
        get => PlayerPrefs.GetInt("Score", 0);
        set => PlayerPrefs.SetInt("Score", value);
    }

    public int Best
    {
        get => PlayerPrefs.GetInt($"Best{GameMode}", 0);
        set => PlayerPrefs.SetInt($"Best{GameMode}", value);
    }

    public int tempScore;
    int GameMode = 0;

    // Start is called before the first frame update
    void Start()
    {
        BallItem.OnCollision += OnBallCollision;
        ControlScreen.OnShot += Shot;

        loseLine.OnLose += () => ShowResult(false);
    }

    public void StartGame() => StartGame(GameMode);

    public void StartGame(int mode)
    {
        if (gateRemainTime > 0) return;

        GameMode = mode;

        SoundController.Instance.Click();

        startScreen.SetActive(false);

        SetOptionsVisible(false);

        resultScreen.SetActive(false);

        tempScore = 0;
        scoreText.text = LastScore.ToString();
        bestText.text = Best.ToString();

        gameRoot.SetActive(true);

        BackgroundImg.sprite = backgrounds[Random.Range(0, backgrounds.Length)];

        currentBall.SetValue(Random.Range(0, 2));
        nextBall.SetValue(Random.Range(0, 2));

        gateAnimator.Play("Close");
        gateSkinController.SetRandomSkin();

        //Additional modes
        timerRoot.SetActive(GameMode == 1);
        collectModeRoot.SetActive(GameMode == 2);
        
        if(GameMode == 1) //Timer
        {
            StopAllCoroutines();
            StartCoroutine(Timer());
        }
        else if(GameMode == 2)
        {
            for(int i = 0; i < counters.Length; i++)
            {
                counters[i] = 0;
            }
            ShowCounters();
        }
    }

    public void BackToMenu()
    {
        startScreen.SetActive(true);
        gameRoot.SetActive(false);
        resultScreen.SetActive(false);

        foreach(var ball in balls) ball.gameObject.SetActive(false);

        SoundController.Instance.Click();
    }

    public void SetOptionsVisible(bool visible)
    {
        optionScreen.SetActive(visible);
        againBtn.SetActive(gameRoot.activeSelf);

        SoundController.Instance.Click();
    }
    private void Shot()
    {
        var ball = balls.FindLast(b => !b.gameObject.activeSelf);

        if(ball == null)
        {
            ball = Instantiate(balls[0].gameObject, playerRoot.position, Quaternion.identity, gameRoot.transform)
                .GetComponent<BallItem>();
            
            balls.Add(ball);
        }

        loseLine.SetDelay();

        ball.SetValue(currentBall.Value);
        currentBall.SetValue(nextBall.Value);
        nextBall.SetValue(Random.Range(0, 3));

        ball.transform.position = playerRoot.position;
        ball.gameObject.SetActive(true);

        SoundController.Instance.Click();
    }

    private void OnBallCollision(BallItem one, BallItem two)
    {
        if (!one.gameObject.activeSelf) return;

        var whoLast = balls.FindLast(ball => ball == one || ball == two);

        if (whoLast == one)
            two.gameObject.SetActive(false);
        else
            one.gameObject.SetActive(false);


        burst.transform.position = whoLast.transform.position;
        burst.Play();

        tempScore++;
        if (GameMode == 0)
        {
            whoLast.SetValue(whoLast.Value + 1);

            scoreText.text = (LastScore + tempScore).ToString();

            if (whoLast.Value == 5)
            {
                ShowResult(true);

                gateAnimator.Play("Open");
                gateRemainTime = 2f;
            }
        }
        else
        {
            counters[whoLast.Value]++;

            if (GameMode == 2)
            {
                if(tempScore > Best)
                {
                    Best = tempScore;
                    bestText.text = Best.ToString();
                }

                ShowCounters();
            }
            
            if(whoLast.Value == 5)
            {
                whoLast.gameObject.SetActive(false);
            }
            else
            {
                whoLast.SetValue(whoLast.Value + 1);
            }

            scoreText.text = (tempScore).ToString();
        }

        SoundController.Instance.React();
    }

    private void ShowResult(bool win)
    {
        SoundController.Instance.Win();

        resultScreen.SetActive(true);
        if (win)
        {
            LastScore += tempScore;


            if (LastScore > Best)
            {
                resultText.text = $"New Record";
                resultStat.text = $"Score {tempScore}";
                Best = LastScore;
            }
            else
            {
                resultText.text = $"Check Point Achieved\n";
                resultStat.text = $"Get score {tempScore}";
            }
        }
        else
        {
            LastScore = 0;

            if (GameMode > 0)
            {
                if(tempScore > Best)
                {
                    resultText.text = "New record";
                    resultStat.text = $"Score: {tempScore}";
                }
                else
                {
                    resultText.text = "Game is End";
                    resultStat.text = $"Best: {Best}";
                }
            }
            else
            {
                resultText.text = "You Lose";
                resultStat.text = $"Best: {Best}";
            }

            foreach (var ball in balls)
            {
                ball.gameObject.SetActive(false);
            }
        }
        tempScore = 0;
    }

    float gateRemainTime;
    private void Update()
    {
        if (gateRemainTime > 0f)
        {
            gateRemainTime -= Time.deltaTime;

            if(gateRemainTime <= 0f)
            {
                foreach(var ball in balls)
                {
                    ball.gameObject.SetActive(false);
                }
            }
        }
    }

    #region Timer

    [Header("Timer")]
    [SerializeField] private GameObject timerRoot;
    [SerializeField] private Text timerLable;

    IEnumerator Timer()
    {
        int remainTime = 120;
        while(remainTime > 0)
        {
            if(!optionScreen.activeSelf)
            {
                remainTime--;
                timerLable.text = System.TimeSpan.FromSeconds(remainTime).ToString(@"mm\:ss");
                Debug.Log($"Timer {remainTime}");
            }

            if(startScreen.activeSelf) yield break;

            yield return new WaitForSeconds(1f);
        }

        ShowResult(false);
    }

    #endregion

    #region Collectable

    [Header("Collectables")]
    [SerializeField] private GameObject collectModeRoot;
    [SerializeField] private Text[] collectCountersLables;

    int[] counters = new int[6];

    private void ShowCounters()
    {
        for(int i = 0; i < counters.Length; i++)
        {
            collectCountersLables[i].text = $"x{counters[i]}";
        }
    }

    #endregion
}
