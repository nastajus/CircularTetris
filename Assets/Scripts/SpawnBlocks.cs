using UnityEngine;
using System.Collections;

public class SpawnBlocks : MonoBehaviour {
	private float timer;
	public static float blockAdvanceTime = .1f;
	//private GameObject gridGO;
	public static int[,] gridActiveBlocks = new int[13,16];
	public static GameObject[,] gridActiveBlocksGO = new GameObject[13,16];
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
	private int score = 0;
	private AudioSource aud;

	private GameObject[] arrayAtomicBlock = new GameObject[10];

	// Use this for initialization
	void Start () {
		//initialize timer
		timer = Time.time;
//		gridGO = GameObject.FindGameObjectWithTag("Grid");
//		Debug.Log ( gridGO.renderer.bounds.extents ) ;
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

		aud = GameObject.Find("GameObject").GetComponent<AudioSource>();
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
			//Debug.Log ("+");
			PushBlockSideways(true);
		}

		//scroll down
		else if (Input.GetAxis("Mouse ScrollWheel") < 0){
			//move cw
			//Debug.Log ("-");
			PushBlockSideways(false);
		}

		if (!audio.isPlaying && audio.clip.isReadyToPlay){
			audio.Play();
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
			gridActiveBlocksGO[spawnPosY+3, spawnPosX] = movingBlock;
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
			ScoreIncrease(10);
			if ( OuterRingFull() ){
				//DeleteOuterRing();
			}

			Spawn ();
		}

		//performs so called block movement, if allowed
		if (movingBlockExists && movingBlockPosY < maxBlockPosY && !logicCollision){
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 0;
			movingBlockPosY++;
			Destroy(movingBlock);
			//Destroy(gridActiveBlocksGO[movingBlockPosY+3,movingBlockPosX]);
			Vector3 blockPos = CalcBlockRenderPos();
			Quaternion blockRot = Quaternion.identity * Quaternion.Euler(0f,0f, 22.5f * movingBlockPosX);
			movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockPosY], blockPos, blockRot );
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 1;
			gridActiveBlocksGO[movingBlockPosY+3,movingBlockPosX] = movingBlock;
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
			//Destroy(gridActiveBlocksGO[movingBlockPosY+3,movingBlockPosX]);
			movingBlockPosX = clockX;
			Vector3 blockPos = CalcBlockRenderPos();
			Quaternion blockRot = Quaternion.identity * Quaternion.Euler(0f,0f, 22.5f * movingBlockPosX);
			movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[movingBlockPosY], blockPos, blockRot );
			gridActiveBlocks[movingBlockPosY+3,movingBlockPosX] = 1;
			gridActiveBlocksGO[movingBlockPosY+3,movingBlockPosX] = movingBlock;
			DebugGrid();
			
			
		}
	}

	Vector3 CalcBlockRenderPos(){
		float dist_incr = 20.5f / 10.5f;
		float d = dist_incr * (movingBlockPosY+1);
		float angle_offset = 11.125f;
		float a = angle_offset + (22.5f * movingBlockPosX );
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

	bool OuterRingFull(){
		for (int i=gridActiveBlocks.GetUpperBound(0); i >= 0; i--){
			for (int j=0; j <= gridActiveBlocks.GetUpperBound(1); j++){
				bool hasContent = gridActiveBlocks[i,j] == 0;
				if (hasContent) return false;
			}
			DeleteOuterRing();
			return true;
		}
		return false; //compiler doesn't believe will return
	}

	void DeleteOuterRing(){
		int i=maxBlockPosY;
		for (int j=0; j <= gridActiveBlocks.GetUpperBound(1); j++){
			Debug.Log ( "DESTROYING NOW: " ) ; 
			DebugGrid();
			Destroy( gridActiveBlocksGO[i,j]); 
		}
		aud.Play();
		blockAdvanceTime = 4f;
	}

	void ScoreIncrease(int _score){
		score += _score;
	}

	void OnGUI(){

		float margin = Screen.width/30;
		float fontSize = Screen.height/30;
		Vector2 HUDsize = new Vector2 ( Screen.width/5, Screen.height - margin*2 );
		Vector2 HUDpos = new Vector2 ( Screen.width - margin - HUDsize.x, margin );

		Rect rct = new Rect(HUDpos.x, HUDpos.y, HUDsize.x, HUDsize.y);

		GUI.Box ( rct, "" );
		GUI.BeginGroup( rct );
		GUI.Box ( new Rect ( margin/2, margin/2, rct.width-margin, 50 ), "SCORE: \n" + score );
		GUI.EndGroup();


		//if (gameover){
		//	GUI.Box ( new Rect(margin, margin, Screen.width, Screen.height), 
		//}
	}

}
