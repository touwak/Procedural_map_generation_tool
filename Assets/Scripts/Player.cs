using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MovingObject
{
	public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
	//public Text healthText;						//UI Text to display current player health total.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private int health;             //Used to store player health points total during level.
  private static Vector2 position;
  public bool onWorldBoard;
  public bool dungeonTransition;
  public static Vector2 lastPosition;
  public Image glove;
  public Image boot;
  public int attackMod = 0, defenseMod = 0;
  private Dictionary<string, Item> inventory;

  protected override void Start ()
	{
		animator = GetComponent<Animator>();
		health = GameManager.instance.healthPoints;
    //healthText.text = "Health: " + health;

    position.x = position.y = 2;
    onWorldBoard = true;
    dungeonTransition = false;

    inventory = new Dictionary<string, Item>();

    base.Start ();
	}
	
	private void Update () {

    if (!GameManager.instance.playersTurn) return;

    int horizontal = 0;
    int vertical = 0;

    bool canMove = false;

    horizontal = (int)(Input.GetAxisRaw("Horizontal"));
    vertical = (int)(Input.GetAxisRaw("Vertical"));

    if (horizontal != 0) {
      vertical = 0;
    }

    if (horizontal != 0 || vertical != 0) {
      if (!dungeonTransition) {
        if (onWorldBoard) {
          canMove = AttemptMove<Wall>(horizontal, vertical);
        }
        else {
          canMove = AttemptMove<Chest>(horizontal, vertical);
        }

        if (canMove && onWorldBoard) {
          lastPosition = position;
          position.x += horizontal;
          position.y += vertical;
          GameManager.instance.UpdateBoard(horizontal, vertical);
        }
      }
    }
  }

  /// <summary>
  /// Detect if the object can move or not
  /// </summary>
  /// <typeparam name="T"> Type of object </typeparam>
  /// <param name="xDir"> position in the X axis to check </param>
  /// <param name="yDir"> position in the Y axis to check </param>
  /// <returns></returns>
  protected override bool AttemptMove <T> (int xDir, int yDir) {	
		bool hit = base.AttemptMove <T> (xDir, yDir);
		GameManager.instance.playersTurn = false;

		return hit;
	}

  /// <summary>
  /// Define the behaviour of the player when collide with an object
  /// </summary>
  /// <typeparam name="T"> object collided type </typeparam>
  /// <param name="component"> object collided </param>
  protected override void OnCantMove <T> (T component) {
    if (typeof(T) == typeof(Wall)){
      Wall hitWall = component as Wall;
      hitWall.DamageWall(wallDamage);
    }
    else if(typeof(T) == typeof(Chest)) {
      Chest hitChest = component as Chest;
      hitChest.Open();
    }

		animator.SetTrigger ("playerChop");
	}
	
  /// <summary>
  /// update player's health 
  /// </summary>
  /// <param name="loss"> amount of health lost </param>
	public void LoseHealth (int loss) {
		animator.SetTrigger ("playerHit");
		health -= loss;
		//healthText.text = "-"+ loss + " Health: " + health;	
		
	}
	
  public Vector2 Position {
    get {
      return position;
    }
    set {
      if (value != position) {
        position = value;
        transform.position = position;
      }
    }
  }

  /// <summary>
  /// Switch between dungeon and endless world
  /// </summary>
  private void GoDungeonPortal() {
    if (onWorldBoard) {
      onWorldBoard = false;
      GameManager.instance.EnterDungeon();
      transform.position = DungeonManager.startPos;
    }
    else {
      onWorldBoard = true;
      GameManager.instance.ExitDungeon();
      transform.position = lastPosition;
      position = lastPosition;
    }
  }

  private void OnTriggerEnter2D(Collider2D other) {
    if(other.tag == "Exit") {
      dungeonTransition = true;
      Invoke("GoDungeonPortal", 0.5f);
    }
    else if(other.tag == "Item") {
      UpdateInvetory(other);
      Destroy(other.gameObject);
    }
  }

  /// <summary>
  /// Update the inventory
  /// </summary>
  /// <param name="item"> Item added to the inventory</param>
  private void UpdateInvetory(Collider2D item) {
    Item itemData = item.GetComponent<Item>();

    switch (itemData.type) {
      case ItemType.glove:
        if (!inventory.ContainsKey("glove")) {
          inventory.Add("glove", itemData);
        }
        else {
          inventory["glove"] = itemData;
        }

        glove.color = itemData.level;
        break;
      case ItemType.boot:
        if (!inventory.ContainsKey("boot")) {
          inventory.Add("boot", itemData);
        }
        else {
          inventory["boot"] = itemData;
        }

        boot.color = itemData.level;
        break;
    }

    attackMod = 0;
    defenseMod = 0;

    foreach(KeyValuePair<string, Item> gear in inventory) {
      attackMod += gear.Value.attackMod;
      defenseMod += gear.Value.defenseMod;
    }

  }
}

