using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : MonoBehaviour {
    private void Update() {
        transform.position = Vector3.MoveTowards( transform.position, Player.PlayerPosition,
            Time.deltaTime * ( 15 + Player.WrongColorsPicked ) );
        if( Player.GameOver )
            Destroy( gameObject );
    }
}