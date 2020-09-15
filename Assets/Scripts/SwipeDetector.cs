using System;
using UnityEngine;

public struct SwipeData {
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
}

public enum SwipeDirection {
    Up,
    Down,
    Left,
    Right
}

public class SwipeDetector : MonoBehaviour
{
    private Vector2 _fingerUpPosition;
    private Vector2 _fingerDownPosition;
    private bool _detectSwipeOnlyAfterRelease = false;
    private float _minDistanceForSwipe = 20f;
    private SwipeData _swipeData;
    public static event Action<SwipeData> OnSwipe = delegate { };
    
    void Update()
    {
        foreach( Touch touch in Input.touches ) {
            if( touch.phase == TouchPhase.Began ) {
                _fingerUpPosition = touch.position;
                _fingerDownPosition = touch.position;
            }

            if( !_detectSwipeOnlyAfterRelease && touch.phase == TouchPhase.Moved ) {
                _fingerDownPosition = touch.position;
                DetectSwipe();
            }

            if( touch.phase == TouchPhase.Ended ) {
                _fingerDownPosition = touch.position;
                DetectSwipe();
            }
        }
    }
    
    private void DetectSwipe() {
        if( SwipeDistanceCheckMet() ) {
            if( IsVerticalSwipe() ) {
                var direction = _fingerDownPosition.y - _fingerUpPosition.y > 0
                    ? SwipeDirection.Up
                    : SwipeDirection.Down;
                if( direction == SwipeDirection.Up ) {
                     Player.MovingUp = true;
                     Player.MovingDown = false;   
                     Player.MovingLeft = false;
                     Player.MovingRight = false;

                }
                else {
                    Player.MovingUp = false;
                    Player.MovingDown = true;   
                    Player.MovingLeft = false;
                    Player.MovingRight = false;
                }

                SendSwipe( direction );
            }
            else {
                var direction = _fingerDownPosition.x - _fingerUpPosition.x > 0
                    ? SwipeDirection.Right
                    : SwipeDirection.Left;
                if( direction == SwipeDirection.Left ) {
                    Player.MovingUp = false;
                    Player.MovingDown = false;   
                    Player.MovingLeft = true;
                    Player.MovingRight = false;
                }
                else {
                    Player.MovingUp = false;
                    Player.MovingDown = false;   
                    Player.MovingLeft = false;
                    Player.MovingRight = true;
                }

                SendSwipe( direction );
            }
        }
    }


    private bool IsVerticalSwipe() {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private bool SwipeDistanceCheckMet() {
        return VerticalMovementDistance() > _minDistanceForSwipe ||
               HorizontalMovementDistance() > _minDistanceForSwipe;
    }

    private float VerticalMovementDistance() {
        return Mathf.Abs( _fingerDownPosition.y - _fingerUpPosition.y );
    }

    private float HorizontalMovementDistance() {
        return Mathf.Abs( _fingerDownPosition.x - _fingerUpPosition.x );
    }

    private void SendSwipe( SwipeDirection direction ) {
        SwipeData swipeData = new SwipeData() {
            Direction = direction,
            StartPosition = _fingerDownPosition,
            EndPosition = _fingerUpPosition
        };
        OnSwipe( swipeData );
    }

}
