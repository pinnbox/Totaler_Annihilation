using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using GameUtility;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace MonoProgram;

public class Game1 : Game
{
	//Screen dimensions
	const int SCREEN_WIDTH = 1920;
	const int SCREEN_HEIGHT = 1200;

	//Map dimensions
	const int MAP_WIDTH = 4000;
	const int MAP_HEIGHT = 4000;

	//Grid tile dimensions
	const int GRID_WIDTH = 20;
	const int GRID_HEIGHT = 20;

	//Camera speed
	const float CAM_PAN_SPEED = 25f;

	//Max nodes for pathfinding
	const int MAX_NODES = 10000;

	//Max amound of sounds played a second
	const int maxSoundsPerSecond = 10;

	//Setting up randomness
	static Random rng = new Random();

	//Grids for buildings and blocked tiles
	bool[,] buildingGrid;
	bool[,] blockedGrid;

	//Camera variables
	float camZoom = 1f;
	Vector2 camPosition = new Vector2(400, 1875);
	Viewport camViewport;
	Matrix camTransform;

	//Keyboard state and previous frame keyboard state
	KeyboardState kb;
	KeyboardState prevKb;

	//Mouse state and previous frame mouse state
	MouseState mouse;
	MouseState prevMouse;

	//Fonts
	SpriteFont smallFont;
	SpriteFont medFont;
	SpriteFont largeFont;
	SpriteFont americanFont;

	//Explosion animations
	Animation[] explosions = new Animation[20];

	//Explosion textures
	Texture2D explosion1Img;
	Texture2D explosion2Img;
	Texture2D explosion3Img;
	Texture2D explosion4Img;

	//all sound effects
	SoundEffect explosion1SFX;
	SoundEffect explosion2SFX;
	SoundEffect explosion3SFX;
	SoundEffect explosion4SFX;
	SoundEffect shootingSFX;
	SoundEffect movingSFX;

	//Title and pause screen textures
	Texture2D winScreenImg;
	Texture2D loseScreenImg;
	Texture2D titleScreenImg;
	Texture2D pauseScreenImg;
	Texture2D papyrusImg;

	//Ttile and pause screen rectangles
	Rectangle winScreenRec;
	Rectangle loseScreenRec;
	Rectangle titleScreenRec;
	Rectangle pauseScreenRec;
	Rectangle papyrusRec;

	//Title screen button rectangles
	Rectangle button1Rec;
	Rectangle button2Rec;
	Rectangle button3Rec;
	Rectangle button4Rec;

	//Set up metal spot positions
	Vector2[] mSpots = new Vector2[] { new Vector2(528, 828), new Vector2(340, 924), new Vector2(334, 1741), new Vector2(538, 1861), new Vector2(320, 1964), new Vector2(320, 2938), new Vector2(534, 3065), new Vector2(1749, 832), new Vector2(1375, 2038), new Vector2(1978, 1964), new Vector2(1381, 2904), new Vector2(2626, 1089), new Vector2(2552, 1870), new Vector2(2400, 3170), new Vector2(3455, 742), new Vector2(3649, 874), new Vector2(3731, 1789), new Vector2(3573, 1897), new Vector2(3785, 2069), new Vector2(3663, 3010), new Vector2(3485, 3215) };

	//Unit management variables
	List<Unit> units = new List<Unit>();
	List<Unit> unitMenu = new List<Unit>();
	List<Unit> enemyUnits = new List<Unit>();
	Queue<Unit> unitQueue = new Queue<Unit>();
	List<Unit> selectedUnits = new List<Unit>();
	bool unitMaking;

	//Building management variables
	List<Building> buildings = new List<Building>();
	List<Building> buildMenu = new List<Building>();
	List<Building> enemyBuildings = new List<Building>();
	Building placingBuilding;
	Building selectedBuilding;
	bool buildingSelected;

	//Projectiles list
	List<Projectile> projectiles = new List<Projectile>();

	//Dictionaries for textures, unit types, building types, and projectile types
	Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
	Dictionary<string, Unit> unitTypes = new Dictionary<string, Unit>();
	Dictionary<string, Building> buildingTypes = new Dictionary<string, Building>();
	Dictionary<string, Projectile> projectileTypes = new Dictionary<string, Projectile>();

	//Fog of war matrix
	int[,] fog = new int[200, 200];
	bool fogOff;

	//Starting positions of drags
	Vector2 leftClickStart;
	Vector2 rightClickStart;

	//Unit ids to differentiate units
	int nextUnitID = 0;

	//Army values for enemy unit spawning
	int playerArmyValue = 0;
	int enemyArmyValue = 0;

	//Resource values
	int metal = 1000;
	int energy = 5000;
	int maxMetal = 1000;
	int maxEnergy = 5000;

	//Menu and endscreen display variables
	bool win;
	bool papyrusActive;

	//Game timer
	int timer = 0;

	//Background song
	Song backgroundSong;

	//Tracks the number of sounds played each second
	int soundsThisSecond;

	//Unit struct
	struct Unit
	{
		//Texture and rectangle
		public string texture;
		public Rectangle rec;

		//Hitbox and location
		public Vector2 hitbox;
		public Vector2 location;

		//Construction variables
		public int tier;
		public int mCost;
		public int eCost;
		public int buildTime;
		public int buildCounter;
		public bool isActive;

		//Combat variables
		public int health;
		public int damage;
		public int atkSpd;
		public int range;
		public int moveSpd;
		public int sightRange;
		public int atkCooldown;
		public int buildPower;
		public string projectile;
		public int currentHealth;
		public int armyValue;

		//Unique identifier
		public int id;

		//Pathfinding variables
		public Queue<Point> path;
		public Point destination;
		public double rotation;

		//Description
		public string description;
	}

	//Building struct
	struct Building
	{
		//Texture and rectangle
		public string texture;
		public Rectangle rec;

		//Hitbox and location
		public Vector2 hitbox;
		public Vector2 location;

		//Construction variables
		public int tier;
		public int mCost;
		public int eCost;
		public int buildTime;
		public int buildCounter;
		public bool isActive;
		public int buildPower;
		public int mProduction;
		public int eProduction;
		public int mStorage;
		public int eStorage;

		//Combat variables
		public int currentHealth;
		public int health;
		public int damage;
		public int atkSpd;
		public int range;
		public int sightRange;
		public int atkCooldown;
		public string projectile;

		//Description
		public string description;
	}

	//Projectile struct
	struct Projectile
	{
		//Texture and rectangle
		public string texture;
		public Rectangle rec;

		//Hitbox, initial position, destination, and rotation
		public Vector2 location;
		public Vector2 hitbox;
		public Vector2 destination;
		public float rotation;

		//Progression towards destination
		public int timer;
		public int lifespan;

		//Radius of explosion and damage
		public int range;
		public int damage;
	}

	//Map texture and rectangle
	Texture2D mapImg;
	Rectangle mapRec;

	//Selection box texture and rectangle	
	Texture2D pixel;
	Rectangle selectBoxRec;

	//Gamestate
	string gameState = "MENU";

	//Graphics device manager and spritebatch
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;

