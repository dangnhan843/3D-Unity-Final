using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameUI : MonoBehaviour
{
    public GameObject pauseMenuHolder;
    public GameObject newWaveBannerHolder;
    public GameObject panelHolder;
    public GameObject optionMenuHolder;
    public GameObject gameOverUI;
    public GameObject newHighScoreText;
    public GameObject oldHighScoreText;
    public GameObject gameOverHS;
    public Animator hsAnim;
    public Animator hsGOAnim;
    public Text newHSText;
    public Image fadePlane;
    public RectTransform newWaveBanner;
    public Text newWaveTile;
    public Text newWaveEnemyCount;
    public Text scoreUI;
    public Text highScoreUI;
    public Text gameOverScoreUI;
    public RectTransform healthBar;
    public GameObject pauseMenuUI;
    public static bool GameIsPaused = false;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;
    public Slider[] volumeSliders;
    bool run = true;
    int tempHighScore;
    Player player;
    Spawner spawner;
    int activeScreenResIndex;
    bool hs = false;


    void Start()
    {
        player = FindObjectOfType<Player>();
            player.OnDeath += OnGameOver;
        
        highScoreUI.text = PlayerPrefs.GetInt("HighScore", 0).ToString("D6");
        newHighScoreText.SetActive(false);
        gameOverHS.gameObject.SetActive(false);
        oldHighScoreText.SetActive(true);
    }
    private void Update()
    {
        if (gameOverUI)
        {
            if (run)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (GameIsPaused)
                    {
                        Resume();
                    }
                    else
                    {
                        Pause();
                    }
                }
            }
        }
        scoreUI.text = ScoreKeeper.score.ToString("D6");
        if (ScoreKeeper.score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", ScoreKeeper.score);
            highScoreUI.text = ScoreKeeper.score.ToString("D6");
            hsAnim.enabled = true;
            hs = true;
            NewHighScoreText();
            ColorChange();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetHighScore();
        }
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;
            
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
        
    }

    public void Resume()
    {
        newWaveBannerHolder.SetActive(true);
        pauseMenuUI.SetActive(false);
        panelHolder.SetActive(false);
        Time.timeScale = 1.0f;
        GameIsPaused = false;

    }
    public void Pause()
    {
        newWaveBannerHolder.SetActive(false);
        pauseMenuUI.SetActive(true);
        panelHolder.SetActive(true);
        Time.timeScale = 0.0f;
        GameIsPaused = true;
    }
    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }
    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One" , "Two","Three" ,"Four" ,"Five"};
        newWaveTile.text = "- Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "Infinite":
            spawner.waves[waveNumber - 1].enemyCount+"");
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }
    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 1.5f;
        float speed = 3.0f;
        float animatePercent = 0.0f;
        float endDelayTime = Time.time + 1 / speed + delayTime;
        int dir = 1;
        
        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;
            {
                if (animatePercent >=1)
                {
                    animatePercent = 1;
                    if (Time.time > endDelayTime)
                    {
                        dir = -1;
                    }

                }
                newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-400,
                    45, animatePercent);
                yield return null;
            }
        }
    }

    void OnGameOver()
    {
        run = false;
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, new Color (0,0,0,0.95f), 1));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        highScoreUI.gameObject.SetActive(false);
        newHighScoreText.gameObject.SetActive(false);
        oldHighScoreText.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
        if (hs)
        {

            gameOverHS.gameObject.SetActive(true);
            hsGOAnim.enabled = true;
        }

    }

    IEnumerator Fade (Color from, Color to , float time)
    {
        float speed = 1.0f / time;
        float percent = 0.0f;
        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame ()
    {
        SceneManager.LoadScene(1);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {

        pauseMenuHolder.SetActive(false);
        optionMenuHolder.SetActive(true);
    }

    public void ReturnPauseMenu()
    {
        pauseMenuHolder.SetActive(true);
        optionMenuHolder.SetActive(false);
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn)
        {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !isFullscreen;
        }

        if (isFullscreen)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else
        {
            SetScreenResolution(activeScreenResIndex);
        }

        PlayerPrefs.SetInt("fullscreen", ((isFullscreen) ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value)
    {
        
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.SFX);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
    }
    public void NewHighScoreText()
    {
        newHighScoreText.SetActive(true);
        oldHighScoreText.SetActive(false);
    }
    public void ColorChange()
    {
        
    }
}
