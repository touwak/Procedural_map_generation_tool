using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Chest : MonoBehaviour {

  public Sprite openSprite;
  public Item randomItem;
  public Weapon weapon;

  private SpriteRenderer spriteRenderer;

  private void Awake() {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  /// <summary>
  /// Randomlu instantiate a item 
  /// </summary>
  public void Open() {
    spriteRenderer.sprite = openSprite;

    GameObject toInstantiate;

    if(Random.Range(0, 2) == 1) {
      randomItem.RandomItemInit();
      toInstantiate = randomItem.gameObject;
    }
    else {
      toInstantiate = weapon.gameObject;
    }

    GameManager.instance.InstanceTile(new Vector3(transform.position.x, transform.position.y),
      toInstantiate, transform.parent);

    gameObject.layer = 11;
    spriteRenderer.sortingLayerName = "Items";
  }
}
