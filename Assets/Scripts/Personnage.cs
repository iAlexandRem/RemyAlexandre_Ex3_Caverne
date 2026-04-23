using UnityEngine;
using UnityEngine.InputSystem;

public class Personnage : MonoBehaviour
{
    [Header("Gestion boutons")]
    public InputAction actionDeplacement;
    public InputAction actionSaut;
    public InputAction actionTir;
    public InputAction actionDash;
    public InputAction actionAccroupirSoi;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    [Header("Déplacement horizontal")]
    public bool peutBouger = true;
    Vector2 positionDepart;
    public float vitesseDeplacement;
    public float directionDeplacement;

    bool enDeplacement = false;

    [Header("Saut")]
    public float forceSaut;
    public bool estEnSaut;
    public bool estAuSol;
    public LayerMask masqueSol;

    [Header("Tir")]
    public GameObject prefabProjectile;
    public Transform positionProjectile;
    public float delaiTirMin = 1f;
    public float tempsEntreTir = 0f;

    [Header("Dash")]
    public float forceDash = 10f;

    AudioSource audioSource;
    public AudioClip sfxSonShoot;
    public AudioClip sfxBasseFrequence;
    public AudioClip sfxSaut;
    public AudioClip sfxDash;


    void OnEnable()
    {
        actionDeplacement.Enable();
        actionSaut.Enable();
        actionTir.Enable();
        actionDash.Enable();
        actionAccroupirSoi.Enable();
    }

    void OnDisable()
    {
        actionDeplacement.Disable();
        actionSaut.Disable();
        actionTir.Disable();
        actionDash.Disable();
        actionAccroupirSoi.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        positionDepart = transform.position;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (directionDeplacement < 0)
        {
            sr.flipX = true;
        }
        else if (directionDeplacement > 0)
        {
            sr.flipX = false;
        }
        //Debug.Log(directionDeplacement);


        estAuSol = Physics2D.Raycast(transform.position, Vector2.down, 1f, masqueSol);
        if (anim != null)
        {
            anim.SetBool("estAuSol", estAuSol);
        }
        Debug.DrawRay(transform.position, Vector2.down, Color.orange);

        if (tempsEntreTir > 0)
        {
            tempsEntreTir -= Time.deltaTime;
        }

        if (actionTir.WasPressedThisFrame() == true && tempsEntreTir <= 0) // && !enDeplacement pour ne pas glisser en shootant mais c'est plus fun de bouger et shoot
        {
            anim.SetTrigger("estEnTir");
            audioSource.PlayOneShot(sfxSonShoot);

            tempsEntreTir = delaiTirMin;
            float directionPersonnage = 1;

            Vector2 nouvellePositionDepart = positionProjectile.localPosition;
            nouvellePositionDepart.x = 1.5f;

            if (sr.flipX == true)
            {
                directionPersonnage = -1;
                nouvellePositionDepart.x = -1.5f;
            }

            positionProjectile.localPosition = nouvellePositionDepart;

            GameObject clone = Instantiate(prefabProjectile, positionProjectile.position, positionProjectile.rotation);

            clone.GetComponent<Projectile>().directionHorizontale = directionPersonnage;
        }



        if (actionDash.WasPressedThisFrame() == true) // C'est le temps de DASH let's go
        {
            anim.SetTrigger("aDash");
            audioSource.PlayOneShot(sfxDash, 0.3f);


            float direction; // Nouveau float de direction, car celui de directionDeplacement est nul de base 

            if (sr.flipX)
            {
                direction = -1f;
            }
            else
            {
                direction = 1f;
            }

            rb.AddForce(new Vector2(direction * forceDash, 0f), ForceMode2D.Impulse); // Force dans l'axe des x, selon direction du sprite

        }



        if (actionAccroupirSoi.IsPressed() == true && !enDeplacement)
        {
            anim.SetBool("estAccroupi", true);
        }
        else
        {
            anim.SetBool("estAccroupi", false);
        }








    }

    // FixedUpdate is called at a constant interval and is very good for physics
    void FixedUpdate()
    {
        if (peutBouger)
        {

            directionDeplacement = actionDeplacement.ReadValue<float>();
            estEnSaut = actionSaut.WasPressedThisFrame();

            if (directionDeplacement != 0)
            {
                rb.linearVelocityX = directionDeplacement * vitesseDeplacement;
                enDeplacement = true;
            }
            else
            {
                enDeplacement = false;
            }

            float vitesseAbsolue = Mathf.Abs(rb.linearVelocityX);
            if (anim != null)
            {
                anim.SetFloat("vitesse", vitesseAbsolue);
            }

            if (estAuSol == true && estEnSaut == true)
            {
                rb.AddForce(Vector2.up * forceSaut, ForceMode2D.Impulse);
                audioSource.PlayOneShot(sfxSaut, 0.5f);
            }
        }


    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Lave")
        {
            transform.position = positionDepart;

            anim.SetBool("estBlesse", true);
            audioSource.PlayOneShot(sfxBasseFrequence);
            peutBouger = false;
            Invoke("ReviensAuRepos", 1f);
        }


    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ennemi")
        {
            peutBouger = false;
            float direction = -1;

            if (sr.flipX == true)
            {
                direction = 1;
            }

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction, 1) * 10, ForceMode2D.Impulse);
            anim.SetBool("estBlesse", true);
            Invoke("ReviensAuRepos", 1f);
        }
    }

    void ReviensAuRepos()
    {
        peutBouger = true;
        anim.SetBool("estBlesse", false);
    }
}
