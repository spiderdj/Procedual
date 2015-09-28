using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public float speed = 100.0f;
	// Use this for initialization
	void Start () {
	
	}
	
    void FixedUpdate()
    {
        transform.Translate(Vector3.up * speed * Time.fixedDeltaTime);
    }
	// Update is called once per frame
	void Update () {
        
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(gameObject);
    }
}
