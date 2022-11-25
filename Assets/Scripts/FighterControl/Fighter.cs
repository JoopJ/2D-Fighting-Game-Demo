using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spi;

    [SerializeField]
    FighterControl fighterControl;
    // add AI fighter controller

    [SerializeField]
    Fighter enemyFighter;

    float sceneEdge = 8;

#region Movement

    [SerializeField]
    float moveForce = 0.05f;

    [SerializeField]
    float drag = 0.1f;

    [SerializeField]
    float dragThreshold = 0.2f;
    

    [SerializeField]
    float dragToApply;

    [SerializeField]
    float jumpForce =  0.2f;
    [SerializeField]
    float stompJumpForce = 7.5f;    // jump force when you land on enemies head

    private Vector3 velocity;

    float slowMultiplier;

    bool left;
    bool right;
#endregion

#region Collision Detection

    bool grounded;
    bool enemyBodyCollision;
    float colliderWidth = 0.85f;
    float colliderHeight = 1.25f;
    Vector3 enemyFighterPos;

#endregion

#region Blocking

    bool blocking;
    bool stunned = false;
    [SerializeField]
    float pushBackForce = 0.3f;
    [SerializeField]
    float stunTime = 1;
    [SerializeField]
    float stunTimer;

#endregion

#region Animation

    private Animator animator;

    bool facingRight = true;

#endregion


#region Gameplay

    int HP = 100;

#endregion

#region Attacks
    [SerializeField]
    float lightAttackReach;
    [SerializeField]
    int lightAttackDamage = 5;
    [SerializeField]
    float heavyAttackReach;
    [SerializeField]
    int heavyAttackDamage = 10;
    [SerializeField]
    Vector3 attackOffset;
    [SerializeField]
    float lightAttackTime;
    [SerializeField]
    float heavyAttackTime;
    float attackTimer = 0;

