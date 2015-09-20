using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 3.0f;
    public float strafeSpeed = 1.5f;
    public float turnSpeed = 360;
	// Use this for initialization
	void Start () {
	
	}
	

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 0, -turnSpeed * Input.GetAxis("RightH") * turnSpeed));
        transform.Translate(Vector3.up * Input.GetAxis("Vertical") * moveSpeed * Time.fixedDeltaTime);
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * strafeSpeed * Time.fixedDeltaTime);

       

    }

	// Update is called once per frame
	void Update () {
	
	}
}
