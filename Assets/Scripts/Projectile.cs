using UnityEngine;

public class Projectile : MonoBehaviour
{
    float vitesse = 10f;
    public float directionHorizontale = 1;

    Rigidbody2D rigid;


    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rigid.linearVelocityX = directionHorizontale * vitesse;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this.gameObject);
    }
}