#endregion




    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        spi = GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }

    private void Update() {
        if (!stunned) {
            // stop movement when colliding with enemy.
            if (fighterControl.left && !enemyBodyCollision) { left = true; MoveLeft(); } else { left = false; }
            if (fighterControl.right && !enemyBodyCollision) { right = true; MoveRight(); } else { right = false; }

            if (fighterControl.jump && grounded) { Jump(); }

            if (fighterControl.lightAttack && attackTimer <= 0) { LightAttack(); } else { animator.SetBool("LightAttack", false); }
            if (fighterControl.heavyAttack && attackTimer <= 0) { HeavyAttack(); } else { animator.SetBool("HeavyAttack", false); }
            attackTimer -= Time.deltaTime;

            // animate movement when moving in either direction.
            if (left != right) { animator.SetBool("Moving", true); } else { animator.SetBool("Moving", false); }
        } else {
            left = false;
            right = false;
            stunTimer -= Time.deltaTime;
        }
        // enemy position required for UpdatePosition
        enemyFighterPos = enemyFighter.GetPosition();
        UpdatePosition();

        // face the fighter towards the enemy 
        if (enemyFighterPos.x > transform.position.x && !facingRight) {
            facingRight = true;
            Flip();
        } else if (enemyFighterPos.x < transform.position.x && facingRight) { 
            facingRight = false;
            Flip();
        }
    }

        private void UpdatePosition() {
        // greatly slow the fighter when not moving left or right
        slowMultiplier = (left != right) ? 1 : 10;
        // apply drag after a certain threshold velocity, so fighter can gain speed quickly
        dragToApply = (Mathf.Abs(velocity.x) > dragThreshold || left == right) ? drag : 0;
        
        // clamp the Fighter to within the bounds of scene
        velocity -= Vector3.ClampMagnitude(velocity, 1) * (dragToApply * slowMultiplier ) * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
        if (transform.position.x > sceneEdge) {
            transform.position = new Vector3(sceneEdge, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < -sceneEdge) {
            transform.position = new Vector3(-sceneEdge, transform.position.y, transform.position.z);
        } 

        // push away from enemy when colliding.
        if (enemyBodyCollision) {
            rb.AddForce(new Vector3 (transform.position.x - enemyFighterPos.x, 0, 0) * pushBackForce);
        }

        // check position of enemy fighter, flip facing direction if not facing enemy
    }

    private void Flip() {
        spi.flipX = !spi.flipX;
        Debug.Log("Flipped!");
    }
    
    private void MoveLeft() {
        // move left at constant velocity
        velocity += Vector3.left * moveForce * Time.fixedDeltaTime;
    }

    private void MoveRight() {
        // move right at constant velocity
        velocity += Vector3.right * moveForce * Time.fixedDeltaTime;
    }

    private void Jump() {
        rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        animator.SetBool("Jumped", true);
    }

    private void LightAttack() {
        // do a light attack
        animator.SetBool("LightAttack", true); // plays animation
        attackTimer = lightAttackTime;
        Debug.Log("Light");

        Vector3 direction;
        if (facingRight) {
            direction = Vector3.right;
            attackOffset.x = (colliderWidth/2 + 0.1f);
        } else {
            direction = Vector3.left;
            attackOffset.x = -(colliderWidth/2 + 0.1f);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position + attackOffset, direction, lightAttackReach);
        Debug.DrawRay(transform.position + attackOffset, direction * lightAttackReach, Color.red, lightAttackTime);

        if (hit.collider != null) {
            Debug.Log(gameObject.name + " Hit something: " + hit.collider.name);
            if (hit.collider.tag == "Fighter") {
                enemyFighter.TakeDamage(lightAttackDamage);
            }
        }
    }

    private void HeavyAttack() {
        // do a heavy attack
        animator.SetBool("HeavyAttack", true);
        attackTimer = heavyAttackTime;
        Debug.Log("Heavy");

        // set the direction and add x offset depending on facing direction. x offset prevents raycast hitting own collider
        Vector3 direction;
        if (facingRight) {
            direction = Vector3.right;
            attackOffset.x = (colliderWidth/2 + 0.1f);
        } else {
            direction = Vector3.left;
            attackOffset.x = -(colliderWidth/2 + 0.1f);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position + attackOffset, direction, heavyAttackReach);
        Debug.DrawRay(transform.position + attackOffset, direction * heavyAttackReach, Color.blue, heavyAttackTime);

        // if attack hits the enemy fighter, deal damage to them.
        if (hit.collider != null) {
            Debug.Log(gameObject.name + " Hit something: " + hit.collider.name);
            if (hit.collider.tag == "Fighter" && hit.transform != transform) {
                Debug.Log("Enemy Hit");
                enemyFighter.TakeDamage(heavyAttackDamage);
            }
        }
    }

    private void Block() {
        // block, continue to block until you unblock
        blocking = true;
    }

    private void Unblock() {
        // unblock if blocking
        blocking = false;
    }

    public void TakeDamage(int dmg) {
        // take no damage if blocking
        if (!blocking) {
            animator.SetTrigger("Hurt");
        }
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Floor") {
            grounded = true;
            animator.SetBool("Grounded", true);
        }
        
        if (col.gameObject.tag == "Fighter") {
            enemyBodyCollision = true;

            enemyFighterPos = enemyFighter.GetPosition();
            // check if the fighers are on top of each other
            if (enemyFighterPos.x < transform.position.x + (colliderWidth) &&
                enemyFighterPos.x > transform.position.x - (colliderWidth)) {
                    Debug.Log("X positions line up.");
                    // if this fighter is on top of enemy, jump and damage the enemy
                    if (enemyFighterPos.y < transform.position.y) {
                        rb.AddForce(Vector3.up * stompJumpForce, ForceMode2D.Impulse);
                        enemyFighter.TakeDamage(5);
                    }
                }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "Floor") {
            grounded = false;

            // update animator information
            animator.SetBool("Grounded", false);
            animator.SetBool("Jumped", false);
        }

        if (col.gameObject.tag == "Fighter") {
            enemyBodyCollision = false;
        }
    }
}
