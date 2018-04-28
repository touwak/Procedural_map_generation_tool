using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour
{
	public float moveTime = 0.1f;			//Time it will take object to move, in seconds.
	public LayerMask blockingLayer;			//Layer on which collision will be checked.
	
	
	private BoxCollider2D boxCollider; 		//The BoxCollider2D component attached to this object.
	private Rigidbody2D rb2D;				//The Rigidbody2D component attached to this object.
	private float inverseMoveTime;			//Used to make movement more efficient.
	
	
	protected virtual void Start ()
	{
		boxCollider = GetComponent <BoxCollider2D> ();
		rb2D = GetComponent <Rigidbody2D> ();
		
		//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
		inverseMoveTime = 1f / moveTime;
	}

  /// <summary>
  /// Move this object
  /// </summary>
  /// <param name="xDir"> direction to move in X axis</param>
  /// <param name="yDir"> direction to move in Y axis </param>
  /// <param name="hit"> raycast2D to detect collisions </param>
  /// <returns></returns>
  protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2 (xDir, yDir);
		
		//Disable the boxCollider so that linecast doesn't hit this object's own collider.
		boxCollider.enabled = false;
		hit = Physics2D.Linecast (start, end, blockingLayer);
		
		boxCollider.enabled = true;
		
		if(hit.transform == null)
		{
			StartCoroutine (SmoothMovement (end));
			
			return true;
		}
		
		return false;
	}
	
	/// <summary>
  /// Move the object smoothly
  /// </summary>
  /// <param name="end"> destiny to move </param>
  /// <returns></returns>
	protected IEnumerator SmoothMovement (Vector3 end)
	{
		//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
		//Square magnitude is used instead of magnitude because it's computationally cheaper.
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		
		while(sqrRemainingDistance > float.Epsilon)
		{
			//Find a new position proportionally closer to the end, based on the moveTime
			Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition (newPostion);

			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			yield return null;
		}
	}

  /// <summary>
  /// Detect if the object can move or not
  /// </summary>
  /// <typeparam name="T"> Type of object </typeparam>
  /// <param name="xDir"> position in the X axis to check </param>
  /// <param name="yDir"> position in the Y axis to check </param>
  /// <returns></returns>
  protected virtual bool AttemptMove <T> (int xDir, int yDir)
		where T : Component
	{
		//Hit will store whatever our linecast hits when Move is called.
		RaycastHit2D hit;
		
		//Set canMove to true if Move was successful, false if failed.
		bool canMove = Move (xDir, yDir, out hit);

    //Check if nothing was hit by linecast
    if (hit.transform == null) {
      return true;
    }
		//Get a component reference to the component of type T attached to the object that was hit
		T hitComponent = hit.transform.GetComponent <T> ();
		
		//If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
		if(!canMove && hitComponent != null)
			
			//Call the OnCantMove function and pass it hitComponent as a parameter.
			OnCantMove (hitComponent);

		return false;
	}
	
	protected abstract void OnCantMove <T> (T component)
		where T : Component;
}
