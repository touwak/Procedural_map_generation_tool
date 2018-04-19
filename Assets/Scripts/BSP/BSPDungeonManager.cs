using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BSPDungeonManager : MonoBehaviour {

  public int minSize = 50, maxSize = 100;
  public Dictionary<Vector2, TileType> gridPositions = 
    new Dictionary<Vector2, TileType>();

  [HideInInspector]
  public int width, height;

  private void Awake() {
    width = Random.Range(minSize, maxSize);
    height = Random.Range(minSize, maxSize);
  }

  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  public void StartDungeon() {

    gridPositions.Clear();
    width = Random.Range(minSize, maxSize);
    height = Random.Range(minSize, maxSize);

    GenerateLevel();

    Debug.LogFormat("width: {0}, height: {1}", width, height);
  }

  //error when the list is modify
  private void GenerateLevel() {

    Leaf root = new Leaf(0, 0, width, height);
    root.Split();

    root.CreateRooms(gridPositions);
  }
}
