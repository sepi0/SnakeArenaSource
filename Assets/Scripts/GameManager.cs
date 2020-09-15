using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject colorPrefab;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject inGameUi;
    [SerializeField] private GameObject gameOverUi;
    [SerializeField] private GameObject mainMenuUi;
    [SerializeField] private TextMeshProUGUI heat;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI highScore;
    [SerializeField] private TextMeshProUGUI finalScore;
    [SerializeField] private TextMeshProUGUI pauseResumeText;
    [SerializeField] private GameObject ingameQuitButton;
    public static int MapSize = 500;
    public static List<Vector3> Positions;
    public static List<List<Vector3>> InitializedPositions = new List<List<Vector3>>();
    public static List<GameObject> InitializedObjects = new List<GameObject>();
    private bool gamePaused = false;

    private int foodCount = MapSize / 10;
    private void Awake() {
        Positions = GeneratePositions();
        InitializedPositions = InitializePositions();
        mainMenuUi.SetActive( true );
    }

    private void Update() {
        if( Player.GameOver && !mainMenuUi.gameObject.activeSelf ) {
            inGameUi.SetActive( false );
            gameOverUi.SetActive( true );
        }

        heat.text = Player.ChaserInitialized.ToString();
        score.text = Player.Score.ToString();
        highScore.text = "High Score: " + Player.HighScore;
        finalScore.text = "Final Score: " + Player.Score;
    }

    private List<Vector3> GeneratePositions() {
        List<Vector3> tempList = new List<Vector3>();
        for( int x = -MapSize; x < MapSize; x += 5 ) {
            for( int y = -MapSize; y < MapSize; y += 5 ) {
                var xFinal = 1 * x;
                var yFinal = 1 * y;
                tempList.Add( new Vector3( xFinal, yFinal, -1f ) );
            }
        }
        return tempList;
    }

    private List<List<Vector3>> InitializePositions() {
        List<List<Vector3>> allColors = new List<List<Vector3>>();
        for( int spriteIndex = 0; spriteIndex < sprites.Length; spriteIndex++ ) {
            allColors.Add( InitializeColor( spriteIndex ) );
        }

        return allColors;
    }

    private List<Vector3> InitializeColor( int spriteIndex ) {
        List<Vector3> singleColor = new List<Vector3>();
        for( int count = 0; count < foodCount; count++ ) {
            int randomNumber = Random.Range( 10, Positions.Count );
            Vector3 randomVector = Positions[randomNumber];

            GameObject instance = Instantiate( colorPrefab, randomVector, Quaternion.identity );

            Positions.Remove( randomVector );

            instance.GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
            instance.name = spriteIndex.ToString();
            instance.tag = spriteIndex.ToString();
            instance.transform.localScale = new Vector3( 2f, 2f, 1f );

            InitializedObjects.Add( instance );
            singleColor.Add( randomVector );
        }

        return singleColor;
    }

    public void PlayAgain() {
        inGameUi.SetActive( true );
        gameOverUi.SetActive( false );
        mainMenuUi.SetActive( false );

        foreach( var foodObject in InitializedObjects ) {
            Destroy( foodObject );
        }

        InitializedObjects.Clear();
        InitializedPositions.Clear();

        Positions = GeneratePositions();
        InitializedPositions = InitializePositions();

        Player.GameOver = false;
        Player.CanMove = true;
        Player.CurrentColor = 0;
        Player.NextColor = 1;
        Player.Score = 0;
        Player.ColorStreak = 0;
        Player.Size = 1;
        Player.ChaserInitialized = 0;
        Player.WrongColorsPicked = 0;
        Player.adWasPlayed = false;
        Player.GamesPlayed++;
        StartCoroutine( ActivateHelpers() );
    }

    public static IEnumerator ActivateHelpers() {
        foreach( var helper in InitializedObjects ) {
            helper.transform.GetChild( 0 ).gameObject.SetActive( true );
            if( !helper.CompareTag( Player.NextColor.ToString() ) ) {
                helper.transform.GetChild( 0 ).gameObject.SetActive( false );
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void PauseResumeGame() {
        gamePaused = !gamePaused;
        if( gamePaused ) {
            Time.timeScale = 0;
            gamePaused = true;
            pauseResumeText.text = "Resume";
            ingameQuitButton.SetActive( true );
        }
        else {
            Time.timeScale = 1;
            gamePaused = false;
            pauseResumeText.text = "Pause";
            ingameQuitButton.SetActive( false );
        }
    }
}