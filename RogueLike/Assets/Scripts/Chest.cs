using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour {

  public Sprite openSprite;
  public Item randomItem;

  private SpriteRenderer spriteRenderer;

  private void Awake() {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public void Open() {
    spriteRenderer.sprite = openSprite;

    randomItem.RandomItemInit();
    GameManager.instance.InstanceTile(new Vector2(transform.position.x, transform.position.y),
      randomItem.gameObject, transform.parent);

    gameObject.layer = 11;
    spriteRenderer.sortingLayerName = "Items";
  }
}
