using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("Inputs")]
    public KeyCode leftKey, rightkey, jumpKey, attackKey;

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
    public Transform attackPointL, attackPointR;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public float hammerProjection = 3;
        //false = droite; true = gauche
    private bool attackDirection = false;
    public float attackRate = 2f;
    float nextAttackTime = 0f;
    public float stunTime = 5f;
    private float stunTimeActu = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        oldYPosition = transform.position.y;
        startJumpPosition = transform.position.y;
    }

    private void Update()
    {
        //TODO : Stun
        //Error : Projection plus effective après un premier mouvement

        if (Time.time >= stunTimeActu)
        {
            //reset var
            stunTimeActu = 0;

            //Gauche + Droite
            if (Input.GetKey(leftKey) && (!isGrippingLeft || jumpState == JumpState.Grounded))
            {
                rb.velocity = new Vector2(-speed, rb.velocity.y);

                //Rotation sprite
                //TODO: hammer attack point
                //spriteRenderer.flipX = true;
                attackDirection = true;
            }
            else if (Input.GetKey(rightkey) && (!isGrippingRight || jumpState == JumpState.Grounded))
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
            if (Input.GetKey(jumpKey) && jumpState == JumpState.Grounded)
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
            //Gauche
            if (attackDirection && Input.GetKey(attackKey) && Time.time >= nextAttackTime)
            {
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection des player dans la zone
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayer);
                //Collider2D[] hitEnemiesL = Physics2D.OverlapCircleAll(attackPointR.position, -attackRange, enemyLayer);

                //On leur applique une velocité (effet de l'attaque)
                foreach (Collider2D enemy in hitEnemies)
                {
                    //Appliquer une velocité
                    //Attention: check la direction pour coord x
                    
                    //direction = gauche
                    //enemy.velocity = new Vector2((hammerProjection, 0);
                    enemy.GetComponent<PlayerController>().applyAttack(-hammerProjection, 0);
                    Debug.Log("Attaque à Gauche");
                    
                    Debug.Log("Enemy hit");
                }
            }

            if (!attackDirection && Input.GetKey(attackKey) && Time.time >= nextAttackTime)
            {
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection des player dans la zone
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayer);
                //Collider2D[] hitEnemiesL = Physics2D.OverlapCircleAll(attackPointR.position, -attackRange, enemyLayer);

                //On leur applique une velocité (effet de l'attaque)
                foreach (Collider2D enemy in hitEnemies)
                {
                    //Appliquer une velocité
                    //Attention: check la direction pour coord x

                    //direction = gauche
                    //enemy.velocity = new Vector2((hammerProjection, 0);
                    enemy.GetComponent<PlayerController>().applyAttack(hammerProjection, 0);
                    Debug.Log("Attaque à Droite");

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
        stunTimeActu = stunTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPointR.position, attackRange);
        Gizmos.DrawWireSphere(attackPointL.position, attackRange);
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
