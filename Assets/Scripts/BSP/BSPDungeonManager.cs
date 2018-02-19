using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BSPDungeonManager : MonoBehaviour {

  public uint maxLeafSize = 20;
  private ArrayList leafs;
  public int width, height;
  public Dictionary<Vector2, TileType> gridPositions = 
    new Dictionary<Vector2, TileType>();
  

  private void Awake() {
    width = Random.Range(50, 101);
    height = Random.Range(50, 101);
    leafs = new ArrayList();
  }

  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  public void StartDungeon() {

    GenerateLevel();
  }

  //error when the list is modify
  private void GenerateLevel() {
    Leaf root = new Leaf(0, 0, height, height);
    root.Split();

    root.CreateRooms();
  }
}
