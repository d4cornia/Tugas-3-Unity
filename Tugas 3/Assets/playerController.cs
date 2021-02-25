using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class playerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject playerObj;
    public float maxSpeed;
    public Vector2 force;
    public Animator animator;
    private int look;


    [SerializeField]
    private fieldOfView fov;

    void Awake()
    {
        if(rb == null)
        {
            playerObj = GameObject.Find("Player");
            rb = playerObj.GetComponent<Rigidbody2D>();
        }
        look = 4;
    }

    private void Update()
    {
        processInput();
    }

    void FixedUpdate()
    {
        move();
        walkAnim();
        updateOrientationPlayer();
        spriteOrientation();
    }

    private float moveX, moveY;

    void processInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
        
        float xForce = moveX * maxSpeed;
        float yForce = moveY * maxSpeed;

        force = new Vector2(xForce, yForce);
    }

    void move()
    {
        if (moveX == 0 && moveY == 0)
        {
            rb.drag = 1;
        }
        rb.AddForce(force);
    }

    public void updateOrientationPlayer()
    {
        Vector3 targetPosition = UtilsClass.GetWorldPositionFromUI();
        fov.setAimDirection((targetPosition - transform.position).normalized);
        fov.setOrigin(rb.transform.position);
    }

    void spriteOrientation()
    {
        if (fov.startingAngle > 60 && fov.startingAngle < 170) look = 1;
        else if (fov.startingAngle > 170 && fov.startingAngle < 250) look = 3;
        else if (fov.startingAngle > 250 && fov.startingAngle < 330) look = 4;
        else if (fov.startingAngle < 60 || fov.startingAngle > 330) look = 2;

        animator.SetInteger("Look", look);
        look = 0;
    }

    void walkAnim()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x + rb.velocity.y));
        if (Mathf.Abs(rb.velocity.x + rb.velocity.y) > 2) animator.speed = 1.3f;
        else animator.speed = 0.6f;
    }
}
