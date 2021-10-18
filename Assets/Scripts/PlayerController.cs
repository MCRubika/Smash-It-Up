using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public float speed = 3;
    public float jumpSpeed = 3;

    public uint health;
    public bool controlEnabled = true;

    //Player Controll Asignation
    public uint playerID = 0;

    //Sprite et animation
    private Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    //jump variable
    private float oldYPosition;
    private float startJumpPosition;
    public float maxJumpHigh = 1;
    private JumpState jumpState = JumpState.Grounded;

    //Colision checks
    public Transform groundCheck;
    private bool isGrippingLeft = false, isGrippingRight = false;

    //Object properties
    private float hight = 0.9f;

    //Attack
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public float hammerProjection = 3;
        //false = droite; true = gauche
    private bool attackDirection = false;
    public float attackRate = 2f;
    float nextAttackTime = 0f;
    public float stunTime = 5f;    


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        oldYPosition = transform.position.y;
        startJumpPosition = transform.position.y;
    }

    private void Update()
    {
        //TODO : Hammer + Git
        //Detail: Enlever le transfer de force entre player lors d'une colision
        //TUTO : https://www.youtube.com/watch?v=44djqUTg2Sg (deplacement)
        //       https://www.youtube.com/watch?v=sPiVz1k-fEs (hammer)

        if (playerID == 0 && Time.time >= stunTime)
        {
            //Gauche + Droite
            if ((Input.GetKey("q") || Input.GetKey("left")) && (!isGrippingLeft || jumpState == JumpState.Grounded))
            {
                rb.velocity = new Vector2(-speed, rb.velocity.y);

                //Rotation sprite
                //TODO: hammer attack point
                //spriteRenderer.flipX = true;
                attackDirection = true;
            }
            else if ((Input.GetKey("d") || Input.GetKey("right")) && (!isGrippingRight || jumpState == JumpState.Grounded))
            {
                rb.velocity = new Vector2(speed, rb.velocity.y);

                //Rotation sprite
                //TODO: hammer attack point
                //spriteRenderer.flipX = false;
                attackDirection = false;
            }
            else
            {
                if (rb.velocity.x > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x - 0.1f, rb.velocity.y);
                }
                else if (rb.velocity.x < 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x + 0.1f, rb.velocity.y);
                }

            }

            //Saut
            if ((Input.GetKey("z") || Input.GetKey("up")) && jumpState == JumpState.Grounded)
            {
                //Debug.Log("Jump");
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                jumpState = JumpState.InFlight;
                startJumpPosition = transform.position.y;
            }

            //Hauteur max
            if (transform.position.y > startJumpPosition + maxJumpHigh)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - 0.05f);
            }

            //Colision Sol
            if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")) ||
                Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Plateform")))
            {
                //le perso touche le sol
                jumpState = JumpState.Grounded;
                startJumpPosition = transform.position.y;
            }
            else
            {
                jumpState = JumpState.InFlight;
                oldYPosition = transform.position.y;
            }

            //Attaque droite et gauche
            if (Input.GetKey("space") && Time.time >= nextAttackTime)
            {
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection des player dans la zone
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

                //On leur applique une velocité (effet de l'attaque)
                foreach (Collider2D enemy in hitEnemies)
                {
                    //Appliquer une velocité
                    //Attention: check la direction pour coord x
                    //enemy.velocity = new Vector2( (+/-) hammerProjection, 0);
                    if (attackDirection)
                    {
                        //direction = gauche
                        //enemy.velocity = new Vector2((hammerProjection, 0);
                        enemy.GetComponent<PlayerController>().applyAttack(-hammerProjection, 0);
                        Debug.Log("Attaque à gauche");
                    }
                    else
                    {
                        //        |
                        //TODO : \/
                        //direction = droite
                        enemy.GetComponent<PlayerController>().applyAttack(hammerProjection, 0);
                        Debug.Log("Attaque à droite");
                    }

                    Debug.Log("Enemy hit");
                }

            }
        }

        

    }

    void applyAttack(float velocityX, float velocityY)
    {
        //Velocité
        rb.velocity = new Vector2(velocityX, velocityY);
        //Stun

    }

    void OnDrawGizmosSelected()
    {
        if(attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }



    void OnCollisionStay2D(Collision2D col)
    {
        //Exception anti "grip" sur le coté des plateforme
        //Bug: Le grip ne s'effectue pas au niveau des pieds. Solution : Changer la HitBox (à faire après changement du sprite et animations)
        if (jumpState == JumpState.InFlight && col.gameObject.tag == "Plateform" && 
            col.gameObject.transform.position.x <= transform.position.x && col.gameObject.transform.position.y >= transform.position.y - hight)
        {
            isGrippingLeft = true;
            //Debug.Log("Gripping Left");
        }
        else if (jumpState == JumpState.InFlight && col.gameObject.tag == "Plateform" && 
            col.gameObject.transform.position.x >= transform.position.x && col.gameObject.transform.position.y >= transform.position.y - hight)
        {
            isGrippingRight = true;
            //Debug.Log("Gripping Right");
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if(col.gameObject.tag == "Plateform" && jumpState == JumpState.InFlight)
        {
            isGrippingLeft = false;
            isGrippingRight = false;
            //Debug.Log("Just stopped gripping");
        }
    }

    public enum JumpState
    {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

}
