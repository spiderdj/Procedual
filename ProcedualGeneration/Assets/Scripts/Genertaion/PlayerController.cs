using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 3.0f;
    public float strafeSpeed = 1.5f;
    public float turnSpeed = 360;

    public float reloadTime = 0.2f;
    public float fireCooldown = 0.0f;
    public Object bulletPrefab;
    public Vector2 facing;
	// Use this for initialization
	void Start () {
	
	}
	

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 0, -turnSpeed * Input.GetAxis("RightH") * turnSpeed));
        transform.Translate(Vector3.up * Input.GetAxis("Vertical") * moveSpeed * Time.fixedDeltaTime);
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * strafeSpeed * Time.fixedDeltaTime);

       if(Input.GetAxis("Fire1") > 0 && fireCooldown <= 0)
       {
           GameObject bullet = (GameObject)Instantiate(bulletPrefab);
           bullet.transform.position = transform.position + transform.up;
           bullet.transform.rotation = transform.rotation;
           fireCooldown = reloadTime;
       }

       facing = transform.up;
    }

	// Update is called once per frame
	void Update () {
        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;
	}
}
