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
    public float wallJumpSpeed = 1;
    private JumpState jumpState = JumpState.InFlight;
    public float wallJumpMovementFreeze = 0.2f;
    private float wallJumpMovementFreezeActuL, wallJumpMovementFreezeActuR;

    //Colision checks
    public Transform groundCheck;
    private bool isGrippingLeft = false, isGrippingRight = false;

    //Object properties
    private float hight = 0.9f;

    //Attack
    public Transform attackPointL, attackPointR;
    public Transform hammerPointL, hammerPointR;
    public float attackRange = 0.5f;
    public float hammerHitboxRange = 1f;
    public LayerMask enemyLayer, hammerHitboxLayer;
    public float hammerProjection = 3;
        //false = droite; true = gauche
    private bool attackDirection = false;
    public float attackRate = 2f;
    public float attackDuration = 1f;
    private float attackDurationActu;
    private bool isAttackRunningL, isAttackRunningR;
    float nextAttackTime = 0f;
    public float stunTime = 1f;
    private float stunTimeActu;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        oldYPosition = transform.position.y;
        startJumpPosition = transform.position.y;
        stunTimeActu = 0;
        wallJumpMovementFreezeActuL = 0;
        wallJumpMovementFreezeActuR = 0;
        attackDurationActu = 0;
        isAttackRunningL = false;
        isAttackRunningR = false;
    }

    private void Update()
    {
        //TODO : 
        //Error : jump physics

        //stun also equal to immortality
        if (Time.time >= stunTimeActu)
        {
            //reset var
            stunTimeActu = 0;

            /////////////////////////////////////
            //////////// DEPLACEMENT ////////////
            /////////////////////////////////////

            //Gauche + Droite
            if (Input.GetKey(leftKey) && (!isGrippingLeft || jumpState == JumpState.Grounded) && Time.time >= wallJumpMovementFreezeActuL && !isAttackRunningL && !isAttackRunningR)
            {
                //gauche
                rb.velocity = new Vector2(-speed, rb.velocity.y);
                attackDirection = true;
            }
            else if (Input.GetKey(rightkey) && (!isGrippingRight || jumpState == JumpState.Grounded) && Time.time >= wallJumpMovementFreezeActuR && !isAttackRunningL && !isAttackRunningR)
            {
                //droite
                rb.velocity = new Vector2(speed, rb.velocity.y);
                attackDirection = false;
            }
            else if (jumpState != JumpState.InFlight)
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
            if (Input.GetKeyDown(jumpKey) && jumpState == JumpState.Grounded && !isAttackRunningL && !isAttackRunningR)
            {
                //Debug.Log("Jump");
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                jumpState = JumpState.InFlight;
                startJumpPosition = transform.position.y;
            }
            else if (Input.GetKeyDown(jumpKey) && isGrippingRight && !isAttackRunningL && !isAttackRunningR)
            {
                //jump to the left w/ 45° angle
                rb.velocity = new Vector2(-wallJumpSpeed, jumpSpeed / (Mathf.Sqrt(2) / 2));
                jumpState = JumpState.InFlight;
                startJumpPosition = transform.position.y;

                //freeze movement for small time
                wallJumpMovementFreezeActuR = wallJumpMovementFreeze + Time.time;
            }
            else if (Input.GetKeyDown(jumpKey) && isGrippingLeft && !isAttackRunningL && !isAttackRunningR)
            {
                //jump to the right w/ 45° angle
                rb.velocity = new Vector2(wallJumpSpeed, jumpSpeed / (Mathf.Sqrt(2) / 2));
                jumpState = JumpState.InFlight;
                startJumpPosition = transform.position.y;

                //freeze movement for small time
                wallJumpMovementFreezeActuL = wallJumpMovementFreeze + Time.time;
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

                //reset var for walljump
                wallJumpMovementFreezeActuL = Time.time;
                wallJumpMovementFreezeActuR = Time.time;
            }
            else
            {
                jumpState = JumpState.InFlight;
                oldYPosition = transform.position.y;
            }

            /////////////////////////////////
            //////////// ATTAQUE ////////////
            /////////////////////////////////

            //Attaque droite et gauche
            //Gauche
            
            if (attackDirection && Input.GetKey(attackKey) && Time.time >= nextAttackTime)
            {
                Debug.Log("OUI1G");
                isAttackRunningL = true;
                attackDurationActu = attackDuration + Time.time;
            }

            
            if (isAttackRunningL && Time.time >= attackDurationActu)
            {
                Debug.Log("OUI2G");
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection d'un blocage
                Collider2D[] hammers = Physics2D.OverlapCircleAll(hammerPointL.position, hammerHitboxRange, hammerHitboxLayer);
                if (hammers.Length > 0)
                {
                    //on contre
                    Debug.Log("Blocage à gauche");
                }
                else
                {
                    //Detection des player dans la zone
                    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayer);

                    //On leur applique une velocité (effet de l'attaque)
                    foreach (Collider2D enemy in hitEnemies)
                    {
                        //Appliquer une velocité
                        //Attention: check la direction pour coord x
                        enemy.GetComponent<PlayerController>().applyAttack(-hammerProjection, 0);
                        Debug.Log("Attaque à Gauche");

                        Debug.Log("Enemy hit");
                    }
                }
                isAttackRunningL = false;
            }


            //Droite
            if (!attackDirection && Input.GetKey(attackKey) && Time.time >= nextAttackTime)
            {
                Debug.Log("OUI1D");
                isAttackRunningR = true;
                attackDurationActu = attackDuration + Time.time;
            }

            if (isAttackRunningR && Time.time >= attackDurationActu)
            {
                Debug.Log("OUI2D");
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection d'un blocage
                Collider2D[] hammers = Physics2D.OverlapCircleAll(hammerPointR.position, hammerHitboxRange, hammerHitboxLayer);
                if (hammers.Length > 0)
                {
                    //on contre
                    Debug.Log("Blocage à Droite");
                }
                else
                {
                    //Detection des player dans la zone
                    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayer);

                    //On leur applique une velocité (effet de l'attaque)
                    foreach (Collider2D enemy in hitEnemies)
                    {
                        //Appliquer une velocité
                        //Attention: check la direction pour coord x
                        enemy.GetComponent<PlayerController>().applyAttack(hammerProjection, 0);
                        Debug.Log("Attaque à Droite");

                        Debug.Log("Enemy hit");
                    }
                }
                isAttackRunningR = false;
            }
            

            /*
            //Gauche
            if (attackDirection && Input.GetKey(attackKey) && Time.time >= nextAttackTime)
            {
                Debug.Log("OUI1G");
                isAttackRunningL = true;
                attackDurationActu = attackDuration + Time.time;
            }

            if (isAttackRunningL && Time.time >= attackDurationActu)
            {
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection des player dans la zone
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayer);

                //On leur applique une velocité (effet de l'attaque)
                bool _isBlocked = false;
                foreach (Collider2D enemy in hitEnemies)
                {
                    //Appliquer une velocité
                    //Attention: check la direction pour coord x
                    enemy.GetComponent<PlayerController>().applyAttack(-hammerProjection, 0);
                    if (enemy.GetComponent<PlayerController>().isAttackRunningR() == true) _isBlocked = true;
                    Debug.Log("Attaque à Gauche");

                    Debug.Log("Enemy hit");
                }

                //fin attaque
                isAttackRunningL = false;
            }

            //Droite
            if (attackDirection && Input.GetKey(attackKey) && Time.time >= nextAttackTime)
            {
                Debug.Log("OUI1G");
                isAttackRunningL = true;
                attackDurationActu = attackDuration + Time.time;
            }

            if (isAttackRunningR && Time.time >= attackDurationActu)
            {
                //reset timeAttack
                nextAttackTime = Time.time + 1f / attackRate;

                //Animation / Attack hitbox Apparition (pour test)

                //Detection des player dans la zone
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayer);

                //On leur applique une velocité (effet de l'attaque)
                foreach (Collider2D enemy in hitEnemies)
                {
                    //Appliquer une velocité
                    //Attention: check la direction pour coord x
                    enemy.GetComponent<PlayerController>().applyAttack(hammerProjection, 0);
                    Debug.Log("Attaque à Droite");

                    Debug.Log("Enemy hit");
                }
                
            //fin attaque
            isAttackRunningR = false;
            }
            */
            
                        
        }
    }

    void applyAttack(float velocityX, float velocityY)
    {
        //Stun
        stunTimeActu = stunTime + Time.time;

        //Velocité
        rb.velocity = new Vector2(velocityX, velocityY);
    }

    bool isAttackingL()
    {
        if (isAttackRunningL) return true;
        return false;
    }

    bool isAttackingR()
    {
        if (isAttackRunningR) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPointR.position, attackRange);
        Gizmos.DrawWireSphere(attackPointL.position, attackRange);
        Gizmos.DrawWireSphere(hammerPointL.position, hammerHitboxRange);
        Gizmos.DrawWireSphere(hammerPointR.position, hammerHitboxRange);
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
