﻿using System.Collections;
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

    // Parameter
    public int type;
    public Transform tf;
    public Rigidbody2D rb;
    public Rigidbody2D rb_player;

    //
    private float MAX_ACCELERATION = 10;


    void updateMovement(Steering steering) {
        if (steering == null) return;
        rb.AddForce(steering.linear, ForceMode2D.Impulse);
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
        }
        else if (type == VELOCITY) {
            steering = move_velocity();
        }
        else {
            return;
        }
        updateMovement(steering);
        // Check kecepatan
        float MAX_SPEED = 100;
        if(rb.velocity.magnitude > MAX_SPEED) {
            rb.velocity = rb.velocity.normalized * MAX_SPEED;
        }
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
        float targetRadius = 20;
        float slowRadius = 100;
        float maxSpeed = 100;
        float maxAcceleration = 50;
        float timeToTarget = 0.1f;
        // Algorithm;
        Vector2 direction = rb.position - rb_player.position;
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
        return null;
    }

    Steering move_velocity() {
        // Parameter
        float maxAcceleration = 5;
        float timeToTarget = 0.1f;
        // Algorithm;
        Steering steering = new Steering();
        steering.linear = rb.velocity - rb_player.velocity;
        steering.linear /= timeToTarget;
        if (steering.linear.magnitude > maxAcceleration) {
            steering.linear = steering.linear.normalized * maxAcceleration;
        }
        steering.angular = 0;
        return steering;
    }
}