	//Setup graphics device manager and root directory
	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
	}

	//Initialize graphics settings and fog
	protected override void Initialize()
	{
		//Initializes all graphics settings
		_graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
		_graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
		_graphics.SynchronizeWithVerticalRetrace = false;
		IsFixedTimeStep = true;

		//Initializes all fog tiles to be unseen
		for (int x = 0; x < 200; x++)
		{
			for (int y = 0; y < 200; y++)
			{
				fog[x, y] = 0;
			}
		}

		//Applies the changes
		_graphics.ApplyChanges();
		base.Initialize();
	}

	//Loads all content into the respective variables
	protected override void LoadContent()
	{
		//Initializes the spritebatch
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		//Initializes the dynamic camera
		camViewport = GraphicsDevice.Viewport;

		//Sets up map texture and rectangle
		mapImg = Content.Load<Texture2D>("Images/Maps/Map");
		mapRec = new Rectangle(0, 0, MAP_WIDTH, MAP_HEIGHT);

		//Loads selection box texture
		pixel = Content.Load<Texture2D>("Images/Sprites/Misc/SelectionBox");

		//Sets up the building grid and blocked grid
		buildingGrid = new bool[MAP_WIDTH / GRID_WIDTH, MAP_HEIGHT / GRID_HEIGHT];
		blockedGrid = new bool[MAP_WIDTH / GRID_WIDTH, MAP_HEIGHT / GRID_HEIGHT];

		//Load song
		backgroundSong = Content.Load<Song>("Audio/Music/Music");

		//Setup music player
		MediaPlayer.Volume = 0.5f;
		MediaPlayer.IsRepeating = true;
		MediaPlayer.Play(backgroundSong);

		//Block all areas with impassable terrain
		BlockArea(0, 0, 14, 24);
		BlockArea(12, 0, 26, 18);
		BlockArea(65, 0, 129, 12);
		BlockArea(71, 12, 125, 20);
		BlockArea(83, 20, 116, 30);
		BlockArea(68, 52, 97, 89);
		BlockArea(95, 58, 107, 73);
		BlockArea(129, 71, 143, 83);
		BlockArea(176, 0, 200, 23);
		BlockArea(63, 114, 81, 123);
		BlockArea(68, 123, 82, 131);
		BlockArea(110, 102, 120, 111);
		BlockArea(104, 111, 131, 122);
		BlockArea(98, 123, 132, 144);
		BlockArea(132, 120, 140, 128);
		BlockArea(0, 174, 21, 200);
		BlockArea(68, 162, 108, 171);
		BlockArea(67, 170, 116, 189);
		BlockArea(59, 189, 133, 200);
		BlockArea(184, 178, 200, 184);
		BlockArea(178, 183, 200, 200);

		//Loads all fonts
		smallFont = Content.Load<SpriteFont>("Fonts/SmallFont");
		medFont = Content.Load<SpriteFont>("Fonts/MedFont");
		largeFont = Content.Load<SpriteFont>("Fonts/LargeFont");
		americanFont = Content.Load<SpriteFont>("Fonts/AmericanFont");

		//Loads all title and pause screen images
		winScreenImg = Content.Load<Texture2D>("Images/Sprites/Misc/WinScreen");
		loseScreenImg = Content.Load<Texture2D>("Images/Sprites/Misc/LoseScreen");
		titleScreenImg = Content.Load<Texture2D>("Images/Sprites/Misc/TitleScreen");
		pauseScreenImg = Content.Load<Texture2D>("Images/Sprites/Misc/PauseScreen");
		papyrusImg = Content.Load<Texture2D>("Images/Sprites/Misc/Papyrus");

		//Sets up all title and pause screen rectangles
		winScreenRec = new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
		loseScreenRec = new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
		titleScreenRec = new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
		pauseScreenRec = new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
		papyrusRec = new Rectangle(100, 100, SCREEN_WIDTH - 200, SCREEN_HEIGHT - 200);

		//Sets up all title screen buttons
		button1Rec = new Rectangle(380, 970, 350, 80);
		button2Rec = new Rectangle(1195, 970, 350, 80);
		button3Rec = new Rectangle(380, 1060, 350, 80);
		button4Rec = new Rectangle(1195, 1060, 350, 80);

		//Loads all explosion images
		explosion1Img = Content.Load<Texture2D>("Images/Sprites/Animations/Explosion1");
		explosion2Img = Content.Load<Texture2D>("Images/Sprites/Animations/Explosion2");
		explosion3Img = Content.Load<Texture2D>("Images/Sprites/Animations/Explosion3");
		explosion4Img = Content.Load<Texture2D>("Images/Sprites/Animations/Explosion4");

		//Sets up the explosion animations array
		for (int i = 0; i < explosions.Length; i++)
		{
			explosions[i] = new Animation(explosion1Img, 5, 5, 23, 0, Animation.NO_IDLE, 1, 500, new Vector2(0, 0), 1f, 1f, false, "Explosion1");
		}

		//Loads all sound effects
		explosion1SFX = Content.Load<SoundEffect>("Audio/Sounds/Explosion1");
		explosion2SFX = Content.Load<SoundEffect>("Audio/Sounds/Explosion2");
		explosion3SFX = Content.Load<SoundEffect>("Audio/Sounds/Explosion3");
		explosion4SFX = Content.Load<SoundEffect>("Audio/Sounds/Explosion4");
		shootingSFX = Content.Load<SoundEffect>("Audio/Sounds/Shooting");
		movingSFX = Content.Load<SoundEffect>("Audio/Sounds/Moving");

		//Loads all unit textures
		textures["AConstructor"] = Content.Load<Texture2D>("Images/Sprites/Units/AConstructor");
		textures["Banisher"] = Content.Load<Texture2D>("Images/Sprites/Units/Banisher");
		textures["Brute"] = Content.Load<Texture2D>("Images/Sprites/Units/Brute");
		textures["Commander"] = Content.Load<Texture2D>("Images/Sprites/Units/Commander");
		textures["Constructor"] = Content.Load<Texture2D>("Images/Sprites/Units/Constructor");
		textures["Incisor"] = Content.Load<Texture2D>("Images/Sprites/Units/Incisor");
		textures["Lasher"] = Content.Load<Texture2D>("Images/Sprites/Units/Lasher");
		textures["Pounder"] = Content.Load<Texture2D>("Images/Sprites/Units/Pounder");
		textures["Quaker"] = Content.Load<Texture2D>("Images/Sprites/Units/Quaker");
		textures["Rascal"] = Content.Load<Texture2D>("Images/Sprites/Units/Rascal");
		textures["Salamander"] = Content.Load<Texture2D>("Images/Sprites/Units/Salamander");
		textures["Tiger"] = Content.Load<Texture2D>("Images/Sprites/Units/Tiger");
		textures["Tzar"] = Content.Load<Texture2D>("Images/Sprites/Units/Tzar");
		textures["Wolverine"] = Content.Load<Texture2D>("Images/Sprites/Units/Wolverine");

		//Loads all building textures
		textures["AConverter"] = Content.Load<Texture2D>("Images/Sprites/Buildings/AConverter");
		textures["AFUS"] = Content.Load<Texture2D>("Images/Sprites/Buildings/AFUS");
		textures["Agitator"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Agitator");
		textures["AMex"] = Content.Load<Texture2D>("Images/Sprites/Buildings/AMex");
		textures["ConsTurret"] = Content.Load<Texture2D>("Images/Sprites/Buildings/ConsTurret");
		textures["Converter"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Converter");
		textures["DTeeth"] = Content.Load<Texture2D>("Images/Sprites/Buildings/DTeeth");
		textures["EStorage"] = Content.Load<Texture2D>("Images/Sprites/Buildings/EStorage");
		textures["Lab1"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Lab1");
		textures["Lab2"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Lab2");
		textures["LLT"] = Content.Load<Texture2D>("Images/Sprites/Buildings/LLT");
		textures["Mex"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Mex");
		textures["MStorage"] = Content.Load<Texture2D>("Images/Sprites/Buildings/MStorage");
		textures["Persecutor"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Persecutor");
		textures["Scorpion"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Scorpion");
		textures["Solar"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Solar");
		textures["Turbine"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Turbine");
		textures["Warden"] = Content.Load<Texture2D>("Images/Sprites/Buildings/Warden");

		//Loads all projectile textures
		textures["Artillery"] = Content.Load<Texture2D>("Images/Sprites/Projectiles/Artillery");
		textures["Cannon"] = Content.Load<Texture2D>("Images/Sprites/Projectiles/Cannon");
		textures["Laser"] = Content.Load<Texture2D>("Images/Sprites/Projectiles/Laser");
		textures["Plasma"] = Content.Load<Texture2D>("Images/Sprites/Projectiles/Plasma");
		textures["Rocket"] = Content.Load<Texture2D>("Images/Sprites/Projectiles/Rocket");

		//Sets up all unit types
		unitTypes["AConstructor"] = new Unit { texture = "AConstructor", tier = 2, mCost = 580, eCost = 7000, buildTime = 12900, health = 2150, damage = 0, range = 400, moveSpd = 50, sightRange = 295, buildPower = 500, armyValue = 30, hitbox = new Vector2(30, 50), description = "T2 Constructor - Builds more advanced buildings than its T1 counterpart" };
		unitTypes["Banisher"] = new Unit { texture = "Banisher", tier = 2, mCost = 1000, eCost = 23000, buildTime = 23100, health = 2500, damage = 1000, atkSpd = 75000, range = 800, moveSpd = 54, sightRange = 650, buildPower = 0, armyValue = 30, hitbox = new Vector2(60, 60), projectile = "Rocket", description = "Heavy Missile Tank - Fires homing missiles with huge damage" };
		unitTypes["Brute"] = new Unit { texture = "Brute", tier = 1, mCost = 235, eCost = 2400, buildTime = 3310, health = 1970, damage = 97, atkSpd = 70, range = 350, moveSpd = 73, sightRange = 325, buildPower = 0, armyValue = 6, hitbox = new Vector2(50, 50), projectile = "Plasma", description = "Medium Assault Tank - The armored core of T1 armies" };
		unitTypes["Commander"] = new Unit { texture = "Commander", tier = 1, mCost = 0, eCost = 0, buildTime = 9999999, health = 8000, damage = 100, atkSpd = 20, range = 300, moveSpd = 38, sightRange = 450, buildPower = 500, armyValue = 999999, hitbox = new Vector2(75, 50), projectile = "Laser", description = "Commander - If this unit dies, you lose" };
		unitTypes["Constructor"] = new Unit { texture = "Constructor", tier = 1, mCost = 145, eCost = 2100, buildTime = 4160, health = 1430, damage = 0, range = 350, moveSpd = 51, sightRange = 260, buildPower = 250, armyValue = 5, hitbox = new Vector2(30, 50), description = "T1 Constructor - A basic constructor to build up your economy" };
		unitTypes["Incisor"] = new Unit { texture = "Incisor", tier = 1, mCost = 120, eCost = 1040, buildTime = 2200, health = 820, damage = 75, atkSpd = 46, range = 98, moveSpd = 86, sightRange = 330, buildPower = 0, armyValue = 2, hitbox = new Vector2(40, 50), projectile = "Laser", description = "Light Tank - A light and fast unit armed with a weak laser turret" };
		unitTypes["Lasher"] = new Unit { texture = "Lasher", tier = 1, mCost = 155, eCost = 2400, buildTime = 3440, health = 860, damage = 120, atkSpd = 150, range = 700, moveSpd = 52, sightRange = 620, buildPower = 0, armyValue = 3, hitbox = new Vector2(45, 75), projectile = "Rocket", description = "Light Missile Tank - Inefficient in close combat but can outrange most other T1 options" };
		unitTypes["Pounder"] = new Unit { texture = "Pounder", tier = 1, mCost = 220, eCost = 2600, buildTime = 3000, health = 1490, damage = 190, atkSpd = 108, range = 315, moveSpd = 41, sightRange = 286, buildPower = 0, armyValue = 6, hitbox = new Vector2(50, 50), projectile = "Cannon", description = "Anti Swarm Tank - A powerful tank that shreds groups of small units" };
		unitTypes["Quaker"] = new Unit { texture = "Quaker", tier = 2, mCost = 400, eCost = 4400, buildTime = 6500, health = 1200, damage = 420, atkSpd = 300, range = 800, moveSpd = 58, sightRange = 299, buildPower = 0, armyValue = 20, hitbox = new Vector2(50, 70), projectile = "Artillery", description = "Heavy Artillery - Keep them safe while they shell your enemies from afar" };
		unitTypes["Rascal"] = new Unit { texture = "Rascal", tier = 1, mCost = 26, eCost = 270, buildTime = 1150, health = 90, damage = 35, atkSpd = 60, range = 180, moveSpd = 153, sightRange = 600, buildPower = 0, armyValue = 1, hitbox = new Vector2(30, 30), projectile = "Laser", description = "Light Scout Vehicle - Cheap and quick to build vehicles to scout out your opponent's base" };
		unitTypes["Salamander"] = new Unit { texture = "Salamander", tier = 2, mCost = 350, eCost = 7000, buildTime = 7900, health = 2100, damage = 180, atkSpd = 66, range = 340, moveSpd = 72, sightRange = 385, buildPower = 0, armyValue = 30, hitbox = new Vector2(60, 60), projectile = "Laser", description = "Heavy Laser Tank - Their beam laser makes them good at single target damage" };
		unitTypes["Tiger"] = new Unit { texture = "Tiger", tier = 2, mCost = 655, eCost = 10000, buildTime = 11500, health = 5300, damage = 109, atkSpd = 42, range = 410, moveSpd = 71, sightRange = 462, buildPower = 0, armyValue = 40, hitbox = new Vector2(50, 60), projectile = "Plasma", description = "Heavy Assault Tank - General purpose heavy tank" };
		unitTypes["Tzar"] = new Unit { texture = "Tzar", tier = 2, mCost = 1650, eCost = 28000, buildTime = 30000, health = 7800, damage = 900, atkSpd = 180, range = 650, moveSpd = 41, sightRange = 395, buildPower = 0, armyValue = 150, hitbox = new Vector2(80, 80), projectile = "Cannon", description = "Very Heavy Assault Tank - A superheavy tank equipped wwith a ppwerful plasma cannon" };
		unitTypes["Wolverine"] = new Unit { texture = "Wolverine", tier = 1, mCost = 170, eCost = 2500, buildTime = 3550, health = 750, damage = 300, atkSpd = 426, range = 710, moveSpd = 48, sightRange = 300, buildPower = 0, armyValue = 4, hitbox = new Vector2(40, 50), projectile = "Artillery", description = "Light Artillery - Light artillery to outrange all other T1 units" };

		//Sets up all building types
		buildingTypes["AConverter"] = new Building { texture = "AConverter", tier = 2, mCost = 370, eCost = 2100, mProduction = -10, eProduction = 600, buildTime = 31300, health = 560, damage = 0, sightRange = 273, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(5, 5), description = "Converts metal into energy at a faster rate than its T1 counterpart" };
		buildingTypes["AFUS"] = new Building { texture = "AFUS", tier = 2, mCost = 2500, eCost = 10000, mProduction = 0, eProduction = 1200, buildTime = 100000, health = 9400, damage = 0, sightRange = 273, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(10, 10), description = "The ultimate machine for generating energy" };
		buildingTypes["Agitator"] = new Building { texture = "Agitator", tier = 1, mCost = 1300, eCost = 5500, mProduction = 0, eProduction = 0, buildTime = 19300, health = 3250, damage = 350, atkSpd = 95, range = 1245, mStorage = 0, eStorage = 0, sightRange = 455, buildPower = 0, hitbox = new Vector2(5, 4), projectile = "Artillery", description = "Medium range plasma cannon with large AOE" };
		buildingTypes["AMex"] = new Building { texture = "AMex", tier = 2, mCost = 640, eCost = 8100, mProduction = 20, eProduction = 0, buildTime = 14100, health = 3900, damage = 0, sightRange = 273, buildPower = 1, mStorage = 0, eStorage = 0, hitbox = new Vector2(4, 4), description = "Advanced metal extractor to extract more resources" };
		buildingTypes["ConsTurret"] = new Building { texture = "ConsTurret", tier = 1, mCost = 500, eCost = 5000, mProduction = 0, eProduction = 0, buildTime = 15300, health = 560, damage = 0, range = 500, sightRange = 380, buildPower = 100, mStorage = 0, eStorage = 0, hitbox = new Vector2(3, 3), description = "Assists in building nearby units and buildings" };
		buildingTypes["Converter"] = new Building { texture = "Converter", tier = 1, mCost = 1, eCost = 1250, mProduction = -1, eProduction = 60, buildTime = 2680, health = 167, damage = 0, sightRange = 273, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(4, 4), description = "Slowly converts energy into metal" };
		buildingTypes["DTeeth"] = new Building { texture = "DTeeth", tier = 1, mCost = 8, eCost = 0, mProduction = 0, eProduction = 0, buildTime = 255, health = 2800, damage = 0, sightRange = 40, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(2, 2), description = "Wall to stop your enemies in their tracks" };
		buildingTypes["EStorage"] = new Building { texture = "EStorage", tier = 1, mCost = 175, eCost = 1800, mProduction = 0, eProduction = 0, buildTime = 4260, health = 2000, damage = 0, sightRange = 273, buildPower = 0, mStorage = 0, eStorage = 10000, hitbox = new Vector2(4, 4), description = "Increases your max energy capacity" };
		buildingTypes["Lab1"] = new Building { texture = "Lab1", tier = 1, mCost = 570, eCost = 1550, mProduction = 0, eProduction = 0, buildTime = 5650, health = 3000, damage = 0, range = 100, sightRange = 273, buildPower = 150, mStorage = 0, eStorage = 0, hitbox = new Vector2(8, 8), description = "T1 lab to produce T1 units" };
		buildingTypes["Lab2"] = new Building { texture = "Lab2", tier = 2, mCost = 2800, eCost = 16000, mProduction = 0, eProduction = 0, buildTime = 18500, health = 5100, damage = 0, range = 100, sightRange = 273, buildPower = 300, mStorage = 0, eStorage = 0, hitbox = new Vector2(8, 8), description = "T2 lab to produce T2 units" };
		buildingTypes["LLT"] = new Building { texture = "LLT", tier = 1, mCost = 90, eCost = 700, mProduction = 0, eProduction = 0, buildTime = 2500, health = 650, damage = 50, atkSpd = 20, range = 240, sightRange = 494, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(2, 2), projectile = "Laser", description = "Light laser turret that shoots at nearby enemies" };
		buildingTypes["Mex"] = new Building { texture = "Mex", tier = 1, mCost = 50, eCost = 500, mProduction = 5, eProduction = 0, buildTime = 1870, health = 270, damage = 0, sightRange = 273, buildPower = 1, hitbox = new Vector2(4, 4), mStorage = 0, eStorage = 0, description = "Metal extractor - Make sure to build it on a metal spot or else it will not make metal" };
		buildingTypes["MStorage"] = new Building { texture = "MStorage", tier = 1, mCost = 340, eCost = 590, mProduction = 0, eProduction = 0, buildTime = 2920, health = 2100, damage = 0, sightRange = 273, buildPower = 0, hitbox = new Vector2(4, 4), mStorage = 1000, eStorage = 0, description = "Increases your max metal capacity" };
		buildingTypes["Persecutor"] = new Building { texture = "Persecutor", tier = 2, mCost = 2500, eCost = 17000, mProduction = 0, eProduction = 0, buildTime = 25700, health = 4250, damage = 420, atkSpd = 60, range = 1390, sightRange = 416, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(5, 5), projectile = "Artillery", description = "Long range plasma battery that shreds anything in its range" };
		buildingTypes["Scorpion"] = new Building { texture = "Scorpion", tier = 2, mCost = 730, eCost = 14000, mProduction = 0, eProduction = 0, buildTime = 15000, health = 3000, damage = 825, atkSpd = 120, range = 413, sightRange = 546, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(3, 3), projectile = "Rocket", description = "Deals insane damage up close" };
		buildingTypes["Solar"] = new Building { texture = "Solar", tier = 1, mCost = 150, eCost = 0, mProduction = 0, eProduction = 20, buildTime = 2800, health = 355, damage = 0, sightRange = 273, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(5, 5), description = "Solar panel - Produces 60 energy per second" };
		buildingTypes["Turbine"] = new Building { texture = "Turbine", tier = 1, mCost = 43, eCost = 175, mProduction = 0, eProduction = 5, buildTime = 1680, health = 220, damage = 0, sightRange = 273, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(4, 3), description = "Wind turbine - Produces 15 energy per second" };
		buildingTypes["Warden"] = new Building { texture = "Warden", tier = 1, mCost = 360, eCost = 2700, mProduction = 0, eProduction = 0, buildTime = 9650, health = 2750, damage = 261, atkSpd = 50, range = 455, sightRange = 455, buildPower = 0, mStorage = 0, eStorage = 0, hitbox = new Vector2(5, 3), projectile = "Laser", description = "Heavy laser turret that outranges most T1 units" };

		//Sets up all projectile types
		projectileTypes["Laser"] = new Projectile { texture = "Laser", lifespan = 8, range = 20, hitbox = new Vector2(30, 5) };
		projectileTypes["Rocket"] = new Projectile { texture = "Rocket", lifespan = 10, range = 30, hitbox = new Vector2(40, 20) };
		projectileTypes["Plasma"] = new Projectile { texture = "Plasma", lifespan = 10, range = 50, hitbox = new Vector2(25, 25) };
		projectileTypes["Cannon"] = new Projectile { texture = "Cannon", lifespan = 15, range = 80, hitbox = new Vector2(30, 30) };
		projectileTypes["Artillery"] = new Projectile { texture = "Artillery", lifespan = 15, range = 100, hitbox = new Vector2(40, 25) };

		SoundEffect.MasterVolume = 0.2f;

		//Spawns player and enemy commanders
		Unit u = unitTypes["Commander"];
		u.isActive = true;
		SpawnUnit(u, new Vector2(400, 1850));
		SpawnEnemyUnit(u, new Vector2(MAP_WIDTH - 400, 1850));

		//Places all enemy buildings
		PlaceEnemyBuilding(buildingTypes["Warden"], new Vector2(2245, 935));
		PlaceEnemyBuilding(buildingTypes["Warden"], new Vector2(2245, 1780));
		PlaceEnemyBuilding(buildingTypes["Warden"], new Vector2(2245, 3140));
		PlaceEnemyBuilding(buildingTypes["Agitator"], new Vector2(3000, 955));
		PlaceEnemyBuilding(buildingTypes["Agitator"], new Vector2(3000, 3150));
		PlaceEnemyBuilding(buildingTypes["Scorpion"], new Vector2(3225, 1900));
		PlaceEnemyBuilding(buildingTypes["Mex"], new Vector2(2600, 1060));
		PlaceEnemyBuilding(buildingTypes["Mex"], new Vector2(2520, 1850));
		PlaceEnemyBuilding(buildingTypes["Mex"], new Vector2(1960, 1950));
		PlaceEnemyBuilding(buildingTypes["Mex"], new Vector2(2370, 3140));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3420, 700));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3600, 840));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3700, 1760));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3530, 1950));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3750, 2040));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3630, 2990));
		PlaceEnemyBuilding(buildingTypes["AMex"], new Vector2(3450, 3180));
		PlaceEnemyBuilding(buildingTypes["Lab1"], new Vector2(3370, 1720));
		PlaceEnemyBuilding(buildingTypes["Lab2"], new Vector2(3400, 2140));
	}

	//Logic that happens every frame
	protected override void Update(GameTime gameTime)
	{
		//Update keyboard and mouse states
		prevKb = kb;
		kb = Keyboard.GetState();
		prevMouse = mouse;
		mouse = Mouse.GetState();
		int scrollChange = mouse.ScrollWheelValue - prevMouse.ScrollWheelValue;

		//Decides what logic to do based on the game's state
		switch (gameState)
		{
			//If the game's state is currently PLAY
			case "PLAY":
				//Increments timer
				timer++;

				//Processes camera movement
				if (kb.IsKeyDown(Keys.W))
				{
					camPosition += new Vector2(0, -CAM_PAN_SPEED / camZoom);
				}
				if (kb.IsKeyDown(Keys.S))
				{
					camPosition += new Vector2(0, CAM_PAN_SPEED / camZoom);
				}
				if (kb.IsKeyDown(Keys.A))
				{
					camPosition += new Vector2(-CAM_PAN_SPEED / camZoom, 0);
				}
				if (kb.IsKeyDown(Keys.D))
				{
					camPosition += new Vector2(CAM_PAN_SPEED / camZoom, 0);
				}
				if (scrollChange != 0)
				{
					camZoom += Math.Sign(scrollChange) * 0.1f;
					camZoom = MathHelper.Clamp(camZoom, 0.1f, 10f);
				}

				//Update placing building hologram location
				if (buildingSelected)
				{
					placingBuilding.location = GridToWorld(WorldToGrid(new Vector2(ScreenToWorld(mouse.Position.ToVector2()).X, ScreenToWorld(mouse.Position.ToVector2()).Y)));
					placingBuilding.rec = new Rectangle((int)placingBuilding.location.X - (int)placingBuilding.hitbox.X / 2 * GRID_WIDTH, (int)placingBuilding.location.Y - (int)placingBuilding.hitbox.Y / 2 * GRID_HEIGHT, (int)placingBuilding.hitbox.X * GRID_WIDTH, (int)placingBuilding.hitbox.Y * GRID_HEIGHT);
				}

				//Clears the unit queue if Q is pressed
				if (kb.IsKeyDown(Keys.Q))
				{
					unitQueue.Clear();
				}

				//Removes the building at the mouse's location if E is pressed
				if (kb.IsKeyDown(Keys.E))
				{
					for (int i = 0; i < buildings.Count; i++)
					{
						if (buildings[i].rec.Contains(ScreenToWorld(mouse.Position.ToVector2())))
						{
							RemoveBuildings(buildings[i]);
						}
					}
				}

				//Enable cheats when tilde is pressed
				if (kb.IsKeyDown(Keys.OemTilde))
				{
					maxMetal = 99999999;
					maxEnergy = 99999999;
					metal = 99999999;
					energy = 99999999;
					fogOff = true;
				}

				//Spawn units when R is pressed and cheats are enabled
				if (kb.IsKeyDown(Keys.R))
				{
					if (fogOff)
					{
						SpawnUnit(unitTypes["Brute"], ScreenToWorld(mouse.Position.ToVector2()));
					}
				}

				//Removes the unit at the mouse's position when F is pressed
				if (kb.IsKeyDown(Keys.F))
				{
					for (int i = 0; i < units.Count; i++)
					{
						if (units[i].rec.Contains(ScreenToWorld(mouse.Position.ToVector2())))
						{
							Unit u = units[i];
							u.currentHealth = 0;
							units[i] = u;
						}
					}
				}

				//Pauses the game when the escape key is pressed
				if (kb.IsKeyDown(Keys.Escape) && !prevKb.IsKeyDown(Keys.Escape))
				{
					gameState = "PAUSE";
				}

				//Sets the start of a click and drag to the mouse's position
				if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
				{
					leftClickStart = ScreenToWorld(mouse.Position.ToVector2());
				}

				//Logic that runs during a left button press
				if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Pressed)
				{
					//Places the selected building if the mouse is not on the building select menu
					if (buildingSelected && (mouse.Position.X > 500 || mouse.Position.Y < 750))
					{
						//Places the building
						PlaceBuilding();

						//Moves units to the building location to build it
						for (int i = 0; i < selectedUnits.Count; i++)
						{
							//Checks if the unit can build and is close enough to help
							if (selectedUnits[i].buildPower > 0 && Math.Sqrt(Math.Pow(selectedUnits[i].location.X - placingBuilding.location.X - placingBuilding.hitbox.X * GRID_WIDTH / 2, 2) + Math.Pow(selectedUnits[i].location.Y - placingBuilding.location.Y - placingBuilding.hitbox.Y * GRID_WIDTH / 2, 2)) > selectedUnits[i].range - 50)
							{
								//Sets the units destination to the building's location and finds a path to that location
								Unit u = selectedUnits[i];
								u.destination = WorldToGrid(placingBuilding.location + placingBuilding.hitbox * GRID_WIDTH / 2 + Vector2.Normalize(u.location - (placingBuilding.location + placingBuilding.hitbox * GRID_WIDTH / 2)) * (u.range / 2));
								u.path = FindPath(WorldToGrid(u.location), u.destination);

								//Updates the selectedUnits list to match the units list
								selectedUnits[i] = u;
								for (int j = 0; j < units.Count; j++)
								{
									if (selectedUnits[i].id == units[j].id)
									{
										units[j] = selectedUnits[i];
										break;
									}
								}
							}
						}
					}
					//If the user is not placing a building, it makes the selection box
					else
					{
						Vector2 mousePos = ScreenToWorld(mouse.Position.ToVector2());
						selectBoxRec = new Rectangle((int)Math.Min(leftClickStart.X, mousePos.X), (int)Math.Min(leftClickStart.Y, mousePos.Y), (int)Math.Abs(leftClickStart.X - mousePos.X), (int)Math.Abs(leftClickStart.Y - mousePos.Y));
					}
				}

				//Logic that runs when the left button is released
				if (mouse.LeftButton != ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Pressed)
				{
					//Clears selection if Shift is not held, click is not on UI, and no building is selected
					if (!kb.IsKeyDown(Keys.LeftShift) && (mouse.Position.X > 500 || mouse.Position.Y < 750) && !buildingSelected)
					{
						selectedUnits.Clear();
						selectedBuilding.tier = -1;
					}
					else
					{
						//Checks if a building was selected from the build menu
						for (int i = 0; i < buildMenu.Count; i++)
						{
							if (buildMenu[i].rec.Contains(mouse.Position.ToVector2()))
							{
								placingBuilding = buildingTypes[buildMenu[i].texture];
								buildingSelected = true;
								break;
							}
						}

						//Checks if a unit was queued from the unit menu
						for (int i = 0; i < unitMenu.Count; i++)
						{
							if (unitMenu[i].rec.Contains(mouse.Position.ToVector2()))
							{
								Unit u = unitTypes[unitMenu[i].texture];
								u.buildCounter = 0;

								//Spawns the unit at the selected building
								u.location = new Vector2(selectedBuilding.location.X + selectedBuilding.hitbox.X * GRID_WIDTH - 50, selectedBuilding.location.Y + selectedBuilding.hitbox.Y * GRID_HEIGHT / 2);

								//Adds the unit to the unit production queue
								unitQueue.Enqueue(u);
							}
						}
					}

					//Selects units if no building is being placed
					if (!buildingSelected)
					{
						for (int i = 0; i < units.Count; i++)
						{
							//Selects units inside the selection box or directly clicked
							if ((selectBoxRec.Contains(units[i].location) || units[i].rec.Contains(ScreenToWorld(mouse.Position.ToVector2()))) && units[i].isActive)
							{
								selectedUnits.Add(units[i]);
							}
						}

						//Clears the selection box
						selectBoxRec = new Rectangle(0, 0, 0, 0);
					}

					//Checks if a building was selected
					for (int i = 0; i < buildings.Count; i++)
					{
						if (selectBoxRec.Contains(buildings[i].location) || buildings[i].rec.Contains(ScreenToWorld(mouse.Position.ToVector2())) && mouse.Position.X > 500 && mouse.Position.Y < 750)
						{
							selectedBuilding = buildings[i];
							break;
						}
					}
				}

				//Records the starting position of a right-click drag
				if (mouse.RightButton == ButtonState.Pressed && prevMouse.RightButton != ButtonState.Pressed)
				{
					rightClickStart = ScreenToWorld(mouse.Position.ToVector2());
				}

				//Logic that runs when the right mouse button is released
				if (mouse.RightButton != ButtonState.Pressed && prevMouse.RightButton == ButtonState.Pressed)
				{
					//Cancels building placement if one was selected
					if (buildingSelected)
					{
						buildingSelected = false;
					}
					//Otherwise gives a movement command to selected units
					else
					{
						MakeUnitSpread(rightClickStart, ScreenToWorld(mouse.Position.ToVector2()));
					}
				}

				//Handles building construction progress
				for (int i = 0; i < buildings.Count; i++)
				{
					//If the building is still under construction
					if (!buildings[i].isActive)
					{
						//Units contribute build power if in range
						for (int j = 0; j < units.Count; j++)
						{
							if (Math.Sqrt(Math.Pow(units[j].location.X - buildings[i].location.X - buildings[i].hitbox.X * GRID_WIDTH / 2, 2) + Math.Pow(units[j].location.Y - buildings[i].location.Y - buildings[i].hitbox.Y * GRID_WIDTH / 2, 2)) < units[j].range)
							{
								Building b = buildings[i];
								b.buildCounter += units[j].buildPower / 50;
								buildings[i] = b;
							}
						}

						//Nearby active buildings also contribute build power
						for (int j = 0; j < buildings.Count; j++)
						{
							if (buildings[j].isActive && Math.Sqrt(Math.Pow(buildings[j].location.X - buildings[i].location.X - buildings[i].hitbox.X * GRID_WIDTH / 2, 2) + Math.Pow(buildings[j].location.Y - buildings[i].location.Y - buildings[i].hitbox.Y * GRID_WIDTH / 2, 2)) < buildings[j].range)
							{
								Building b = buildings[i];
								b.buildCounter += buildings[j].buildPower / 50;
								buildings[i] = b;
							}
						}
					}

					//Activates the building once it is built
					if (buildings[i].buildCounter >= buildings[i].buildTime)
					{
						Building b = buildings[i];
						b.isActive = true;
						b.buildCounter = 0;
						buildings[i] = b;
					}
				}

				//Handles unit construction progress
				for (int i = 0; i < units.Count; i++)
				{
					//Units build from nearby active buildings' buildPower
					if (!units[i].isActive)
					{
						for (int j = 0; j < buildings.Count; j++)
						{
							if (buildings[j].isActive && Math.Sqrt(Math.Pow(buildings[j].location.X + buildings[j].hitbox.X * GRID_WIDTH / 2 - units[i].location.X, 2) + Math.Pow(buildings[j].location.Y + buildings[j].hitbox.Y * GRID_WIDTH / 2 - units[i].location.Y, 2)) < buildings[j].range)
							{
								Unit u = units[i];
								u.buildCounter += buildings[j].buildPower / 15;
								units[i] = u;
							}
						}
					}

					//Activates the unit when it is constructed
					if (metal > units[i].mCost && energy > units[i].eCost && units[i].buildCounter >= units[i].buildTime && !units[i].isActive)
					{
						Unit u = units[i];
						u.isActive = true;
						u.buildCounter = 0;
						u.location.X += 100;
						u.rec.X += 100;
						metal -= u.mCost;
						energy -= u.eCost;
						unitMaking = false;
						units[i] = u;
					}
					else if (metal <= units[i].mCost || energy <= units[i].eCost)
					{
						Unit u = units[i];
						u.buildCounter = u.buildTime;
						units[i] = u;
					}
				}

				//Updates movement and path-following logic for all player units
				for (int i = 0; i < units.Count; i++)
				{
					//Copy the unit so it can be modified
					Unit u = units[i];

					//Only move the unit if it has a path
					if (u.path != null && u.path.Count > 0)
					{
						//Gets the next grid tile in the path
						Point next = u.path.Peek();

						//Converts the grid tile to world space and centers it
						Vector2 nextWorld = GridToWorld(next) + new Vector2(GRID_WIDTH / 2, GRID_HEIGHT / 2); //center of tile

						//Direction from the unit to the next tile
						Vector2 dir = nextWorld - u.location;

						//Distance the unit can move this frame
						float dist = u.moveSpd * (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

						//If the unit can reach the next tile this frame
						if (dir.Length() <= dist)
						{
							//Put the unit to the centr of the tile
							u.location = nextWorld;

							//Removes the tile from the path when it is reached
							if (u.path != null && u.path.Count > 0)
							{
								u.path.Dequeue();
							}
						}
						//Else, move the unit toward the tile
						else
						{
							u.location += Vector2.Normalize(dir) * dist;
						}

						//Updates the unit's rectangle
						u.rec = new Rectangle((int)(u.location.X - u.hitbox.X / 2), (int)(u.location.Y - u.hitbox.Y / 2), (int)u.hitbox.X, (int)u.hitbox.Y);

						//Rotates the unit to its movement direction
						u.rotation = Math.Atan2(dir.X, -dir.Y);
					}

					//Stores the unit back into the main list
					units[i] = u;

					//updates selectedUnits to match the changes in units
					for (int j = 0; j < selectedUnits.Count; j++)
					{
						if (units[i].id == selectedUnits[j].id)
						{
							selectedUnits[j] = units[i];
						}
					}
				}

				//Updates movement and path following logic for all enemy units
				for (int i = 0; i < enemyUnits.Count; i++)
				{
					//Copy the unit so it can be modified
					Unit u = enemyUnits[i];

					//Only move the unit if it has a path
					if (u.path != null && u.path.Count > 0)
					{
						//Gets the next grid tile in the path
						Point next = u.path.Peek();

						//Converts the grid tile to world space and centers it
						Vector2 nextWorld = GridToWorld(next) + new Vector2(GRID_WIDTH / 2, GRID_HEIGHT / 2); //center of tile

						//Direction from the unit to the next tile
						Vector2 dir = nextWorld - u.location;

						//Distance the unit can move this frame
						float dist = u.moveSpd * (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

						//If the unit can reach the next tile this frame
						if (dir.Length() <= dist)
						{
							//Put the unit to the centr of the tile
							u.location = nextWorld;

							//Removes the tile from the path when it is reached
							if (u.path != null && u.path.Count > 0)
							{
								u.path.Dequeue();
								enemyUnits[i] = u;
								continue;
							}
						}
						//Else, move the unit toward the tile
						else
						{
							u.location += Vector2.Normalize(dir) * dist;
						}

						//Updates the unit's rectangle
						u.rec = new Rectangle((int)(u.location.X - u.hitbox.X / 2), (int)(u.location.Y - u.hitbox.Y / 2), (int)u.hitbox.X, (int)u.hitbox.Y);

						//Rotates the unit to its movement direction
						u.rotation = Math.Atan2(dir.X, -dir.Y);
					}

					//Stores the unit back into the main list
					enemyUnits[i] = u;
				}

				//Used for the position of things in the build and unit menus
				int counter = 0;

				//Clears the previous build and unit selection menus
				buildMenu.Clear();
				unitMenu.Clear();

				//Builds the building menu based on selected units
				for (int i = 0; i < selectedUnits.Count; i++)
				{
					//Only units the can build can open the build menu
					if (selectedUnits[i].buildPower > 0)
					{
						for (int j = 0; j < buildingTypes.Count; j++)
						{
							//Checks if the constructor can build this building
							if (buildingTypes.ElementAt(j).Value.tier == selectedUnits[i].tier || (buildingTypes.ElementAt(j).Value.texture == "Lab2" && selectedUnits[i].texture == "Constructor"))
							{
								Building b = buildingTypes.ElementAt(j).Value;
								bool menuContains = false;

								//Prevents duplicate buildings from appearing in the menu
								for (int k = 0; k < buildMenu.Count; k++)
								{
									if (b.texture.Equals(buildMenu[k].texture))
									{
										menuContains = true;
										counter--;
										break;
									}
								}

								//Adds the building to the build menu if not already present
								if (!menuContains)
								{
									b.rec = new Rectangle(100 * (counter % 5), 750 + 100 * (int)Math.Floor(counter / 5d), 100, 100);
									buildMenu.Add(b);
								}

								counter++;
							}
						}
					}
				}

				//Makes the unit production menu if a lab is selected
				if (selectedBuilding.texture == "Lab1" || selectedBuilding.texture == "Lab2")
				{
					for (int i = 0; i < unitTypes.Count; i++)
					{
						//Only shows units of the same tier, excluding commanders
						if (unitTypes.ElementAt(i).Value.tier == selectedBuilding.tier && unitTypes.ElementAt(i).Value.texture != "Commander")
						{
							Unit u = unitTypes.ElementAt(i).Value;

							//Positions the unit icon in the menu grid and adds it to the menu
							u.rec = new Rectangle(100 * (counter % 5), 750 + 100 * (int)Math.Floor(counter / 5d), 100, 100);
							unitMenu.Add(u);
							counter++;
						}
					}
				}

				//Processes the unit production queue if no unit is currently being built
				if (unitMaking == false && unitQueue.Count > 0)
				{
					Unit u = unitQueue.Peek();

					//Only spawns the unit if there are enough resources
					if (metal >= u.mCost && energy >= u.eCost)
					{
						unitQueue.Dequeue();
						SpawnUnit(u, u.location);
						unitMaking = true;
					}
				}

				//Produces resources every half second
				if (timer % 30 == 0)
				{
					ProduceRes();
				}

				//Resets the sound counter every second
				if (timer % 60 == 0)
				{
					soundsThisSecond = 0;
				}

				//enemy AI logic that runs once per second
				if (timer % 60 == 0)
				{
					//Spawns enemy units to match the player's army strength
					if (playerArmyValue + 2 > enemyArmyValue)
					{
						List<Unit> unitChoice;
						unitChoice = unitTypes.Values.ToList();
						unitChoice = BuildEnemyWave(unitChoice, playerArmyValue + 2 - enemyArmyValue);

						if (unitChoice.Count > 0)
						{
							for (int i = 0; i < unitChoice.Count; i++)
							{
								//Spawns enemy units near the edge of the map
								Unit u = unitChoice[i];
								u.location = new Vector2(MAP_WIDTH - 100, 1000 + rng.Next(0, 2000));
								SpawnEnemyUnit(u, u.location);
							}
						}
					}

					//Updates enemy movement destinations every 5 seconds
					if (timer % 300 == 0)
					{
						for (int i = 0; i < enemyUnits.Count; i++)
						{
							Unit u = enemyUnits[i];

							//Enemy non commander units target player's buildings
							if (!u.texture.Equals("Commander"))
							{
								//Default target is the player's commander
								u.destination = WorldToGrid(units[0].location);

								//Occasionally targets a building
								for (int j = 0; j < buildings.Count; j++)
								{
									if (rng.Next(0, 10) == 0 && buildings[j].buildPower > 0 && !blockedGrid[WorldToGrid(buildings[j].location - new Vector2(2, -2)).X, WorldToGrid(buildings[j].location - new Vector2(2, -2)).Y])
									{
										u.destination = WorldToGrid(buildings[j].location - new Vector2(2, -2));
									}
								}

								//Clamps destination to map boundaries
								u.destination.X = Math.Clamp(u.destination.X, 0, MAP_WIDTH / GRID_WIDTH - 1);
								u.destination.Y = Math.Clamp(u.destination.Y, 0, MAP_HEIGHT / GRID_HEIGHT - 1);
							}
							//Commander moves semi randomly
							else
							{
								u.destination = new Point(MAP_WIDTH / GRID_WIDTH - rng.Next(0, 30), MAP_WIDTH / GRID_WIDTH / 2 - 50 + rng.Next(0, 100));
							}

							//Recalculates the enemy unit's path
							u.path = FindPath(WorldToGrid(u.location), u.destination);
							enemyUnits[i] = u;
						}
					}
				}

				//Periodically plays a moving sound and resets a lab's cooldown
				if (timer % 600 == 1)
				{
					SoundEffectInstance moveSound = movingSFX.CreateInstance();
					moveSound.Volume = 0.1f;
					moveSound.Play();
				}

				//Removes destroyed enemy buildings
				for (int i = 0; i < enemyBuildings.Count; i++)
				{
					if (enemyBuildings[i].currentHealth <= 0)
					{
						RemoveEnemyBuilding(enemyBuildings[i]);
						i--;
					}
				}

				//Removes destroyed player buildings
				for (int i = 0; i < buildings.Count; i++)
				{
					if (buildings[i].currentHealth <= 0)
					{
						RemoveBuildings(buildings[i]);
						i--;
					}
				}

				//Updates explosion animations
				for (int i = 0; i < explosions.Length; i++)
				{
					if (explosions[i].IsAnimating())
					{
						explosions[i].Update(gameTime);
					}
				}

				//Cleans up dead units, resolves combat, updates fog of war, and procceses win conditions
				RemoveUnits();
				RemoveEnemyUnits();
				UnitCollisions();
				UpdateUnitAttacks();
				UpdateBuildingAttacks();
				UpdateProjectiles();
				UpdateFog();
				ProcessWins();


				//Ensures that resources don't exceed their max values
				if (metal > maxMetal)
				{
					metal = maxMetal;
				}
				if (energy > maxEnergy)
				{
					energy = maxEnergy;
				}

				//Updates the camera transformation matrix
				camTransform = CamTransform();

				break;

			case "MENU":
				//Detects when left mouse button is released
				if (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
				{
					//Disables the papyrus menu by default
					papyrusActive = false;

					//Starts the game if singleplayer button is clicked
					if (button1Rec.Contains(mouse.Position))
					{
						gameState = "PLAY";
					}
					//Activates the papyrus menu if either info button or the multiplayer button is clicked
					else if (button2Rec.Contains(mouse.Position) || button3Rec.Contains(mouse.Position))
					{
						papyrusActive = true;
					}
					//Exits the game if the Exit button is clicked
					else if (button4Rec.Contains(mouse.Position))
					{
						Exit();
					}
				}
				break;

			case "PAUSE":
				//Resumes the game when Escape is pressed
				if (kb.IsKeyDown(Keys.Escape) && !prevKb.IsKeyDown(Keys.Escape))
				{
					gameState = "PLAY";
				}

				break;
		}

		//Proccesses the game update
		base.Update(gameTime);
	}

	//Draws the current frame
	protected override void Draw(GameTime gameTime)
	{
		//Clears the screen with a dark gray background
		GraphicsDevice.Clear(Color.DimGray);

		//Draw logic depends on the current game state
		switch (gameState)
		{
			case "PLAY":
				//Begins drawing things in world coordinates
				_spriteBatch.Begin(transformMatrix: camTransform);

				//Draws the map
				_spriteBatch.Draw(mapImg, mapRec, Color.White);

				//Draws all player buildings
				for (int i = 0; i < buildings.Count(); i++)
				{
					//Draws completed buildings
					if (buildings[i].isActive)
					{
						//Switches to additive blending for highlight on selected units
						_spriteBatch.End();
						_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, transformMatrix: camTransform);

						//Draws a glowing outline on each selected building
						if (buildings[i].Equals(selectedBuilding))
						{
							for (int j = 0; j < 8; j++)
							{
								_spriteBatch.Draw(textures[buildings[i].texture], new Rectangle((int)buildings[i].rec.X - 3, (int)buildings[i].rec.Y - 3, (int)buildings[i].hitbox.X * GRID_WIDTH + 6, (int)buildings[i].hitbox.Y * GRID_HEIGHT + 6), Color.White);
							}
						}

						//Returns to normal drawing
						_spriteBatch.End();
						_spriteBatch.Begin(transformMatrix: camTransform);

						//Draws the building
						_spriteBatch.Draw(textures[buildings[i].texture], new Rectangle((int)buildings[i].rec.X, (int)buildings[i].rec.Y, (int)buildings[i].hitbox.X * GRID_WIDTH, (int)buildings[i].hitbox.Y * GRID_HEIGHT), Color.White);
					}
					//Draws constructing buildings
					else
					{
						_spriteBatch.Draw(textures[buildings[i].texture], new Rectangle((int)buildings[i].rec.X, (int)buildings[i].rec.Y, (int)buildings[i].hitbox.X * GRID_WIDTH, (int)buildings[i].hitbox.Y * GRID_HEIGHT), Color.White * 0.5f);

						//Draws the construction progress bar
						DrawProgressBar(buildings[i].location, buildings[i].hitbox, buildings[i].buildCounter, buildings[i].buildTime, Color.YellowGreen);
					}
				}

				//Draws the hologram of a building being placed
				if (buildingSelected)
				{
					//If the placement if valid
					if (IsAreaFree(WorldToGrid(placingBuilding.location), (int)placingBuilding.hitbox.X, (int)placingBuilding.hitbox.Y))
					{
						_spriteBatch.Draw(textures[placingBuilding.texture], placingBuilding.rec, Color.White * 0.6f);
					}
					//If the placement is invalid
					else
					{
						_spriteBatch.Draw(textures[placingBuilding.texture], placingBuilding.rec, Color.LightPink * 0.8f);
					}
				}

				//Draws all player units
				for (int i = 0; i < units.Count(); i++)
				{
					//Draws active units
					if (units[i].isActive)
					{
						//Highlights selected units
						for (int j = 0; j < selectedUnits.Count(); j++)
						{
							if (units[i].id == selectedUnits[j].id)
							{
								_spriteBatch.End();
								_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, transformMatrix: camTransform);

								//Draws a glowing outline around all selected units
								for (int k = 0; k < 8; k++)
								{
									_spriteBatch.Draw(textures[units[i].texture], new Vector2(units[i].rec.X + units[i].rec.Width * 0.5f, units[i].rec.Y + units[i].rec.Height * 0.5f), null, Color.White, (float)units[i].rotation, new Vector2(textures[units[i].texture].Width * 0.5f, textures[units[i].texture].Height * 0.5f), new Vector2((units[i].rec.Width + 6) / (float)textures[units[i].texture].Width, (units[i].rec.Height + 6) / (float)textures[units[i].texture].Height), SpriteEffects.None, 0f);
								}

								_spriteBatch.End();
								_spriteBatch.Begin(transformMatrix: camTransform);
								break;
							}
						}

						//Draws the unit
						_spriteBatch.Draw(textures[units[i].texture], new Vector2(units[i].rec.X + units[i].rec.Width * 0.5f, units[i].rec.Y + units[i].rec.Height * 0.5f), null, Color.White, (float)units[i].rotation, new Vector2(textures[units[i].texture].Width * 0.5f, textures[units[i].texture].Height * 0.5f), new Vector2(units[i].rec.Width / (float)textures[units[i].texture].Width, units[i].rec.Height / (float)textures[units[i].texture].Height), SpriteEffects.None, 0f);

						//Draws a movement line if the destination is far enough away
						if (Math.Sqrt(Math.Pow(units[i].location.X - GridToWorld(units[i].destination).X, 2) + Math.Pow(units[i].location.Y - GridToWorld(units[i].destination).Y, 2)) > 200)
						{
							DrawLine(units[i].location, GridToWorld(units[i].destination), Color.LightGreen);
						}
					}
					//Draws constructing units
					else
					{
						_spriteBatch.Draw(textures[units[i].texture], new Vector2(units[i].rec.X + units[i].rec.Width * 0.5f, units[i].rec.Y + units[i].rec.Height * 0.5f), null, Color.White, (float)units[i].rotation, new Vector2(textures[units[i].texture].Width * 0.5f, textures[units[i].texture].Height * 0.5f), new Vector2(units[i].rec.Width / (float)textures[units[i].texture].Width, units[i].rec.Height / (float)textures[units[i].texture].Height), SpriteEffects.None, 0f);

						//Draws the unit construction progress bar
						DrawProgressBar(new Vector2(units[i].location.X - units[i].hitbox.X / 2, units[i].location.Y - units[i].hitbox.Y / 2), new Vector2(units[i].hitbox.X / GRID_WIDTH, units[i].hitbox.Y / GRID_HEIGHT), units[i].buildCounter, units[i].buildTime, Color.YellowGreen);
					}
				}

				//Draws visible enemy buildings
				for (int i = 0; i < enemyBuildings.Count; i++)
				{
					if (fog[(int)enemyBuildings[i].location.X / GRID_WIDTH, (int)enemyBuildings[i].location.Y / GRID_HEIGHT] == 2 || fogOff)
					{
						_spriteBatch.Draw(textures[enemyBuildings[i].texture], new Rectangle((int)enemyBuildings[i].rec.X, (int)enemyBuildings[i].rec.Y, (int)enemyBuildings[i].hitbox.X * GRID_WIDTH, (int)enemyBuildings[i].hitbox.Y * GRID_HEIGHT), null, Color.LightSkyBlue, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
					}
				}

				//Draws visible enemy units
				for (int i = 0; i < enemyUnits.Count; i++)
				{
					if (fog[(int)enemyUnits[i].location.X / GRID_WIDTH, (int)enemyUnits[i].location.Y / GRID_HEIGHT] == 2 || fogOff)
					{
						_spriteBatch.Draw(textures[enemyUnits[i].texture], new Vector2(enemyUnits[i].rec.X + enemyUnits[i].rec.Width * 0.5f, enemyUnits[i].rec.Y + enemyUnits[i].rec.Height * 0.5f), null, Color.LightSkyBlue, (float)enemyUnits[i].rotation, new Vector2(textures[enemyUnits[i].texture].Width * 0.5f, textures[enemyUnits[i].texture].Height * 0.5f), new Vector2(enemyUnits[i].rec.Width / (float)textures[enemyUnits[i].texture].Width, enemyUnits[i].rec.Height / (float)textures[enemyUnits[i].texture].Height), SpriteEffects.None, 0f);
					}
				}

				//Draws fog of war
				if (!fogOff)
				{
					for (int x = 0; x < 200; x++)
					{
						for (int y = 0; y < 200; y++)
						{
							if (fog[x, y] == 2)
							{
								continue;
							}

							if (fog[x, y] == 0 || fog[x, y] == 1)
							{
								_spriteBatch.Draw(pixel, new Rectangle(x * GRID_WIDTH, y * GRID_HEIGHT, GRID_WIDTH, GRID_HEIGHT), new Color(35, 35, 40) * 0.85f);
							}
						}
					}
				}

				//Draws all active projectiles
				for (int i = 0; i < projectiles.Count; i++)
				{
					_spriteBatch.Draw(textures[projectiles[i].texture], new Rectangle(projectiles[i].rec.X, projectiles[i].rec.Y, (int)projectiles[i].hitbox.X, (int)projectiles[i].hitbox.Y), null, Color.White, projectiles[i].rotation, Vector2.Zero, SpriteEffects.None, 0f);
				}

				//Draws explosion animations
				for (int i = 0; i < explosions.Length; i++)
				{
					if (explosions[i].IsAnimating())
					{
						explosions[i].Draw(_spriteBatch, Color.White);
					}
				}

				//Draws the unit selection box
				DrawSelectionRect(selectBoxRec, 6, Color.LightGreen);

				//Switches to screen coordinate rendering for UI
				_spriteBatch.End();
				_spriteBatch.Begin();

				//Draws build menu
				for (int i = 0; i < buildMenu.Count; i++)
				{
					//Draws building texture
					_spriteBatch.Draw(textures[buildMenu[i].texture], buildMenu[i].rec, Color.White);

					//Draws building resource costs
					_spriteBatch.DrawString(smallFont, Convert.ToString(buildMenu[i].mCost), new Vector2(buildMenu[i].rec.X + 5, buildMenu[i].rec.Y + 80), Color.White);
					_spriteBatch.DrawString(smallFont, Convert.ToString(buildMenu[i].eCost), new Vector2(buildMenu[i].rec.X + 95 - smallFont.MeasureString(Convert.ToString(buildMenu[i].eCost)).X, buildMenu[i].rec.Y + 80), Color.Yellow);

					//Draws building description
					if (buildMenu[i].rec.Contains(mouse.Position))
					{
						_spriteBatch.DrawString(medFont, buildMenu[i].description, new Vector2(SCREEN_WIDTH / 2 - medFont.MeasureString(buildMenu[i].description).X / 2, SCREEN_HEIGHT - 100), Color.White);
					}
				}

				//Draws unit menu
				for (int i = 0; i < unitMenu.Count; i++)
				{
					//Draws unit texture
					_spriteBatch.Draw(textures[unitMenu[i].texture], unitMenu[i].rec, Color.White);

					//Draws unit resource costs
					_spriteBatch.DrawString(smallFont, Convert.ToString(unitMenu[i].mCost), new Vector2(unitMenu[i].rec.X + 5, unitMenu[i].rec.Y + 80), Color.White);
					_spriteBatch.DrawString(smallFont, Convert.ToString(unitMenu[i].eCost), new Vector2(unitMenu[i].rec.X + 95 - smallFont.MeasureString(Convert.ToString(unitMenu[i].eCost)).X, unitMenu[i].rec.Y + 80), Color.Yellow);

					//Draws unit description
					if (unitMenu[i].rec.Contains(mouse.Position))
					{
						_spriteBatch.DrawString(medFont, unitMenu[i].description, new Vector2(SCREEN_WIDTH / 2 - medFont.MeasureString(unitMenu[i].description).X / 2, SCREEN_HEIGHT - 100), Color.White);
					}
				}

				//Draws metal and energy counts
				_spriteBatch.DrawString(largeFont, "Metal: " + Convert.ToString(metal) + "/" + Convert.ToString(maxMetal), new Vector2(SCREEN_WIDTH / 2 - largeFont.MeasureString("Metal: " + Convert.ToString(metal) + "/" + Convert.ToString(maxMetal)).X - 50, 100), Color.White);
				_spriteBatch.DrawString(largeFont, "Energy: " + Convert.ToString(energy) + "/" + Convert.ToString(maxEnergy), new Vector2(SCREEN_WIDTH / 2 + 50, 100), Color.Yellow);

				//Draws unit queue images
				Unit[] arr = unitQueue.ToArray();
				int counter = 0;
				for (int i = 0; i < arr.Length; i++)
				{
					arr[i].rec = new Rectangle(SCREEN_WIDTH - 75, 100 + counter * 50, 50, 50);
					_spriteBatch.Draw(textures[arr[i].texture], arr[i].rec, Color.White);
					counter++;
				}

				//Draws unit queue text
				_spriteBatch.DrawString(medFont, "Unit Queue", new Vector2(SCREEN_WIDTH - 50 - medFont.MeasureString("Unit Queue").X, 50), Color.White);

				//Draws commander health text
				_spriteBatch.DrawString(medFont, "Commander Health: " + units[0].currentHealth + " / " + units[0].health, new Vector2(SCREEN_WIDTH - 600, SCREEN_HEIGHT - 120), Color.White);

				break;

			case "ENDGAME":
				//Begins drawing things in screen coordinates
				_spriteBatch.Begin();

				//If the player won
				if (win)
				{
					//Draws the win screen
					_spriteBatch.Draw(winScreenImg, winScreenRec, Color.White);
				}
				//If the player lost
				else
				{
					//Draws the lose screen
					_spriteBatch.Draw(loseScreenImg, loseScreenRec, Color.White);
				}

				break;

			case "MENU":
				//Begins drawing in screen coordinates
				_spriteBatch.Begin();

				//If the papyrus menu is not active
				if (!papyrusActive)
				{
					//Draws the title screen normally
					_spriteBatch.Draw(titleScreenImg, titleScreenRec, Color.White);
				}
				else
				{
					//Draws the title screen darker
					_spriteBatch.Draw(titleScreenImg, titleScreenRec, Color.White * 0.6f);

					//Draws the papyrus menu on top
					_spriteBatch.Draw(papyrusImg, papyrusRec, Color.White);
				}

				break;

			case "PAUSE":
				//Begins drawing in screen coordinates
				_spriteBatch.Begin();

				//Draws the pause screen menu
				_spriteBatch.Draw(pauseScreenImg, pauseScreenRec, Color.White);

				break;
		}

		//Updates all draws
		_spriteBatch.End();
		base.Draw(gameTime);
	}

	//Returns the transform matrix to convert world cooordinates into screen coordinates
	private Matrix CamTransform()
	{
		//Moves the world by camera position, zooms, and centers on the viewport
		return Matrix.CreateTranslation(new Vector3(-camPosition, 0)) * Matrix.CreateScale(camZoom) * Matrix.CreateTranslation(new Vector3(camViewport.Width * 0.5f, camViewport.Height * 0.5f, 0));
	}

	//Converts world coordinates to grid coordinates
	private Point WorldToGrid(Vector2 worldPos)
	{
		return new Point((int)(worldPos.X / GRID_WIDTH), (int)(worldPos.Y / GRID_HEIGHT));
	}

	//Converts grid coordinates to world coordinates
	private Vector2 GridToWorld(Point gridPos)
	{
		return new Vector2(gridPos.X * GRID_WIDTH, gridPos.Y * GRID_HEIGHT);
	}

	//Converts screen coordinates into world coordinates
	private Vector2 ScreenToWorld(Vector2 screenPos)
	{
		return Vector2.Transform(screenPos, Matrix.Invert(camTransform));
	}

	//Marks a piece of terrain on grid as blocked
	void BlockArea(int startX, int startY, int width, int height)
	{
		for (int x = 0; x < width - startX; x++)
		{
			for (int y = 0; y < height - startY; y++)
			{
				blockedGrid[startX + x, startY + y] = true;
			}
		}
	}

	//Calculates valid building positions snapped to the grid
	Vector2 GetBuildingPos(Vector2 worldPos, int widthTiles, int heightTiles)
	{
		//Converts world position to grid position
		Point gridPos = WorldToGrid(worldPos);

		//Cancels placement if the area is not free
		if (!IsAreaFree(gridPos, widthTiles, heightTiles))
		{
			return new Vector2(0, 0);
		}

		//Marks the grid tiles as occupied
		for (int x = 0; x < widthTiles; x++)
		{
			for (int y = 0; y < heightTiles; y++)
			{
				buildingGrid[gridPos.X + x, gridPos.Y + y] = true;
			}
		}

		//Converts the grid position back to world space
		Vector2 realPos = GridToWorld(gridPos);

		return realPos;
	}

	//Checks whether a grid area is free for buildings
	bool IsAreaFree(Point gridPos, int widthTiles, int heightTiles)
	{
		for (int x = 0; x < widthTiles; x++)
		{
			for (int y = 0; y < heightTiles; y++)
			{
				int gridX = gridPos.X + x;
				int gridY = gridPos.Y + y;

				//Prevents placement outside the map bounds
				if (gridX < 0 || gridX >= MAP_WIDTH / GRID_WIDTH || gridY < 0 || gridY >= MAP_HEIGHT / GRID_HEIGHT)
				{
					return false;
				}

				//Prevents placement inside existing buildings
				if (buildingGrid[gridX, gridY])
				{
					return false;
				}

				//Prevents placement on blocked terrain
				if (blockedGrid[gridX, gridY])
				{
					return false;
				}
			}
		}
		return true;
	}

	//Places a player building if player has enough resources
	void PlaceBuilding()
	{
		if (metal >= placingBuilding.mCost && energy >= placingBuilding.eCost)
		{
			//Calculates the grid snapped placement position at mouse coordinates
			Vector2 pos = GetBuildingPos(new Vector2(ScreenToWorld(mouse.Position.ToVector2()).X - (int)placingBuilding.hitbox.X / 2 * GRID_WIDTH, ScreenToWorld(mouse.Position.ToVector2()).Y - (int)placingBuilding.hitbox.Y / 2 * GRID_WIDTH), (int)placingBuilding.hitbox.X, (int)placingBuilding.hitbox.Y);

			//If the placement is valid
			if (pos != Vector2.Zero)
			{
				//Removes resources
				metal -= placingBuilding.mCost;
				energy -= placingBuilding.eCost;

				//Initializes the building's variables
				Building b = placingBuilding;
				b.currentHealth = b.health;
				b.location = pos;
				b.rec = new Rectangle((int)b.location.X, (int)b.location.Y, (int)placingBuilding.hitbox.X * GRID_WIDTH, (int)placingBuilding.hitbox.Y * GRID_HEIGHT);
				b.isActive = false;

				//Increases resource capacity by the building's capacity
				maxMetal += b.mStorage;
				maxEnergy += b.eStorage;

				//Adds the building to the player's buildings list
				buildings.Add(b);
			}
		}
	}

	//Places an enemy building at a position
	void PlaceEnemyBuilding(Building building, Vector2 location)
	{
		building.location = location;
		building.currentHealth = building.health;
		building.rec = new Rectangle((int)building.location.X, (int)building.location.Y, (int)building.hitbox.X * GRID_WIDTH, (int)building.hitbox.Y * GRID_HEIGHT);
		building.isActive = true;
		building.buildCounter = 0;
		building.tier = -1;
		enemyBuildings.Add(building);
	}


	void RemoveEnemyBuilding(Building building)
	{
		//Converts the building's world position into its grid positiom
		Point pos = new Point((int)building.location.X / GRID_WIDTH, (int)building.location.Y / GRID_HEIGHT);

		//Iterates over every tile the building takes up
		for (int x = 0; x < building.hitbox.X; x++)
		{
			for (int y = 0; y < building.hitbox.Y; y++)
			{
				//Calculates the current grid tile being cleared
				int gx = pos.X + x;
				int gy = pos.Y + y;

				//makes sure that the grid coordinates are within map bounds
				if (gx >= 0 && gx < MAP_WIDTH / GRID_WIDTH && gy >= 0 && gy < MAP_HEIGHT / GRID_HEIGHT)
				{
					//Frees the grid tile that was occupied by the building
					buildingGrid[gx, gy] = false;
				}
			}
		}
		//Removes the building from the enemy buildings list
		enemyBuildings.Remove(building);
	}

	void RemoveBuildings(Building building)
	{
		//Converts the building's world position into its grid positiom
		Point pos = new Point((int)building.location.X / GRID_WIDTH, (int)building.location.Y / GRID_HEIGHT);

		//Iterates over every tile the building takes up
		for (int x = 0; x < building.hitbox.X; x++)
		{
			for (int y = 0; y < building.hitbox.Y; y++)
			{
				//Calculates the current grid tile being cleared
				int gx = pos.X + x;
				int gy = pos.Y + y;

				//makes sure that the grid coordinates are within map bounds
				if (gx >= 0 && gx < MAP_WIDTH / GRID_WIDTH && gy >= 0 && gy < MAP_HEIGHT / GRID_HEIGHT)
				{
					//Frees the grid tile that was occupied by the building
					buildingGrid[gx, gy] = false;
				}
			}
		}

		//Updates resource storages based on what building was removed
		maxMetal -= building.mStorage;
		maxEnergy -= building.eStorage;

		//Removes the building from the buildings list
		buildings.Remove(building);
	}

	//Spawns a player unit
	void SpawnUnit(Unit unit, Vector2 position)
	{
		unit.currentHealth = unit.health;
		unit.location = position;
		unit.buildCounter = 0;
		unit.rec = new Rectangle((int)(unit.location.X - unit.hitbox.X / 2), (int)(unit.location.Y - unit.hitbox.Y / 2), (int)unit.hitbox.X, (int)unit.hitbox.Y);
		unit.destination = WorldToGrid(unit.location);
		unit.id = nextUnitID;
		nextUnitID++;

		//Increases player army value
		playerArmyValue += unit.armyValue;

		//Adds the unit to the player's units list
		units.Add(unit);
	}

	//Spawns an enemy unit
	void SpawnEnemyUnit(Unit unit, Vector2 position)
	{
		unit.currentHealth = unit.health;
		unit.location = position;
		unit.rec = new Rectangle((int)(unit.location.X - unit.hitbox.X / 2), (int)(unit.location.Y - unit.hitbox.Y / 2), (int)unit.hitbox.X, (int)unit.hitbox.Y);
		unit.destination = WorldToGrid(unit.location);
		unit.id = nextUnitID;
		unit.isActive = true;
		unit.buildCounter = 0;
		unit.tier = -1;
		nextUnitID++;

		//Increases enemy army value
		enemyArmyValue += unit.armyValue;

		//Adds the unit to the enemy's units list
		enemyUnits.Add(unit);
	}

	//Removes player units if their health is zero or lower
	void RemoveUnits()
	{
		//Iterates through every unit
		for (int i = units.Count - 1; i >= 0; i--)
		{
			//Checks if the unit is dead
			if (units[i].currentHealth <= 0)
			{
				//Stores the ID of the dead unit
				int dyingUnitId = units[i].id;

				//Updates player army value
				playerArmyValue -= units[i].armyValue;

				//Spawns an explosion at the unit's location
				SpawnExplosion(units[i].location, (int)units[i].hitbox.X);

				//Removes the unit from the unit list
				units.RemoveAt(i);

				//Removes the unit from the selected units list if it is there
				for (int s = selectedUnits.Count - 1; s >= 0; s--)
				{
					if (selectedUnits[s].id == dyingUnitId)
					{
						selectedUnits.RemoveAt(s);
						break;
					}
				}
			}
		}
	}

	//Removes enemy units if their health is zero or below
	void RemoveEnemyUnits()
	{
		//Iterates through every unit
		for (int i = enemyUnits.Count - 1; i >= 0; i--)
		{
			//Checks if the unit is dead
			if (enemyUnits[i].currentHealth <= 0)
			{
				//Spawns an explosion at the unit's location
				SpawnExplosion(enemyUnits[i].location, (int)enemyUnits[i].hitbox.X * 2);

				//Updates enemy army value
				enemyArmyValue -= enemyUnits[i].armyValue;

				//Removes the unit from the enemy units list
				enemyUnits.RemoveAt(i);
			}
		}
	}

	//Does collisions between player units
	void UnitCollisions()
	{
		//Calculates grid dimensions
		int width = MAP_WIDTH / GRID_WIDTH;
		int height = MAP_HEIGHT / GRID_HEIGHT;

		//Checks each pair of units
		for (int i = 0; i < units.Count; i++)
		{
			for (int j = i + 1; j < units.Count; j++)
			{
				//Only does collisions for active units
				if (units[i].isActive && units[j].isActive)
				{
					Rectangle a = units[i].rec;
					Rectangle b = units[j].rec;

					//Checks if the two units overlap
					if (a.Intersects(b))
					{
						//Calculates overlap amount
						float overlapX = Math.Min(a.Right - b.Left, b.Right - a.Left);
						float overlapY = Math.Min(a.Bottom - b.Top, b.Bottom - a.Top);
						Vector2 push;

						//Does the collision calculations
						if (overlapX < overlapY)
						{
							if (a.Center.X < b.Center.X)
							{
								push = new Vector2(-overlapX / 2f, 0);
							}
							else
							{
								push = new Vector2(overlapX / 2f, 0);
							}
						}
						else
						{
							if (a.Center.Y < b.Center.Y)
							{
								push = new Vector2(0, -overlapY / 2f);
							}
							else
							{
								push = new Vector2(0, overlapY / 2f);
							}
						}

						//Scales the push force to change the force of the collision movement
						push *= 0.02f;

						Unit ui = units[i];
						Unit uj = units[j];

						//Calculates new positions
						Vector2 newLocationI = ui.location + push;
						Vector2 newLocationJ = uj.location - push;

						//Converts new positions to grid coordinates
						Point gridI = WorldToGrid(newLocationI);
						Point gridJ = WorldToGrid(newLocationJ);

						//Checks if both new positions are valid
						bool iIsSafe = gridI.X >= 0 && gridI.X < width && gridI.Y >= 0 && gridI.Y < height && !blockedGrid[gridI.X, gridI.Y] && !buildingGrid[gridI.X, gridI.Y];
						bool jIsSafe = gridJ.X >= 0 && gridJ.X < width && gridJ.Y >= 0 && gridJ.Y < height && !blockedGrid[gridJ.X, gridJ.Y] && !buildingGrid[gridJ.X, gridJ.Y];

						//Applies movement only if both units can move
						if (iIsSafe && jIsSafe)
						{
							ui.location = newLocationI;
							uj.location = newLocationJ;
						}

						//Updates collision rectangles
						ui.rec = new Rectangle((int)(ui.location.X - ui.hitbox.X / 2), (int)(ui.location.Y - ui.hitbox.Y / 2), (int)ui.hitbox.X, (int)ui.hitbox.Y);
						uj.rec = new Rectangle((int)(uj.location.X - uj.hitbox.X / 2), (int)(uj.location.Y - uj.hitbox.Y / 2), (int)uj.hitbox.X, (int)uj.hitbox.Y);

						//Writes updated units back to the units list
						units[i] = ui;
						units[j] = uj;
					}
				}
			}
		}
	}

	//Finds a path from a start point to an end point using A*
	Queue<Point> FindPath(Point start, Point end)
	{
		//Calculates grid dimensions
		int width = MAP_WIDTH / GRID_WIDTH;
		int height = MAP_HEIGHT / GRID_HEIGHT;

		//Tracks how many nodes have been checked
		int nodesChecked = 0;

		//Returns an empty path if the destination is out of bounds
		if (end.X < 0 || end.X >= width || end.Y < 0 || end.Y >= height)
		{
			return new Queue<Point>();
		}
		if (blockedGrid[end.X, end.Y] || buildingGrid[end.X, end.Y])
		{
			return new Queue<Point>();
		}

		//Tracks used nodes
		bool[,] closed = new bool[width, height];

		//Tracks movement cost from start to each node
		int[,] gScore = new int[width, height];

		//Initializes all costs to maximum
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				gScore[x, y] = int.MaxValue;
			}
		}

		//Stores previous points
		Point[,] cameFrom = new Point[width, height];

		//Initializes the starting node
		gScore[start.X, start.Y] = 0;

		//Priority queue ordered by estimated cost
		PriorityQueue<Point, int> open = new PriorityQueue<Point, int>();
		open.Enqueue(start, 0);

		//Defines all possible movement directions
		Point[] directions = { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0), new Point(1, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1) };
		bool found = false;

		//Main pathfinding loop
		while (open.Count > 0)
		{
			nodesChecked++;

			//Stops pathfinding if too many nodes are checked and no path is found
			if (nodesChecked > MAX_NODES)
			{
				return new Queue<Point>();
			}

			//Gets the next best node
			Point current = open.Dequeue();

			//Stops if the path is found
			if (current == end)
			{
				found = true;
				break;
			}

			//Marks the node as visited
			closed[current.X, current.Y] = true;

			//Evaluates all adjacent nodes
			for (int d = 0; d < directions.Length; d++)
			{
				Point dir = directions[d];
				Point neighbor = new Point(current.X + dir.X, current.Y + dir.Y);

				//Skips nodes that are out of bounds
				if (neighbor.X < 0 || neighbor.X >= width || neighbor.Y < 0 || neighbor.Y >= height)
				{
					continue;
				}

				//Skips blocked nodes
				if (blockedGrid[neighbor.X, neighbor.Y] || buildingGrid[neighbor.X, neighbor.Y])
				{
					continue;
				}

				//Skips already checked nodes
				if (closed[neighbor.X, neighbor.Y])
				{
					continue;
				}

				//Finds movement cost
				int cost;
				if (dir.X != 0 && dir.Y != 0)
				{
					cost = 2;
				}
				else
				{
					cost = 1;
				}

				int testG = gScore[current.X, current.Y] + cost;

				//Ignores paths that are not better
				if (testG >= gScore[neighbor.X, neighbor.Y])
				{
					continue;
				}

				//Sets the best path to have this node
				if (testG < gScore[neighbor.X, neighbor.Y])
				{
					gScore[neighbor.X, neighbor.Y] = testG;
					cameFrom[neighbor.X, neighbor.Y] = current;
					int f = testG + Math.Abs(neighbor.X - end.X) + Math.Abs(neighbor.Y - end.Y);
					open.Enqueue(neighbor, f);
				}
			}
		}

		//Returns an empty path if no path was found
		if (start == end)
		{
			return new Queue<Point>();
		}
		if (!found)
		{
			return new Queue<Point>();
		}

		//Queues the nodes in the path backwards
		Queue<Point> path = new Queue<Point>();
		Point temp = end;

		while (temp != start)
		{
			path.Enqueue(temp);
			temp = cameFrom[temp.X, temp.Y];
		}

		//Reverses the path so it goes forwards from start to end
		return new Queue<Point>(path.Reverse());
	}

	//Draws the unit selection box
	void DrawSelectionRect(Rectangle rec, int thickness, Color color)
	{
		//Top edge
		_spriteBatch.Draw(pixel, new Rectangle(rec.X, rec.Y, rec.Width, thickness), color);

		//Left edge
		_spriteBatch.Draw(pixel, new Rectangle(rec.X, rec.Y, thickness, rec.Height), color);

		//Bottom edge
		_spriteBatch.Draw(pixel, new Rectangle(rec.X, rec.Bottom - thickness, rec.Width, thickness), color);

		//Right edge
		_spriteBatch.Draw(pixel, new Rectangle(rec.Right - thickness, rec.Y, thickness, rec.Height), color);
	}

	//Calculates units' spread when given a move command
	void MakeUnitSpread(Vector2 worldStart, Vector2 worldEnd)
	{
		//makes sure that both the start and end positions are in the map
		if (!(worldStart.X < 0 || worldStart.X > MAP_WIDTH || worldStart.Y < 0 || worldStart.Y > MAP_WIDTH || worldEnd.X < 0 || worldEnd.X > MAP_WIDTH || worldEnd.Y < 0 || worldEnd.Y > MAP_WIDTH))
		{
			//Does nothing if no units are selected
			if (selectedUnits.Count == 0)
			{
				return;
			}

			//Calculates the evenly spaced destination points for the units
			List<Point> spread = ComputeSpreadWaypoints(worldStart, worldEnd, selectedUnits.Count);

			//Gives each selected unit a destination from the computed spread points
			for (int i = 0; i < selectedUnits.Count; i++)
			{
				Unit u = selectedUnits[i];

				//Destination goes back to the unit's current position if the target is invalid
				if (spread[i].X <= 0 && spread[i].X > (MAP_WIDTH / GRID_WIDTH) && spread[i].Y <= 0 && spread[i].Y > (MAP_HEIGHT / GRID_HEIGHT))
				{
					spread[i] = WorldToGrid(u.location);
				}

				//Calculates a path from the unit's current location to its waypoint
				u.path = FindPath(WorldToGrid(u.location), spread[i]);

				//Cancels movement if the destination is blocked
				if (blockedGrid[spread[i].X, spread[i].Y] || buildingGrid[spread[i].X, spread[i].Y])
				{
					u.destination = WorldToGrid(u.location);
				}
				else
				{
					u.destination = spread[i];
				}

				//Updates the selected units list
				selectedUnits[i] = u;

				//Updates the units list with the changes
				for (int j = 0; j < units.Count; j++)
				{
					if (selectedUnits[i].id == units[j].id)
					{
						units[j] = selectedUnits[i];
						break;
					}
				}
			}
		}
	}

	//Calculates evenly spaced waypoints between two world positions
	List<Point> ComputeSpreadWaypoints(Vector2 start, Vector2 end, int count)
	{
		List<Point> waypoints = new List<Point>();

		//Generates positions between start and end
		for (int i = 0; i < count; i++)
		{
			//Calculates the spacing
			float t = (i + 1) / (float)(count + 1);

			//Converts the calculated waypoints' world position to grid coordinates
			waypoints.Add(WorldToGrid(Vector2.Lerp(start, end, t)));
		}

		return waypoints;
	}

	//Draws a straight line between two points for drawing unit paths
	void DrawLine(Vector2 start, Vector2 end, Color color)
	{
		_spriteBatch.Draw(pixel, start, null, color, (float)Math.Atan2(end.Y - start.Y, end.X - start.X), new Vector2(0, 0.5f), new Vector2((end - start).Length(), 2f), SpriteEffects.None, 0f);
	}

	//Draws a progress bar above a unit or building
	void DrawProgressBar(Vector2 location, Vector2 hitbox, int buildCounter, int buildTime, Color color)
	{
		//Scales the bar width based on build progress
		_spriteBatch.Draw(pixel, new Rectangle((int)location.X, (int)location.Y - 15, (int)(hitbox.X * GRID_WIDTH * buildCounter / buildTime), 12), color);
	}

	//Handles all resource production by buildings
	void ProduceRes()
	{
		//Adds base resource generation
		metal += 3;
		energy += 30;

		//Iterates through all buildings to add their resource production
		for (int i = 0; i < buildings.Count; i++)
		{
			//Only active buildings produce resources
			if (buildings[i].isActive)
			{
				switch (buildings[i].texture)
				{
					//Advanced energy converter turns energy into metal
					case "AConverter":
						metal += buildings[i].mProduction;
						energy += buildings[i].eProduction;
						break;

					//Advanced fusion reactor produces energy
					case "AFUS":
						energy += buildings[i].eProduction;
						break;

					//Advanced metal extractor produces metal when on a metal spot
					case "AMex":
						bool onMetalSpot = false;

						for (int j = 0; j < mSpots.Length; j++)
						{
							if (buildings[i].rec.Contains(mSpots[j]))
							{
								onMetalSpot = true;
								break;
							}
						}

						if (onMetalSpot)
						{
							metal += buildings[i].mProduction;
						}

						break;

					//Energy converter turns energy into metal
					case "Converter":

						metal += buildings[i].mProduction;
						energy += buildings[i].eProduction;

						break;

					//Metal extractor produces metal when on a metal spot
					case "Mex":

						onMetalSpot = false;

						for (int j = 0; j < mSpots.Length; j++)
						{
							if (buildings[i].rec.Contains(mSpots[j]))
							{
								onMetalSpot = true;
								break;
							}
						}

						if (onMetalSpot)
						{
							metal += buildings[i].mProduction;
						}

						break;

					//Solar panel produces energy
					case "Solar":
						energy += buildings[i].eProduction;
						break;

					//Wind turbine produces energy
					case "Turbine":
						energy += buildings[i].eProduction;
						break;
				}
			}
		}
	}

	//Calculates the enemy units whose army value will be as close as possible to the target
	List<Unit> BuildEnemyWave(List<Unit> types, int target)
	{
		//Allows overshooting the target to find a better match
		int max = target + 100;

		//Tells whether the target army value can be formed
		bool[] possible = new bool[max + 1];

		//Backtracking array that stores the previous working sum
		int[] prev = new int[max + 1];

		//Stores the unit indexes that were used to reach the sum
		int[] used = new int[max + 1];

		//Zero army value is always possible
		possible[0] = true;

		//Initializes array for backtracking
		for (int i = 1; i <= max; i++)
		{
			prev[i] = -1;
		}

		//Finds all possible army value sums
		for (int s = 0; s <= max; s++)
		{
			if (!possible[s])
			{
				continue;
			}

			for (int i = 0; i < types.Count; i++)
			{
				int v = types[i].armyValue;

				//Marks new sums as possible
				if (s + v <= max && !possible[s + v])
				{
					possible[s + v] = true;
					prev[s + v] = s;
					used[s + v] = i;
				}
			}
		}

		//Finds the posible sum closest to the target
		int best = 0;
		int bestDiff = int.MaxValue;

		for (int s = 0; s <= max; s++)
		{
			if (!possible[s])
			{
				continue;
			}
			int diff = Math.Abs(target - s);
			if (diff < bestDiff)
			{
				bestDiff = diff;
				best = s;
			}
		}

		//Adds the chosen units by backtracking from the solution
		List<Unit> result = new();
		while (best > 0)
		{
			int i = used[best];
			result.Add(types[i]);
			best = prev[best];
		}

		return result;
	}

	//Updates attacks for all units
	void UpdateUnitAttacks()
	{
		//Player unit attacks
		for (int i = 0; i < units.Count; i++)
		{
			Unit u = units[i];

			//Increments unit's attack cooldown counter
			u.atkCooldown++;

			//Tries to do an attack when the cooldown is up
			if (u.atkCooldown >= u.atkSpd && u.isActive)
			{
				if (TryAttack(u, enemyUnits, enemyBuildings))
				{
					u.atkCooldown = 0;
				}
			}

			units[i] = u;
		}

		//Enemy unit attacks
		for (int i = 0; i < enemyUnits.Count; i++)
		{
			Unit u = enemyUnits[i];

			//Increments unit's attack cooldown counter
			u.atkCooldown++;

			//Tries to do an attack when the cooldown is up
			if (u.atkCooldown >= u.atkSpd && u.isActive)
			{
				if (TryAttack(u, units, buildings))
				{
					u.atkCooldown = 0;
				}
			}

			enemyUnits[i] = u;
		}
	}

	//Updates attacks for all buildings
	void UpdateBuildingAttacks()
	{
		//Player buildings attack enemy units and buildings
		for (int i = 0; i < buildings.Count; i++)
		{
			Building b = buildings[i];

			//Increments building's attack cooldown counter
			b.atkCooldown++;

			//Tries to do an attack when the cooldown is up
			if (b.atkCooldown >= b.atkSpd && b.isActive)
			{
				if (TryBuildingAttack(b, enemyUnits, enemyBuildings))
				{
					b.atkCooldown = 0;
				}
			}

			buildings[i] = b;
		}

		//Enemy buildings attack player units and buildings
		for (int i = 0; i < enemyBuildings.Count; i++)
		{
			Building b = enemyBuildings[i];

			//Increments building's attack cooldown counter
			b.atkCooldown++;

			//Tries to do an attack when the cooldown is up
			if (b.atkCooldown >= b.atkSpd && b.isActive)
			{
				if (TryBuildingAttack(b, units, buildings))
				{
					b.atkCooldown = 0;
				}
			}

			enemyBuildings[i] = b;
		}
	}

	//Tries an attack from a unit to enemies in range
	bool TryAttack(Unit attacker, List<Unit> enemyUnits, List<Building> enemyBuildings)
	{
		//Calculates all targets in range
		List<(bool isUnit, int index)> validTargets = new();

		//Calculates all enemy units in range
		for (int i = 0; i < enemyUnits.Count; i++)
		{
			if (!enemyUnits[i].isActive)
			{
				continue;
			}

			float d = Vector2.Distance(attacker.location, enemyUnits[i].location);
			if (d <= attacker.range * 1.5)
			{
				validTargets.Add((true, i));
			}
		}

		//Calculates all enemy buildings in range
		for (int i = 0; i < enemyBuildings.Count; i++)
		{
			if (!enemyBuildings[i].isActive)
			{
				continue;
			}

			float d = Vector2.Distance(attacker.location, enemyBuildings[i].location);
			if (d <= attacker.range * 1.5)
			{
				validTargets.Add((false, i));
			}
		}

		//Does nothing if there is nothing to attack
		if (validTargets.Count == 0)
		{
			return false;
		}

		//Picks a random target
		var target = validTargets[rng.Next(validTargets.Count)];

		//Attacks a unit
		if (target.isUnit)
		{
			Unit u = enemyUnits[target.index];
			u.currentHealth -= attacker.damage;
			enemyUnits[target.index] = u;

			//Spawns a projectile if it can
			if (attacker.projectile != null)
			{
				if (u.tier >= 0)
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(u.location.X + u.hitbox.X / 2, u.location.Y + u.hitbox.Y / 2), attacker.damage, projectileTypes[attacker.projectile].range, false);

				}
				else
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(u.location.X + u.hitbox.X / 2, u.location.Y + u.hitbox.Y / 2), attacker.damage, projectileTypes[attacker.projectile].range, true);
				}

				//Plays shooting sound
				if (soundsThisSecond < maxSoundsPerSecond)
				{
					shootingSFX.Play();
					soundsThisSecond++;
				}
			}
		}

		//Attacks a building
		else
		{
			Building b = enemyBuildings[target.index];
			b.currentHealth -= attacker.damage;
			enemyBuildings[target.index] = b;

			//Spawns a projectile if it can
			if (attacker.projectile != null)
			{
				if (b.tier >= 0)
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(b.location.X + b.hitbox.X * GRID_WIDTH / 2, b.location.Y + b.hitbox.Y * GRID_HEIGHT / 2), attacker.damage, projectileTypes[attacker.projectile].range, false);
				}
				else
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(b.location.X + b.hitbox.X * GRID_WIDTH / 2, b.location.Y + b.hitbox.Y * GRID_HEIGHT / 2), attacker.damage, projectileTypes[attacker.projectile].range, true);
				}

				//Plays shooting sound
				if (soundsThisSecond < maxSoundsPerSecond)
				{
					shootingSFX.Play();
					soundsThisSecond++;
				}
			}
		}

		return true;
	}

	//Tries an attack from a building to enemies in range
	bool TryBuildingAttack(Building attacker, List<Unit> enemyUnits, List<Building> enemyBuildings)
	{
		//Calculates all valid targets in range
		List<(bool isUnit, int index)> validTargets = new();

		//Calculates all enemy units in range
		for (int i = 0; i < enemyUnits.Count; i++)
		{
			if (!enemyUnits[i].isActive)
			{
				continue;
			}

			float d = Vector2.Distance(attacker.location, enemyUnits[i].location);
			if (d <= attacker.range)
			{
				validTargets.Add((true, i));
			}
		}

		//Calculates all enemy buildings in range
		for (int i = 0; i < enemyBuildings.Count; i++)
		{
			if (!enemyBuildings[i].isActive)
			{
				continue;
			}

			float d = Vector2.Distance(attacker.location, enemyBuildings[i].location);
			if (d <= attacker.range)
			{
				validTargets.Add((false, i));
			}
		}

		//Does nothing if there are no targets
		if (validTargets.Count == 0)
		{
			return false;
		}

		//Randomly selects a target
		var target = validTargets[rng.Next(validTargets.Count)];

		//Attacks a unit
		if (target.isUnit)
		{
			Unit u = enemyUnits[target.index];
			u.currentHealth -= attacker.damage;
			enemyUnits[target.index] = u;

			if (attacker.projectile != null)
			{
				if (u.tier >= 0)
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(u.location.X + u.hitbox.X / 2, u.location.Y + u.hitbox.Y / 2), attacker.damage, projectileTypes[attacker.projectile].range, false);
				}
				else
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(u.location.X + u.hitbox.X / 2, u.location.Y + u.hitbox.Y / 2), attacker.damage, projectileTypes[attacker.projectile].range, true);
				}

				if (soundsThisSecond < maxSoundsPerSecond)
				{
					shootingSFX.Play();
					soundsThisSecond++;
				}
			}
		}
		//Attacks a building
		else
		{
			Building b = enemyBuildings[target.index];
			b.currentHealth -= attacker.damage;
			enemyBuildings[target.index] = b;

			if (attacker.projectile != null)
			{
				if (b.tier >= 0)
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(b.location.X + b.hitbox.X * GRID_WIDTH / 2, b.location.Y + b.hitbox.Y * GRID_HEIGHT / 2), attacker.damage, projectileTypes[attacker.projectile].range, false);
				}
				else
				{
					SpawnProjectile(projectileTypes[attacker.projectile], attacker.location, new Vector2(b.location.X + b.hitbox.X * GRID_WIDTH / 2, b.location.Y + b.hitbox.Y * GRID_HEIGHT / 2), attacker.damage, projectileTypes[attacker.projectile].range, true);
				}

				if (soundsThisSecond < maxSoundsPerSecond)
				{
					shootingSFX.Play();
					soundsThisSecond++;
				}
			}
		}

		return true;
	}

	//Spawns a projectile that travels from an attacker to a destination
	private void SpawnProjectile(Projectile projectile, Vector2 attackerPos, Vector2 destination, int damage, int range, bool flip)
	{
		//Sets projectile start position
		projectile.location = attackerPos;

		//Sets projectile destination
		projectile.destination = destination;

		//Sets projectile lifetime timer
		projectile.timer = 0;

		//Sets damage and explosion range
		projectile.damage = damage;
		projectile.range = range;

		//Calculates projectile rotation based on direction
		projectile.rotation = (float)Math.Atan2(destination.Y - attackerPos.Y, destination.X - attackerPos.X);

		//Adds projectile to the projectile list
		projectiles.Add(projectile);
	}

	//Updates all projectiles
	private void UpdateProjectiles()
	{
		for (int i = 0; i < projectiles.Count; i++)
		{
			Projectile p = projectiles[i];

			//Moves projectile if it is alive
			if (p.timer < p.lifespan)
			{
				//Calculates move amount
				float t = (float)p.timer / p.lifespan;
				t = Math.Clamp(t, 0f, 1f);

				//Calculates new position
				Vector2 currentPos = p.location + (p.destination - p.location) * t;

				//Updates the projectile's rectangle
				p.rec = new Rectangle((int)currentPos.X, (int)currentPos.Y, p.rec.Width, p.rec.Height);

				//Increments the projectile's timer
				p.timer++;
				projectiles[i] = p;
			}
			//Projectile has reached its destination
			else
			{
				//Applies explosion damage
				ApplyProjectileDamage(p);

				//Spawns explosion animation
				SpawnExplosion(projectiles[i].destination, projectiles[i].range);

				//Removes the projectile
				projectiles.RemoveAt(i);
				i--;
			}
		}
	}

	//Applies AOE damage from the projectile's impact
	private void ApplyProjectileDamage(Projectile p)
	{
		//Damages player's units in range
		for (int i = 0; i < units.Count; i++)
		{
			if (!units[i].isActive)
			{
				continue;
			}

			float d = Vector2.Distance(p.destination, units[i].location);
			if (d <= p.range)
			{
				Unit u = units[i];
				u.currentHealth -= p.damage;
				units[i] = u;
			}
		}

		//Damages enemy's units in range
		for (int i = 0; i < enemyUnits.Count; i++)
		{
			if (!enemyUnits[i].isActive)
				continue;

			float d = Vector2.Distance(p.destination, enemyUnits[i].location);
			if (d <= p.range)
			{
				Unit u = enemyUnits[i];
				u.currentHealth -= p.damage;
				enemyUnits[i] = u;
			}
		}
	}

	//Spawns an explosion animation and playes explosion sound
	private void SpawnExplosion(Vector2 pos, int size)
	{
		//Randomly selects explosion type
		int random = rng.Next(0, 4);

		Animation anim = new Animation(explosion1Img, 5, 5, 23, 0, Animation.NO_IDLE, 1, 750, pos, size / 20f, size / 20f, true, "Explosion1");

		switch (random)
		{
			case 0:
				//Explosion type 1
				anim = new Animation(explosion1Img, 5, 5, 23, 0, Animation.NO_IDLE, 1, 750, new Vector2(pos.X - anim.GetDestRec().Width / 2, pos.Y - anim.GetDestRec().Height / 2), size / 15f, size / 15f, true, "Explosion1");
				if (soundsThisSecond < maxSoundsPerSecond)
				{
					explosion1SFX.Play();
					soundsThisSecond++;
				}
				break;
			case 1:
				//Explosion type 2
				anim = new Animation(explosion2Img, 8, 4, 29, 0, Animation.NO_IDLE, 1, 750, new Vector2(pos.X - anim.GetDestRec().Width / 2, pos.Y - anim.GetDestRec().Height / 2), size / 45f, size / 45f, true, "Explosion2");
				if (soundsThisSecond < maxSoundsPerSecond)
				{
					explosion2SFX.Play();
					soundsThisSecond++;
				}
				break;
			case 2:
				//Explosion type 3
				if (soundsThisSecond < maxSoundsPerSecond)
				{
					explosion3SFX.Play();
					soundsThisSecond++;
				}
				anim = new Animation(explosion3Img, 8, 6, 48, 0, Animation.NO_IDLE, 1, 750, new Vector2(pos.X - anim.GetDestRec().Width / 2, pos.Y - anim.GetDestRec().Height / 2), size / 25f, size / 25f, true, "Explosion3");
				break;
			case 3:
				//Explosion type 4
				if (soundsThisSecond < maxSoundsPerSecond)
				{
					explosion4SFX.Play();
					soundsThisSecond++;
				}
				anim = new Animation(explosion4Img, 5, 5, 25, 0, Animation.NO_IDLE, 1, 750, new Vector2(pos.X - anim.GetDestRec().Width / 2, pos.Y - anim.GetDestRec().Height / 2), size / 30f, size / 30f, true, "Explosion4");
				break;
		}

		//Assigns animation to first available explosion animation slot
		for (int i = 0; i < explosions.Length; i++)
		{
			if (explosions[i].IsFinished() || !explosions[i].IsAnimating())
			{
				explosions[i] = anim;
			}
		}
	}

	//Updates fog of war
	void UpdateFog()
	{
		if (fogOff)
		{
			return;
		}

		//Converts all visible tiles to explored tiles
		for (int x = 0; x < 200; x++)
		{
			for (int y = 0; y < 200; y++)
			{
				if (fog[x, y] == 2)
				{
					fog[x, y] = 1;
				}
			}
		}

		//Reveals fog around friendly units
		for (int i = 0; i < units.Count; i++)
		{
			if (!units[i].isActive)
			{
				continue;
			}
			Reveal(units[i].location, units[i].sightRange);
		}

		//Reveals fog around friendly buildings
		for (int i = 0; i < buildings.Count; i++)
		{
			if (!buildings[i].isActive)
			{
				continue;
			}
			Reveal(new Vector2(buildings[i].location.X + buildings[i].hitbox.X * GRID_WIDTH / 2, buildings[i].location.Y + buildings[i].hitbox.Y * GRID_HEIGHT / 2), buildings[i].sightRange);
		}
	}

	//Reveals fog tiles within a circle sight radius
	void Reveal(Vector2 worldPos, float sightRange)
	{
		//Converts world position to grid coordinates
		int cx = (int)(worldPos.X / GRID_WIDTH);
		int cy = (int)(worldPos.Y / GRID_HEIGHT);

		//Calculates radius of sight lines in grid units
		int radius = (int)(sightRange / GRID_WIDTH);

		for (int x = cx - radius; x <= cx + radius; x++)
		{
			for (int y = cy - radius; y <= cy + radius; y++)
			{
				if (x < 0 || y < 0 || x >= 200 || y >= 200)
				{
					continue;
				}

				float dx = x - cx;
				float dy = y - cy;

				//Reveals tile if within sight range
				if (dx * dx + dy * dy <= radius * radius)
				{
					fog[x, y] = 2;
				}
			}
		}
	}

	//Checks if the player won or lost
	private void ProcessWins()
	{
		//Checks if player commander is alive
		if (units.Count != 0)
		{
			if (!units[0].texture.Equals("Commander"))
			{
				gameState = "ENDGAME";
				win = false;
			}
		}
		else
		{
			gameState = "ENDGAME";
			win = false;
			return;
		}

		//Checks if enemy commander is alive
		if (enemyUnits.Count != 0)
		{
			if (!enemyUnits[0].texture.Equals("Commander"))
			{
				gameState = "ENDGAME";
				win = true;
			}
		}
		else
		{
			gameState = "ENDGAME";
			win = true;
			return;
		}
	}
}
