using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnBlocks : MonoBehaviour {
	private float timer;
	private float blockAdvanceTime;
	private const float blockAdvanceTimeDecrementFactor = 0.03f;
	private const float blockAdvanceTimeMax = 0.05f;
	private const int blocksLandedCounterLevelIncreaseAmount = 1;
	private int blocksLandedCounter;
	private int blockAdvanceTimeLevel;

	//private GameObject gridGO;
	private int[][] gridActiveBlocks;
	private GameObject[][] gridActiveBlocksGO;
	private GameObject[] arrayAtomicBlock;
	private bool movingBlockExists;
	private GameObject movingBlock;
	private bool logicCollision;
	private int movingBlockPosY; //0-12, starts at 3, hidden 0-2
	private int movingBlockPosX; //0-15
	private const int maxBlockPosY = 9; //visible spectrum
	private const int maxBlockPosX = 15;
	private bool permittedToSpawnExists;
	private int spawnPosX;
	private int spawnPosY;
	private Color32 spawnColor;
	private int score;
	private AudioSource audRingClear;
	private AudioSource audBlockLand;
	private AudioSource audSlide;
	private AudioSource audMusic;
	private enum ColorNames { red, orange, yellow, green, blue, blue_dark, purple, LANDED };
	private Dictionary<ColorNames, Color32> OurColors = new Dictionary<ColorNames, Color32>(){
		{ColorNames.red , new Color32(238,57,57, 255)},
		{ColorNames.orange , new Color32(227,111,66, 255)},
		{ColorNames.yellow , new Color32(221,197,74, 255)},
		{ColorNames.green , new Color32(106,189,69, 255)},
		{ColorNames.blue , new Color32(69,197,231, 255)},
		{ColorNames.blue_dark , new Color32(71,85,165, 255)},
		{ColorNames.purple , new Color32(132,90,166, 255)},
		{ColorNames.LANDED , new Color32(1,128,255, 255)}
	};
	private enum Tetronimo { I, O, T, J, L, S, Z };
	private Dictionary<Tetronimo, ColorNames> mapTetronimoColors = new Dictionary<Tetronimo, ColorNames>(){
		{Tetronimo.I, ColorNames.red},
		{Tetronimo.O, ColorNames.orange},
		{Tetronimo.T, ColorNames.yellow},
		{Tetronimo.J, ColorNames.green},
		{Tetronimo.L, ColorNames.blue},
		{Tetronimo.S, ColorNames.blue_dark},
		{Tetronimo.Z, ColorNames.purple}
	};
	private bool gameover = false;
	private GameObject blocks; //holder
	private enum Score { BlockSimplyLands, RingsComplete1, RingsComplete2, RingsComplete3, RingsComplete4 };
	private Dictionary<Score, int> mapActionPoints = new Dictionary<Score, int>(){
		{Score.BlockSimplyLands, 10},
		{Score.RingsComplete1, 200}, 
		{Score.RingsComplete2, 500}, 
		{Score.RingsComplete3, 1000}, 
		{Score.RingsComplete4, 2500}
	};
	public Texture GMUtext;



	// Use this for initialization
	void Start () {



		spawnPosX = 0;
		spawnPosY = 0;
		blocks = new GameObject("blocks");
		score = 0;
		blockAdvanceTime = .2f;
		blocksLandedCounter = 0;
		blockAdvanceTimeLevel = 1;
		gridActiveBlocks = new int[13][];
		gridActiveBlocksGO = new GameObject[13][];
		for (int i=0; i<gridActiveBlocks.Length; i++){
			gridActiveBlocks[i] = new int[16];
			gridActiveBlocksGO[i] = new GameObject[16];
		}
		arrayAtomicBlock = new GameObject[10];
		movingBlockExists = false;
		logicCollision = false;
		permittedToSpawnExists = true;
		timer = Time.time;

//		gridGO = GameObject.FindGameObjectWithTag("Grid");
//		Debug.Log ( gridGO.renderer.bounds.extents ) ;
		for (int i=0; i < arrayAtomicBlock.Length; i++){
			string loadMe = "Prefabs/block" + (i+2);
			arrayAtomicBlock[i] = (GameObject) Resources.Load ( loadMe );
		}
		for (int i=0; i < gridActiveBlocks.Length; i++){
			for (int j=0; j < gridActiveBlocks[0].Length; j++){
				gridActiveBlocks[i][j] = 0;
			}
		}


		//DebugGrid();

		audBlockLand = GameObject.Find("AudioEffectBlockLand").GetComponent<AudioSource>();
		audRingClear = GameObject.Find("AudioEffectRingClear").GetComponent<AudioSource>();
		audSlide = GameObject.Find("AudioEffectSlide").GetComponent<AudioSource>();
		audMusic = GameObject.Find("AudioMusic").GetComponent<AudioSource>();

		if ( audMusic != null ) audMusic.Play();

	}

	void FixedUpdate(){

		//timer ticks
		float deltaTick = Time.time - timer;
		if (deltaTick > blockAdvanceTime){
			timer += deltaTick;
			PushBlockOut();
		}

		if (!movingBlockExists & permittedToSpawnExists){
			Spawn();
		}
		else if ( Input.GetKeyDown(KeyCode.Space) ){
			PushBlockOutMax();
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

//		if (!audio.isPlaying && audio.clip.isReadyToPlay){
//			audio.Play();
//		}

		if (gameover && Input.GetKeyDown(KeyCode.Return)){
			ResetGame();
		}

	}

	bool Spawn(){

		movingBlockPosX = 0;

		if (gridActiveBlocks[spawnPosY+3][spawnPosX] == 0 && !gameover){
			spawnColor = OurColors[(ColorNames)Random.Range( 0, 6 )];
			//spawnColor = Color.white;
			movingBlockPosY = spawnPosY;
			Vector2 newPos = new Vector2 ( spawnPosX, spawnPosY );
			Push(newPos);
			movingBlockExists = true;
			return true;
		}
		else 
			return false;
	}

	void Push( Vector2 newPos ){

		Vector3 blockPos = CalcBlockRenderPos(newPos);
		Quaternion blockRot = Quaternion.identity * Quaternion.Euler(0f,0f, 22.5f * newPos.x);
		movingBlock = (GameObject) Instantiate ( arrayAtomicBlock[(int)newPos.y], blockPos, blockRot );
		movingBlock.transform.parent = blocks.transform;
		movingBlock.renderer.material.color = spawnColor;
		gridActiveBlocksGO[(int)newPos.y+3][(int)newPos.x] = movingBlock;
		gridActiveBlocks[(int)newPos.y+3][(int)newPos.x] = 1;
	}

	void PushBlockOut(){

		//check if logic collision happens
		bool condA = movingBlockPosY + 1 > maxBlockPosY;  //WILL BE OUT OF BOUNDS

		//contrapositive applied here 
		if ( !condA && !(gridActiveBlocks[movingBlockPosY+1+3][movingBlockPosX] == 1 )) {  // condA is only used for it's short circuiting property
			logicCollision = false;
		}

		else {
			logicCollision = true;

			//movingBlock.renderer.material.color = OurColors[ColorNames.LANDED];
			if ( audBlockLand != null && !gameover) audBlockLand.Play();

			//DebugGridGO();
			//Destroy( gridActiveBlocksGO[movingBlockPosY+1+3][movingBlockPosX] );
			//lock block
			//??? delay ???

			//check if room

			if ( movingBlockPosX == spawnPosX && movingBlockPosY == spawnPosY ){
				gameover = true;
			}
			else {
				int amtDeleted = OuterRingsDeleted();
				if ( amtDeleted > 0 ){
					ScoreIncrease( mapActionPoints[ (Score)amtDeleted ] );
					PushRingsOut(amtDeleted);
				}
				else {
					ScoreIncrease( mapActionPoints[Score.BlockSimplyLands] );
				}
				blocksLandedCounter++;
				bool condX = blocksLandedCounter >= blocksLandedCounterLevelIncreaseAmount * blockAdvanceTimeLevel;
				bool condY = blockAdvanceTime - blockAdvanceTimeDecrementFactor >= blockAdvanceTimeMax;
				if ( condX && condY){
					blockAdvanceTime -= blockAdvanceTimeDecrementFactor;
					blockAdvanceTimeLevel++;
				}

//				if ( gridActiveBlocksGO[spawnPosY+3+1, spawnPosX] == null ){ 
//					Debug.Log ("WOO"); }
//				else 
//					Debug.Log ("BOO");
				//TODO: add delay here of a tick
				Spawn();
			}
		}

		//performs so called block movement, if allowed
		if (movingBlockExists && movingBlockPosY < maxBlockPosY && !logicCollision){

			gridActiveBlocks[movingBlockPosY+3][movingBlockPosX] = 0;
			//Destroy(movingBlock);
			Destroy(gridActiveBlocksGO[movingBlockPosY+3][movingBlockPosX]);

			Vector2 newPos = new Vector2 ( movingBlockPosX, ++movingBlockPosY );
			Push(newPos);

			//DebugGrid();

		}
	}
	
	void PushBlockOutMax(){
		for (int y=gridActiveBlocks.Length - 1; y >= 0; y--){

			bool condLowestEmpty = gridActiveBlocks[y][movingBlockPosX] == 0;

			if ( condLowestEmpty ){

				movingBlockPosY = y+1-3;

				gridActiveBlocks[movingBlockPosY-1+3][movingBlockPosX] = 0;
				Destroy(gridActiveBlocksGO[movingBlockPosY-1+3][movingBlockPosX]);

				Vector2 newPos = new Vector2 ( movingBlockPosX, movingBlockPosY );
				Push(newPos);
			}
		}

	}

	void PushBlockSideways(bool positiveScroll){

		int modifier;
		if (positiveScroll) modifier = 1; 
		else modifier = -1; 

		int clockX = (int) nfmod ( (float)(movingBlockPosX + modifier), (float)maxBlockPosX + 1);
		//spawnPosX = clockX;

		//check if logic collision happens
		 //WILL BE OUT OF BOUNDS
		
		//contrapositive applied here 
		if ( !(gridActiveBlocks[movingBlockPosY+3][clockX] == 1 )) {  // condA is only used for it's short circuiting property
			logicCollision = false;
		}
		else {
			logicCollision = true;

		}


		//performs so called block movement, if allowed
		if (!logicCollision && !gameover){
			gridActiveBlocks[movingBlockPosY+3][movingBlockPosX] = 0;
			//Destroy(movingBlock);
			Destroy(gridActiveBlocksGO[movingBlockPosY+3][movingBlockPosX]);
			movingBlockPosX = clockX;

			Vector2 newPos = new Vector2 ( clockX, movingBlockPosY );
			Push(newPos);
			//DebugGrid();

		}


		StartCoroutine("Swoosh");

	}

	IEnumerator Swoosh() {
		if ( audSlide != null && !gameover) audSlide.Play();
		yield return null;

	}


	void PushRingsOut(int pushDist){
		for (int y=gridActiveBlocks.Length - 1 - pushDist; y >= 0; y--){
			for (int x=0; x < gridActiveBlocks[0].Length; x++){
				if (gridActiveBlocks[y][x] != 0){
					gridActiveBlocks[y][x] = 0;
					Destroy(gridActiveBlocksGO[y][x]);
					Vector2 newPos = new Vector2 ( x, y+1-3 );
					Push(newPos);
					//gridActiveBlocksGO[y+1][x].renderer.material.color = OurColors[ColorNames.LANDED];
				}
			}
		}
	}

	Vector3 CalcBlockRenderPos( Vector2 pos ){
		float dist_incr = 20.5f / 10.5f;
		float d = dist_incr * ((int)pos.y+1);
		float angle_offset = 11.125f;
		float a = angle_offset + (22.5f * (int)pos.x );
		float x = d * Mathf.Sin( a * Mathf.Deg2Rad );
		float y = d * Mathf.Cos( a * Mathf.Deg2Rad );
		float z = 0.1f;
		//Debug.Log ( x + ", " + y);
		return new Vector3 (x,-y,z);
	}

	void DebugGrid(bool go = false){

		if (!go){
			string lines = "";
			for (int i=0; i < gridActiveBlocks.Length; i++){
				string line = "";
				for (int j=0; j < gridActiveBlocks[0].Length; j++){
					line += gridActiveBlocks[i][j] + " " ;
				}
				line += "\n";
				lines += line;
			}
			Debug.Log ( lines );
		}
		else {
			string lines = "";
			for (int i=0; i < gridActiveBlocksGO.Length; i++){
				string line = "";
				for (int j=0; j < gridActiveBlocksGO[0].Length; j++){
					line += "(null) ";
				}
				line += "\n";
				lines += line;
			}
			Debug.Log ( lines );

		}
	}

	void DebugGridGO(){
		
		string lines = "";
		for (int i=0; i < gridActiveBlocksGO.Length; i++){
			string line = "i";
			for (int j=0; j < gridActiveBlocksGO[0].Length; j++){
				line+="j";
				if ( gridActiveBlocksGO[i][j] != null )
					line += gridActiveBlocksGO[i][j] + " @ (" +i+ ","+j+")" ;
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

	//todo improve outer loop logic, is stupid
	int OuterRingsDeleted(){
		int amtDeleted = 0;
		bool ringCompletelyFull = true;
		for (int i=gridActiveBlocks.Length - 1; i >= 0; i--){
			for (int j=0; j < gridActiveBlocks[0].Length; j++){
				bool hasContent = gridActiveBlocks[i][j] == 0;
				if (hasContent) return amtDeleted;
			}
			if ( DeleteRing(gridActiveBlocksGO[i]) ) amtDeleted++;
			DeleteRing(gridActiveBlocks[i]);
		}
		return amtDeleted;
	}

	bool DeleteRing(GameObject[] ring){
		bool returnState = true; 
		int i=maxBlockPosY+1;

		foreach ( GameObject go in ring ){
			Destroy( go ); 
		}

		if ( audRingClear != null ) audRingClear.Play();
		return returnState;
	}

	bool DeleteRing(int[] ring){
		bool returnState = true; 
		int i=maxBlockPosY+1;

		for (int ii=0; ii< ring.Length; ii++){
			
			ring[ii] = 0;
			
		}

		return returnState;
	}


//	bool DeleteRing(int[] ring){
//		bool returnState = true; 
//		int i=maxBlockPosY+1;
//		
//		Debug.Log ( "BEFORE" );
//		DebugGrid();
//		DebugGrid(true);
//		
//		foreach ( int go in ring ){
//			Debug.Log ( go );
//			Destroy( go ); 
//		}
//			
//		Debug.Log ( "AFTER" );
//		DebugGrid();
//		DebugGrid(true);
//		
//		//aud.Play();
//		return returnState;
//	}

	void ScoreIncrease(int _score){
		score += _score;
	}

	void OnGUI(){

		float margin = Screen.width/30;
//		float fontSize = Screen.height/30;
//		Debug.Log ( fontSize );
//		GUIStyle style = GUI.skin.GetStyle("Skin");
//		style.fontSize = (int)fontSize;
		Vector2 HUDsize = new Vector2 ( Screen.width/5, Screen.height - margin*2 );
		Vector2 HUDpos = new Vector2 ( Screen.width - margin - HUDsize.x, margin );

		Rect rctGroup = new Rect(HUDpos.x, HUDpos.y, HUDsize.x, HUDsize.y);

		GUI.Box ( rctGroup, "" );
		GUI.BeginGroup( rctGroup );
		Rect rctA = new Rect ( margin/2, margin/2, rctGroup.width-margin, rctGroup.height/7 );
		GUI.Box ( rctA, "SCORE: \n" + score );
		Rect rctB = new Rect ( margin/2, margin/2 + rctA.yMax, rctGroup.width-margin, rctGroup.height/7 );
		GUI.Box ( rctB, "SPEED: \n" + Mathf.Round( (maxBlockPosY+1)/blockAdvanceTime ));
		Rect rctC = new Rect ( margin/2, margin/2 + rctB.yMax, rctGroup.width-margin, rctGroup.height/7 );
		GUI.Box ( rctC, "LEVEL: \n" + blockAdvanceTimeLevel.ToString("X") );
		Rect rctD = new Rect ( margin/2, margin/2 + rctC.yMax, rctGroup.width-margin, rctGroup.height - rctC.yMax - margin );
		//GUI.DrawTexture();
		//Resources.Load<Texture> ("Prefabs/block6")
		//Texture2D image = (Texture2D)Resources.Load("image1");
		Sprite sampleBlock = Resources.Load<Sprite> ("Sprites/block6");
		GUI.Box ( rctD, "NEXT PIECE: \n" ); 
		Rect rctE = new Rect ( margin, margin + rctC.yMax + margin/2, rctD.width-margin, rctD.height-margin );//rctGroup.width-margin, rctGroup.height - rctD.yMax - margin
		//GUI.Box ( rctE, "DERP" ); //sampleBlock.texture
		GUI.DrawTexture( rctE, sampleBlock.texture, ScaleMode.ScaleToFit);
		GUI.EndGroup();


		//GMU logo
		if ( GMUtext != null){
			float h = (Screen.height*0.15f);
			float ratio = h/GMUtext.height;
			float w = GMUtext.width*ratio;
		
			GUI.DrawTexture( new Rect( margin/2, Screen.height - h - margin/2, w, h ), GMUtext );
		}

		if (gameover){
			Rect rctGameOver = new Rect(0,0,Screen.width - HUDsize.x - margin,Screen.height);
			GUI.DrawTexture( rctGameOver, Resources.Load<Texture> ("Textures/GameOver" ), ScaleMode.ScaleToFit );
			GUI.Box ( new Rect (Screen.width/2, rctGameOver.yMax + margin, 150, 50 ), "\nPRESS ENTER TO RESTART" );
		}
	}

	void ResetGame(){
		Destroy(blocks);
		gameover = false;
		Start ();
	}

}
