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

    CreateLeafs();
  }

  //error when the list is modify
  private void CreateLeafs() {
    Leaf root = new Leaf(0, 0, height, height);
    leafs.Add(root);

    bool split = true;
    while (split) {

      //test it
      split = false;
      int count = 0;
      while(leafs.Count != 0) {
        Leaf leaf = leafs[count] as Leaf;
        if(leaf.leftChild == null && leaf.rightChild == null) {
          if(leaf.width > maxLeafSize || leaf.height > maxLeafSize 
            || Random.value > 0.25f) {

            if (leaf.Split()) {
              leafs.Add(leaf.rightChild);
              leafs.Add(leaf.leftChild);

              split = true;
            }
          }
        }
      }
    }

    root.CreateRooms();
  }
}
