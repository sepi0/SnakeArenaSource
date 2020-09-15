using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour {
    public enum Direction
    {
        Up, Down, Left, Right, None
    }
    
    [SerializeField] private AudioSource soundSpeedup_1;
    [SerializeField] private AudioSource soundSpeedup_2;
    [SerializeField] private AudioSource soundStreak_1;
    [SerializeField] private AudioSource soundStreak_2;
    [SerializeField] private AudioSource soundStreak_3;
    [SerializeField] private AudioSource soundStreak_4;
    [SerializeField] private AudioSource pickUp_1;
    [SerializeField] private GameObject scoreAnimation;
    [SerializeField] private GameObject streakAnimation;
    [SerializeField] private GameObject chaserObject;
    public static bool CanMove;
    public static bool GameOver;
    public static bool adWasPlayed = false;
    public static bool MovingUp;
    public static bool MovingDown;
    public static bool MovingLeft;
    public static bool MovingRight;
    public static float Score;
    public static float HighScore;
    public static int ColorStreak;
    public static int CurrentColor;
    public static int NextColor;
    public static int Size;
    public static int ChaserInitialized;
    public static int WrongColorsPicked;
    public static int GamesPlayed;
    public static Vector3 PlayerPosition;


    private bool _canBoost;
    private bool _wasBoosted;
    private float _movementSpeed;
    private float _movementSpeedBoostDuration;
    private float _movementSpeedBoostCooldown;
    private float _movementSpeedMultiplier;

    private readonly List<GameObject> _chaserObjects = new List<GameObject>();
    private readonly List<Vector3> _previousPositions = new List<Vector3>();
    private readonly List<GameObject> _colorsCollected = new List<GameObject>();

    private void Awake()
    {
        _movementSpeed = 0.64f;
        _movementSpeedBoostDuration = 2.5f;
        _movementSpeedBoostCooldown = 2.5f;
        _movementSpeedMultiplier = 1f;
        CurrentColor = 0;
        NextColor = 1;
        Size = 1;
        WrongColorsPicked = 0;
        GameOver = true;
        ChaserInitialized = 0;
        LoadStats();
    }

    private void OnEnable() {
        Application.targetFrameRate = 60;
    }

    private void Start() {
        transform.position = Vector3.zero;
        StartCoroutine( GameManager.ActivateHelpers() );
    }

    private void Update() {
        PlayerPosition = transform.position;
        if( _colorsCollected.Count > 0 ) TrailNeonies();
        if( CanMove ) {
            if( Input.GetKeyDown( KeyCode.UpArrow ) ) {
                MovingDown = false;
                MovingUp = true;
                MovingLeft = false;
                MovingRight = false;
            }

            if( Input.GetKeyDown( KeyCode.DownArrow ) ) {
                MovingDown = true;
                MovingUp = false;
                MovingLeft = false;
                MovingRight = false;
            }

            if( Input.GetKeyDown( KeyCode.LeftArrow ) ) {
                MovingDown = false;
                MovingUp = false;
                MovingLeft = true;
                MovingRight = false;
            }

            if( Input.GetKeyDown( KeyCode.RightArrow ) ) {
                MovingDown = false;
                MovingUp = false;
                MovingLeft = false;
                MovingRight = true;
            }

            Move( Direction.Up );
            Move( Direction.Down );
            Move( Direction.Left );
            Move( Direction.Right );
            
            Teleport();
        }

        if( !_canBoost ) BoostCooldown();
        if( GameOver ) {
            CanMove = false;
            MovingDown = false;
            MovingUp = false;
            MovingLeft = false;
            MovingRight = false;

            SaveStats();
            DestroyTail();
            transform.position = Vector3.zero;
            foreach( var chaser in _chaserObjects ) {
                Destroy( chaser );
            }

            _chaserObjects.Clear();
            if( GamesPlayed > 0 && GamesPlayed % 3 == 0 && adWasPlayed == false ) {
                InitializeAdsScript.ShowInterstitialAd();
                adWasPlayed = true;
            }
        }
    }

    private void Teleport() {
        if( transform.position.x > GameManager.MapSize ) {
            var position = transform.position;
            transform.position = new Vector3( -GameManager.MapSize, position.y, position.z );
        }

        if( transform.position.x < -GameManager.MapSize ) {
            var position = transform.position;
            transform.position = new Vector3( GameManager.MapSize, position.y, position.z );
        }

        if( transform.position.y > GameManager.MapSize ) {
            var position = transform.position;
            transform.position = new Vector3( position.x, -GameManager.MapSize, position.z );
        }

        if( transform.position.y < -GameManager.MapSize ) {
            var position = transform.position;
            transform.position = new Vector3( position.x, GameManager.MapSize, position.z );
        }
    }

    private void Move(Direction direction) {
        switch( direction ) {
            case Direction.Up:
                if( MovingUp ) {
                    var position = transform.position;
                    Vector3 nextPosition =
                        new Vector3( position.x, position.y + ( _movementSpeed * _movementSpeedMultiplier ), -1f );
                    position = Vector3.MoveTowards( position, nextPosition,
                        30 * ( nextPosition.y - position.y ) * Time.deltaTime );
                    transform.position = position;
                    _previousPositions.Add( position );
                }        
                break;
            case Direction.Down:
                if( MovingDown ) {
                    var position = transform.position;
                    Vector3 nextPosition =
                        new Vector3( position.x, position.y - ( _movementSpeed * _movementSpeedMultiplier ), -1f );
                    position = Vector3.MoveTowards( position, nextPosition,
                        30 * ( position.y - nextPosition.y ) * Time.deltaTime );
                    transform.position = position;
                    _previousPositions.Add( position );
                }
                break;
            case Direction.Left:
                if( MovingLeft ) {
                    var position = transform.position;
                    Vector3 nextPosition =
                        new Vector3( position.x - ( _movementSpeed * _movementSpeedMultiplier ), position.y, -1f );
                    position = Vector3.MoveTowards( position, nextPosition,
                        30 * ( position.x - nextPosition.x ) * Time.deltaTime );
                    transform.position = position;
                    _previousPositions.Add( position );
                }
                break;
            case Direction.Right:
                if( MovingRight ) {
                    var position = transform.position;
                    Vector3 nextPosition =
                        new Vector3( position.x + ( _movementSpeed * _movementSpeedMultiplier ), position.y, -1f );
                    position = Vector3.MoveTowards( position, nextPosition,
                        30 * ( nextPosition.x - position.x ) * Time.deltaTime );
                    transform.position = position;
                    _previousPositions.Add( position );
                }
                break;
        }
    }

    private void TrailNeonies() {
        int j = 2;
        for( int i = 0; i < _colorsCollected.Count; i++ ) {
            if( _previousPositions.Count > 1 ) {
                _colorsCollected[i].transform.position = _previousPositions[_previousPositions.Count - j];
                j++;
            }

            if( _colorsCollected.Count > _previousPositions.Count ) {
                j = 2;
            }
        }
    }

    private void OnTriggerEnter2D( Collider2D other ) {
        bool isNextColor = other.gameObject.CompareTag( NextColor.ToString() );
        bool isBoost = other.gameObject.CompareTag( "Boost" );
        
        if( isNextColor ) {
            ColorStreak++;
            
            if( WrongColorsPicked > 0 )
                WrongColorsPicked--;

            if( ColorStreak % 5 == 0 && ColorStreak >= 5 ) {
                soundStreak_1.Play();
                StreakAnimation();
            }

            if( ColorStreak % 10 == 0 && ColorStreak >= 10 ) {
                soundStreak_2.Play();
                StreakAnimation();
            }

            if( ColorStreak % 15 == 0 && ColorStreak >= 15 ) {
                soundStreak_3.Play();
                StreakAnimation();
            }

            if( ColorStreak % 20 == 0 && ColorStreak >= 20 ) {
                soundStreak_4.Play();
                StreakAnimation();
            }

            CurrentColor++;
            NextColor++;
            Score += 100 * ( ( ColorStreak / 10 ) + 1 );
            DisplayBonus( ( 100 * ( ( ColorStreak / 10 ) + 1 ) ).ToString() );
            StartCoroutine( SpawnChasers( 1 ) );
        }
        
        if( !isNextColor && !isBoost ) {
            CurrentColor = Convert.ToInt32( other.tag );
            NextColor = CurrentColor + 1;
            WrongColorsPicked++;
            ColorStreak = 0;
            Score += 2;
            DisplayBonus( "+2" );
            StartCoroutine( SpawnChasers( 2 ) );
        }
        
        if( isBoost ) {
            if( _canBoost ) Boost();
        }
        
        if( CurrentColor > 47 ) CurrentColor = 0;
        if( NextColor > 47 ) NextColor = 0;
        pickUp_1.Play();
        Destroy( other.gameObject );
        AddTailNeonie( other );
        GameManager.InitializedObjects.Remove( other.gameObject );
        StartCoroutine( GameManager.ActivateHelpers() );
    }

    private void OnCollisionEnter2D( Collision2D other ) {
        if( other.gameObject.CompareTag( "Chaser" ) ) {
            GameOver = true;
        }
    }

    private void AddTailNeonie( Collider2D other ) {
        GameObject instance = Instantiate( other.gameObject, _previousPositions[_previousPositions.Count - Size],
            Quaternion.identity );
        instance.transform.localScale = new Vector3( 1f, 1f, 1f );
        instance.transform.parent = transform;
        instance.GetComponent<BoxCollider2D>().enabled = false;
        instance.tag = "Untagged";
        Destroy( instance.transform.GetChild( 0 ).gameObject );
        _colorsCollected.Add( instance );
        Size++;
    }

    private void Boost() {
        soundSpeedup_1.Play();
        _movementSpeedMultiplier = 2f;
        _canBoost = false;
        _wasBoosted = true;
    }

    private void BoostCooldown() {
        _movementSpeedBoostCooldown -= Time.deltaTime;
        if( _wasBoosted ) {
            _movementSpeedBoostDuration -= Time.deltaTime;
            if( _movementSpeedBoostDuration < 0 ) {
                _movementSpeedBoostDuration = 2.5f;
                _movementSpeedMultiplier = 1f;
                _wasBoosted = false;
            }
        }

        if( _movementSpeedBoostCooldown < 0 ) {
            _movementSpeedBoostCooldown = 2.5f;
            _canBoost = true;
        }
    }

    private void DisplayBonus( string number ) {
        scoreAnimation.GetComponent<TextMeshPro>().text = number;
        Instantiate( scoreAnimation, transform.position, Quaternion.identity );
    }

    private void StreakAnimation() {
        streakAnimation.GetComponent<TextMeshPro>().text = "color streak x" + ColorStreak;
        Instantiate( streakAnimation, transform.position, Quaternion.identity );
    }

    private void SaveStats() {
        PlayerPrefs.SetFloat( "HighScore", Score );
        PlayerPrefs.SetInt( "GamesPlayed", GamesPlayed );
    }

    private void LoadStats() {
        HighScore = PlayerPrefs.GetFloat( "HighScore", 0 );
        GamesPlayed = PlayerPrefs.GetInt( "GamesPlayed", 0 );
    }

    private void DestroyTail() {
        foreach( Transform child in transform ) {
            if( child.CompareTag( "Light" ) )
                continue;
            Destroy( child.gameObject );
        }

        foreach( GameObject element in _colorsCollected ) {
            Destroy( element );
        }

        _colorsCollected.Clear();
    }

    IEnumerator SpawnChasers( int howMany ) {
        yield return new WaitForSeconds( 1f );
        for( int i = 0; i < howMany; i++ ) {
            int randomNumber = Random.Range( 0, GameManager.Positions.Count );
            var instance = Instantiate( chaserObject, GameManager.Positions[randomNumber], Quaternion.identity );
            _chaserObjects.Add( instance );
            ChaserInitialized++;
            yield return new WaitForSeconds( 0.25f );
        }
    }
}