using UnityEngine;
using System.Collections;

public class SpawnBlocks : MonoBehaviour {
	private float timer;
	public static float blockAdvanceTime = .2f;
	private GameObject gridGO;
	public static int[,] gridActiveBlocks = new int[13,16];
	private bool movingBlockExists = false;
	private GameObject movingBlock;
	private bool logicCollision = false;
	private int movingBlockHeight; //0-12, starts at 3, hidden 0-2
	private int movingBlockWidth; //0-15
	private const int maxBlockPos = 9; //visible spectrum

	private GameObject[] arrayAtomicBlock = new GameObject[10];

	// Use this for initialization
	void Start () {
		//initialize timer
		timer = Time.time;
		gridGO = GameObject.FindGameObjectWithTag("Grid");
		Debug.Log ( gridGO.renderer.bounds.extents ) ;
		for (int i=0; i < arrayAtomicBlock.Length; i++){
			string loadMe = "Prefabs/block" + (i+2);
			arrayAtomicBlock[i] = (GameObject) Resources.Load ( loadMe );
		}
		for (int i=0; i <= gridActiveBlocks.GetUpperBound(0); i++){
			for (int j=0; j <= gridActiveBlocks.GetUpperBound(1); j++){
				gridActiveBlocks[i,j] = 0;
			}
		}
		DebugGrid();
	}

	void FixedUpdate(){

		//timer ticks
		float deltaTick = Time.time - timer;
		if (deltaTick > blockAdvanceTime){
			timer += deltaTick;
			PushBlockOut();
		}

		if (!movingBlockExists){
			Spawn();
		}

	}
	
	// Update is called once per frame
	void Update () {



		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//		if (Physics.Raycast(ray,out hit) && hit.transform.tag == "Grid"){
//			Debug.Log ( hit.point );
//		}
//		if (Physics.Raycast(ray)){
//			Debug.Log ( "any collision!" );
//		}


		//control blocks
		if (Input.GetMouseButtonDown(0))
		{
			//rotate left
			Debug.Log ("Left");
		}

		if (Input.GetMouseButtonDown(1))
		{
			//rotate left
			Debug.Log ("Right");
		}

		if (Input.GetAxis("Mouse ScrollWheel")!=0){
			//move around circularly
			Debug.Log ( Input.GetAxis("Mouse ScrollWheel") );
		}

	}

	void Spawn(){
		movingBlockHeight = 0;
		movingBlockWidth = 0;
		gridActiveBlocks[movingBlockHeight+3, movingBlockWidth] = 1;
		movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockHeight], new Vector3(0.8f, -2.1f, 0.1f), Quaternion.identity );
		movingBlockExists = true;
	}

	void PushBlockOut(){

		//check if logic collision happens
		bool condA = movingBlockHeight + 1 > maxBlockPos;  //WILL BE OUT OF BOUNDS

		//contrapositive applied here 
		if ( !condA && !(gridActiveBlocks[movingBlockHeight+1+3,movingBlockWidth] == 1 )) {  // condA is only used for it's short circuiting property
			logicCollision = false;
		}
		else {
			logicCollision = true;
			//lock block
			//??? delay ???
			Spawn ();
		}

		if (movingBlockExists && movingBlockHeight < maxBlockPos && !logicCollision){
			gridActiveBlocks[movingBlockHeight+3,movingBlockWidth] = 0;
			movingBlockHeight++;
			Vector3 blockPos = CalcBlockPos();
			Destroy(movingBlock);
			movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockHeight], blockPos, Quaternion.identity );
			gridActiveBlocks[movingBlockHeight+3,movingBlockWidth] = 1;
			DebugGrid();


		}
//		else if ( movingBlockHeight >= maxBlockPos ){
//			//lock block
//			//??? delay ???
//			Spawn ();
//		}
	}

	Vector3 CalcBlockPos(){
		float dd = 20.5f / 10.5f;
		float d = dd * (movingBlockHeight+1);
		float y = d * Mathf.Cos( 11.125f * Mathf.Deg2Rad );
		float x = d * Mathf.Sin( 11.125f * Mathf.Deg2Rad );
		float z = 0.1f;
		Debug.Log ( x + ", " + y);
		return new Vector3 (x,-y,z);
	}

	void DebugGrid(){
		string lines = "";
		for (int i=0; i <= gridActiveBlocks.GetUpperBound(0); i++){
			string line = "";
			for (int j=0; j <= gridActiveBlocks.GetUpperBound(1); j++){
				line += gridActiveBlocks[i,j] + " " ;
			}
			line += "\n";
			lines += line;
		}
		Debug.Log ( lines );
	}
	
}
