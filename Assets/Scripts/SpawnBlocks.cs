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
	private int movingBlockPosY; //0-12, starts at 3, hidden 0-2
	private int movingBlockPosX; //0-15
	private const int maxBlockPosY = 9; //visible spectrum
	private const int maxBlockPosX = 15;
	private bool roomToSpawnExists = true;
	private int spawnPosX = 0;
	private int spawnPosY = 0;

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
		spawnPosX = 0;
		spawnPosY = 0;

		//DebugGrid();
	}

	void FixedUpdate(){

		//timer ticks
		float deltaTick = Time.time - timer;
		if (deltaTick > blockAdvanceTime){
			timer += deltaTick;
			PushBlockOut();
		}

		if (!movingBlockExists & roomToSpawnExists){
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
			//rotate right
			Debug.Log ("Right");
		}

		//scroll up
		if (Input.GetAxis("Mouse ScrollWheel") > 0){
			//move ccw
			Debug.Log ("+");
			PushBlockSideways(true);
		}

		//scroll down
		else if (Input.GetAxis("Mouse ScrollWheel") < 0){
			//move cw
			Debug.Log ("-");
			PushBlockSideways(false);
		}

	}

	bool Spawn(){
		movingBlockPosY = 0;
		movingBlockPosX = 0;

		if (gridActiveBlocks[spawnPosY+3, spawnPosX] == 0){

			gridActiveBlocks[spawnPosY+3, spawnPosX] = 1;
			Vector3 blockPos = CalcBlockRenderPos();
			Quaternion blockRot = Quaternion.identity * Quaternion.Euler(0f,0f, 22.5f * movingBlockPosX);
			movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockPosY], blockPos, blockRot );
			movingBlockExists = true;
			return true;
		}
		else 
			return false;
	}

	void PushBlockOut(){

		//check if logic collision happens
		bool condA = movingBlockPosY + 1 > maxBlockPosY;  //WILL BE OUT OF BOUNDS

		//contrapositive applied here 
		if ( !condA && !(gridActiveBlocks[movingBlockPosY+1+3,movingBlockPosX] == 1 )) {  // condA is only used for it's short circuiting property
			logicCollision = false;
		}
		else {
			logicCollision = true;
			//lock block
			//??? delay ???

			//check if room
			roomToSpawnExists = false;

			Spawn ();
		}

		//performs so called block movement, if allowed
		if (movingBlockExists && movingBlockPosY < maxBlockPosY && !logicCollision){
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 0;
			movingBlockPosY++;
			Destroy(movingBlock);
			Vector3 blockPos = CalcBlockRenderPos();
			Quaternion blockRot = Quaternion.identity * Quaternion.Euler(0f,0f, 22.5f * movingBlockPosX);
			movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockPosY], blockPos, blockRot );
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 1;
			//DebugGrid();


		}
	}

	void PushBlockSideways(bool positiveScroll){

		int modifier;
		if (positiveScroll) modifier = 1; 
		else modifier = -1; 

		int clockX = (int) nfmod ( (float)(movingBlockPosX + modifier), (float)maxBlockPosX + 1);

		//check if logic collision happens
		 //WILL BE OUT OF BOUNDS
		
		//contrapositive applied here 
		if ( !(gridActiveBlocks[movingBlockPosY+3,clockX] == 1 )) {  // condA is only used for it's short circuiting property
			logicCollision = false;
		}
		else {
			logicCollision = true;

		}


		//performs so called block movement, if allowed
		if (!logicCollision){
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 0;
			Destroy(movingBlock);
			movingBlockPosX = clockX;
			Vector3 blockPos = CalcBlockRenderPos();
			Quaternion blockRot = Quaternion.identity * Quaternion.Euler(0f,0f, 22.5f * movingBlockPosX);
			movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockPosY], blockPos, blockRot );
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 1;
			//DebugGrid();
			
			
		}
	}

	Vector3 CalcBlockRenderPos(){
		float dist_incr = 20.5f / 10.5f;
		float d = dist_incr * (movingBlockPosY+1);
		float angle_offset = 11.125f;
		float a = angle_offset + (22.5f * movingBlockPosX );
		Debug.Log ( a );
		float x = d * Mathf.Sin( a * Mathf.Deg2Rad );
		float y = d * Mathf.Cos( a * Mathf.Deg2Rad );
		float z = 0.1f;
		//Debug.Log ( x + ", " + y);
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

	//C# Modulus Is WRONG!
	//http://answers.unity3d.com/questions/380035/c-modulus-is-wrong-1.html
	float nfmod(float a, float b)
	{
		return a - b * Mathf.Floor(a / b);
	}
}
