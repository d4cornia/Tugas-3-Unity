﻿using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper Class (Steering)
class Steering {
    public Vector2 linear { get; set; }
    public float angular { get; set; }
}

public class enemyController : MonoBehaviour
{
    // Constant
    public const int SEEK = 0;
    public const int FLEE = 1;
    public const int ARRIVE = 2;
    public const int ALIGN = 3;
    public const int VELOCITY = 4;
    public Animator animator;
    private int look;

    // Parameter
    public int type;
    public Transform tf;
    public Rigidbody2D rb;
    public Rigidbody2D rb_player;

    [SerializeField]
    private fovEnemy fov;


    //
    public float MAX_ACCELERATION = 1;
    public float MAX_SPEED = 3;

    private void Awake()
    {
        look = 4;
    }

    void spriteOrientation()
    {
        if (fov.startingAngle > 60 && fov.startingAngle < 170) look = 1;
        else if (fov.startingAngle > 170 && fov.startingAngle < 250) look = 3;
        else if (fov.startingAngle > 250 && fov.startingAngle < 330) look = 4;
        else if (fov.startingAngle < 60 || fov.startingAngle > 330) look = 2;
        Debug.Log(look);
        animator.SetInteger("Look", look);
        look = 0;


    }

    void walkAnim()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x + rb.velocity.y));
        if (Mathf.Abs(rb.velocity.x + rb.velocity.y) > 2) animator.speed = 1.3f;
        else animator.speed = 0.6f;

    }

    void updateMovement(Steering steering) {
        if (steering == null) {
            rb.AddForce(new Vector2(), ForceMode2D.Impulse);
        } else {
            rb.AddForce(steering.linear * Time.deltaTime, ForceMode2D.Impulse);
            if(type != 3)
            {
                fov.startingAngle = UtilsClass.GetAngleFromVector(rb.velocity.normalized) + fov.fov / 2f;
            }
            fov.setOrigin(rb.transform.position);
        }
    }

    private void FixedUpdate() {
        Steering steering = null;
        if(type == SEEK) {
            steering = move_seek();
        }
        else if(type == FLEE) {
            steering = move_flee();
        }
        else if (type == ARRIVE) {
            steering = move_arrive();
        }
        else if (type == ALIGN) {
            steering = move_align();
            move_align2();
        }
        else if (type == VELOCITY) {
            steering = move_velocity();
        }
        else {
            return;
        }
        updateMovement(steering);
        // Check kecepatan
        if(rb.velocity.magnitude > MAX_SPEED) {
            rb.velocity = rb.velocity.normalized * MAX_SPEED;
        }

        walkAnim();
        spriteOrientation();
    }

    Steering move_seek() {
        Steering steering = new Steering();
        steering.linear = rb_player.position - rb.position;
        steering.linear = steering.linear.normalized * MAX_ACCELERATION;
        steering.angular = 0;
        return steering;
    }
    Steering move_flee() {
        Steering steering = new Steering();
        steering.linear = rb.position - rb_player.position;
        steering.linear = steering.linear.normalized * MAX_ACCELERATION;
        steering.angular = 0;
        return steering;
    }
    Steering move_arrive() {
        // Parameter
        float targetRadius = 2;
        float slowRadius = 10;
        float maxSpeed = 4;
        float maxAcceleration = 2;
        float timeToTarget = 0.1f;
        // Algorithm;
        Vector2 direction = rb_player.position - rb.position;
        float distance = direction.magnitude;
        if(distance < targetRadius) {
            return null;
        }
        float targetSpeed;
        if(distance > slowRadius) {
            targetSpeed = maxSpeed;
        }
        else {
            targetSpeed = maxSpeed * distance / slowRadius;
        }

        Vector2 targetVelocity = direction;
        targetVelocity = targetVelocity.normalized * targetSpeed;

        Steering steering = new Steering();
        steering.linear = targetVelocity - rb.velocity;
        steering.linear /= timeToTarget;
        if(steering.linear.magnitude > maxAcceleration) {
            steering.linear = steering.linear.normalized * maxAcceleration;
        }
        steering.angular = 0;
        return steering;
    }
    Steering move_align() {
        // Parameter
        float targetRadius = fov.fov;
        float slowRadius = 100;
        float maxRotation = 100;
        float maxAngularAcceleration = 50;
        float timeToTarget = 0.1f;
        // Algorithm;
        float char_rotation = fov.startingAngle;
        float target_rotation = GameObject.Find("Player").GetComponent<playerController>().fov.startingAngle;
        float rotation = target_rotation - char_rotation; // Target-Character.orientation
        rotation = rotation % 360;
        // Debug.Log("Rotation : " + rotation + " target_rotation: " + target_rotation + " character rotation: " + char_rotation);
        float rotationSize = Mathf.Abs(rotation);

        // Jika Player tidak ada dlm radius maka Ignore
        if(rotationSize < targetRadius) {
            return null;
        }
        float targetRotation;
        if (rotationSize > slowRadius) {
            // Jika diatas slowradius
            targetRotation = maxRotation;
        } else {
            // Jika dibawah sama dengan slowradius maka
            targetRotation = rotationSize * maxRotation / slowRadius;
        }
        targetRotation *= rotation / rotationSize;

        Steering steering = new Steering();
        // Rotation
        steering.angular = targetRotation - char_rotation;
        steering.angular /= timeToTarget;
        // Normalize Rotation
        float angularAcceleration = Mathf.Abs(steering.angular);
        if(angularAcceleration > maxAngularAcceleration) {
            steering.angular /= angularAcceleration;
            steering.angular *= maxAngularAcceleration;
        }
        // Output Linear
        steering.linear = new Vector3();
        // Return
        return steering;
    }

    void move_align2()
    {
        // Debug.Log("distance : " + Vector3.Distance(rb.position, rb_player.position));
        if (Vector3.Distance(rb.position, rb_player.position) < fov.viewDistance)
        {
            Vector3 dirToPlayer = (rb_player.position - rb.position).normalized;
            if (fov.hit)
            {
                fov.setAimDirection(dirToPlayer);
            }
        }
        fov.hit = false;
    }

    Steering move_velocity() {
        // Parameter
        float maxAcceleration = 2;
        float timeToTarget = 0.1f;
        // Algorithm;
        Steering steering = new Steering();
        steering.linear = rb_player.velocity - rb.velocity;
        steering.linear /= timeToTarget;
        if (steering.linear.magnitude > maxAcceleration) {
            steering.linear = steering.linear.normalized * maxAcceleration;
        }
        steering.angular = 0;
        return steering;
    }
}
