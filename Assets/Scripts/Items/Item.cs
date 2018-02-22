using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum ItemType {
  glove,
  boot
}

public class Item : MonoBehaviour {

  public Sprite glove;
  public Sprite boot;

  public ItemType type;
  public Color level;
  public int attackMod, defenseMod;

  private SpriteRenderer spriteRenderer;

  public void RandomItemInit() {
    spriteRenderer = GetComponent<SpriteRenderer>();
    SelectItem();
  }

  private void SelectItem() {
    var itemCount = Enum.GetValues(typeof(ItemType)).Length;
    type = (ItemType)Random.Range(0, itemCount);

    switch (type) {
      case ItemType.glove:
        attackMod = Random.Range(1, 4);
        defenseMod = 0;
        spriteRenderer.sprite = glove;
        break;
      case ItemType.boot:
        attackMod = 0;
        defenseMod = Random.Range(1, 4);
        spriteRenderer.sprite = boot;
        break;
    }

    int randomLevel = Random.Range(0, 100);
    if(randomLevel >= 0 && randomLevel < 50) {
      spriteRenderer.color = level = Color.blue;
      attackMod += Random.Range(1, 4);
      defenseMod += Random.Range(1, 4);
    }
    else if(randomLevel >= 50 && randomLevel < 75) {
      spriteRenderer.color = level = Color.green;
      attackMod += Random.Range(4, 10);
      defenseMod += Random.Range(4, 10);
    }
    else if (randomLevel >= 75 && randomLevel < 90) {
      spriteRenderer.color = level = Color.yellow;
      attackMod += Random.Range(15, 25);
      defenseMod += Random.Range(15, 25);
    }
    else {
      spriteRenderer.color = level = Color.magenta;
      attackMod += Random.Range(40, 55);
      defenseMod += Random.Range(40, 55);      
    }
  }

  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
