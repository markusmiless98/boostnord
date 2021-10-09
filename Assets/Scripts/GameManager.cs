using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameMode {
    public string name;
    public Sprite icon;
    public VehicleManager.VehicleOption[] vehicles;
}

public class GameManager : MonoBehaviour {

    public bool paused = true;
    bool inGame = false;
    bool musicEnabled = true; // TODO PLAYER PREFS
    public VehicleManager vm;

    public GameMode[] gamemodes;
    public AudioSource music;

    public Transform gameModeButtons;
    public GameObject gameModeButtonPrefab;
    public GameObject mainMenu;

    public CameraController cameraController;

    public GameObject pauseMenu;
    public Image musicIcon;

    public Sprite musicOn, musicOff;

    public GameObject endScreen;
    public Transform leafs, stars;
    public Color32 leafColor, starColor, disabledScoreColor;
    public Text scoreText;

    public GameObject startScreen;

    GameMode selectedGameMode = null;

    [System.Serializable]
    public class Highscore {
        public int score;
        public string gamemode;
    }
    [System.Serializable]
    public class HighscoreList {
        public Highscore[] highscores;
    }

    Dictionary<string, int> highscores = new Dictionary<string, int>();

    void SetMainMenuDisplay(bool visible) {
        mainMenu.SetActive(visible);
    }
    void SetGameModeButtonsDisplay(bool visible) {
        gameModeButtons.gameObject.SetActive(visible);
    }

    void Start() {

        if (PlayerPrefs.HasKey("music")) {
            musicEnabled = PlayerPrefs.GetInt("music") == 1 ? true : false;
        }

        if (PlayerPrefs.HasKey("highscores")) {

            HighscoreList rawScores = JsonUtility.FromJson<HighscoreList>(PlayerPrefs.GetString("highscores"));
            foreach (Highscore highscore in rawScores.highscores) {
                highscores.Add(highscore.gamemode, highscore.score);
            }
        }

        ReloadGamemodeButtons();
        SetGameModeButtonsDisplay(true);
        GoToMainMenu();

        UpdateMusicState();
        ShowStartScreen();
    }

    public void ShowStartScreen() {
        startScreen.SetActive(true);
    }

    public void HideStartScreen() {
        startScreen.SetActive(false);
    }

    void SaveHighScores() {
        HighscoreList rawScores = new HighscoreList();
        rawScores.highscores = new Highscore[highscores.Count];
        int index = 0;
        foreach (KeyValuePair<string, int> score in highscores) {
            Highscore rawScore = new Highscore();
            rawScore.gamemode = score.Key;
            rawScore.score = score.Value;
            rawScores.highscores[index] = rawScore;
            index++;
        }
        string json = JsonUtility.ToJson(rawScores);

        PlayerPrefs.SetString("highscores", json);
    }

    void SetMusicIcon() {
        musicIcon.sprite = musicEnabled ? musicOn : musicOff;
    }

    public void GoToMainMenu() {
        ClosePauseMenu();
        SetMainMenuDisplay(true);
        HideEndScreen();
        inGame = false;
        paused = true;
    }

    public void RestartGame() {
        ClosePauseMenu();
        StartGame(selectedGameMode);
    }

    public void OpenPauseMenu() {
        if (!inGame) return;
        paused = true;
        pauseMenu.SetActive(true);
    }

    public void ClosePauseMenu() {
        paused = false;
        pauseMenu.SetActive(false);
    }

    public void ToggleMusic() {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("music", musicEnabled ? 1 : 0);
        SetMusicIcon();
        UpdateMusicState();
    }

    public void UpdateMusicState() {
        music.mute = !musicEnabled;
    }

    void Update() {
        if (inGame) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (paused) ClosePauseMenu();
                else OpenPauseMenu();
            }
        }
        if (Input.anyKey) {
            HideStartScreen();
        }
    }

    public void EndGame() {
        vm.OnEndGame();
        ShowEndScreen();
        paused = true;
        inGame = false;

        float emissions = SetIconScore(leafs, vm.GetEmissionScore(), leafColor, disabledScoreColor);
        float reviews = SetIconScore(stars, vm.GetRatingsScore(), starColor, disabledScoreColor);
        int score = (int)Mathf.Round(emissions * reviews);

        scoreText.text = score.ToString();
        SetHighScore(selectedGameMode.name, score);
        ReloadGamemodeButtons();
    }

    float SetIconScore(Transform parent, float score, Color32 activeColor, Color32 disabledColor) {
        score = 100f - score;
        for (int i = 0; i < parent.childCount; i++)
            parent.GetChild(i).GetComponent<Image>().color = score >= ((i + 1) * 100 / parent.childCount) ? activeColor : disabledColor;
        return score >= 0 ? score : 0;
    }

    void HideEndScreen() {
        endScreen.SetActive(false);
    }

    void ShowEndScreen() {
        endScreen.SetActive(true);
    }

    void SetHighScore(string gamemode, int score) {
        if (highscores.ContainsKey(gamemode) && highscores[gamemode] > score) return;
        highscores[gamemode] = score;
        SaveHighScores();
    }

    int GetHighscore(string gamemode) {
        if (highscores.ContainsKey(gamemode)) return highscores[gamemode];
        return -1;
    }

    void ReloadGamemodeButtons() {
        while (gameModeButtons.childCount > 0) DestroyImmediate(gameModeButtons.GetChild(0).gameObject);
        foreach (GameMode gamemode in gamemodes) {
            Transform button = Instantiate(gameModeButtonPrefab, gameModeButtons).transform;

            button.Find("Icon").GetComponent<Image>().sprite = gamemode.icon;
            button.Find("Name").GetComponent<Text>().text = gamemode.name;
            int highscore = GetHighscore(gamemode.name);
            button.Find("Score").GetComponent<Text>().text = highscore == -1 ? "-" : highscore.ToString();
            button.GetComponent<Button>().onClick.AddListener(() => {
                StartGame(gamemode);
            });
        }
    }

    void StartGame(GameMode gamemode) {
        inGame = true;
        selectedGameMode = gamemode;
        SetMainMenuDisplay(false);
        HideEndScreen();
        vm.timer = 90f;
        paused = false;

        vm.NewGame(gamemode.vehicles);
        cameraController.ResetValues();
    }
}
