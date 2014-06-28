using UnityEngine;
using System.Collections;

public class TEST_DESTROY_ARRAY_WORKS : MonoBehaviour {

	GameObject go; 
	GameObject[] gos;

	void Start () {
		go = GameObject.Find ( "block3" );

		//these evil lines freeze unity lol
		//if ( go ) { Debug.Log ( "yes?" ); } 
		//else { Debug.Log ( "no?" ); }

		gos = new GameObject[3];
		gos[0] = GameObject.Find ("block4");
	}
	
	void Update () {

		if (go != null) {Destroy ( go ); }
		if (gos[0] != null ) { Destroy ( gos[0] ); } 
		Destroy( gos[1] );
	
	}
}
