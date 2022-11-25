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

    private Vector3 velocity;

    float slowMultiplier;

    bool left;
    bool right;
#endregion

#region Collision Detection

    bool grounded;
    bool enemyBodyCollision;

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

            if (fighterControl.lightAttack) { LightAttack(); } else { animator.SetBool("LightAttack", false); }
            if (fighterControl.heavyAttack) { HeavyAttack(); } else { animator.SetBool("HeavyAttack", false); }

            // animate movement when moving in either direciton.
            if (left != right) { animator.SetBool("Moving", true); } else { animator.SetBool("Moving", false); }
        } else {
            left = false;
            right = false;
            stunTimer -= Time.deltaTime;
        }
        UpdatePosition();
        // add a check for if the enemy changes side of fighter - make it flip the fighter
        //      they are both making that same calculation so it could be done by one script and applied to both.

        // face the fighter towards the enemy
        if (enemyFighter.GetXPosition() > transform.position.x && !facingRight) {
            facingRight = true;
            Flip();
        } else if (enemyFighter.GetXPosition() < transform.position.x && facingRight) { 
            facingRight = false;
            Flip();
        }
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
            rb.AddForce(new Vector3 (transform.position.x - enemyFighter.GetXPosition(), 0, 0) * pushBackForce);
        }

        // check position of enemy fighter, flip facing direciton if not facing enemy
    }

    private void LightAttack() {
        // do a light punch
        animator.SetBool("LightAttack", true); // plays animation



    }

    private void HeavyAttack() {
        // do a heavy kick
        animator.SetBool("HeavyAttack", true);
    }

    private void Block() {
        // block, continue to block until you unblock
    }

    private void Unblock() {
        // unblock if blocking
    }

    public float GetXPosition() {
        return transform.position.x;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Floor") {
            grounded = true;
            animator.SetBool("Grounded", true);
        }
        
        if (col.gameObject.tag == "Fighter") {
            enemyBodyCollision = true;
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
