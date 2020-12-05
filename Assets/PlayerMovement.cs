﻿using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using UnityEngine.Assertions.Must;

public class PlayerMovement : MonoBehaviour
{

    [Foldout("Movement", true)]
    public float swipeMultiplier;
    public float autoWalkSpeed;
    public float maxDistance;
    [ReadOnly] public bool autoWalk;

    [Foldout("Attack", true)]
    public float minSpeedAtk;
    [ReadOnly] public bool timerActive;
    [ReadOnly] public float maxTimer;

    public float cooldown = 2f;

    [ReadOnly] public bool onCooldown;
    [ReadOnly] public bool canAttack;

    [Foldout("Rotation", true)]
    [ReadOnly] public float Angle;
    [ReadOnly] public float currentAngle;
    [ReadOnly] public float distanceSwiped;

    [Foldout("References", true)]
    private LeanRoll leanRoll;
    private Rigidbody rb;
    private PlayerAttack pAttk;
    public SpriteRenderer spriteRenderer;

    
    // Start is called before the first frame update
    void Start()
    {
        leanRoll = GetComponent<LeanRoll>();
        rb = GetComponent<Rigidbody>();
        pAttk = GetComponent<PlayerAttack>();

        autoWalk = false;
        canAttack = true;
      
    }

    private void Update()
    {

        ResetCombo(timerActive);
        if (onCooldown) canAttack = false;
        else canAttack = true;
    }

    void FixedUpdate()
    {
        if (autoWalk) AutoWalk();

        // Attack
        if (rb.velocity.magnitude > minSpeedAtk && canAttack) pAttk.TriggerON();
        else if (rb.velocity.magnitude < minSpeedAtk) pAttk.TriggerOFF();
    }
    
    void ResetCombo(bool whatever)
    {
        if(whatever)
        {
            maxTimer -= Time.deltaTime;

            if (maxTimer <= 0)
            {
                timerActive = false;
                canAttack = false;
                maxTimer = 0;

                StartCoroutine(Cooldown());
            }
            else canAttack = true;
        }
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSecondsRealtime(cooldown);

        onCooldown = false;
        canAttack = true;
    }

    public void OnDistance(float distance)
    {
        distanceSwiped = distance;
        rb.velocity = Vector3.zero;

    
        // Dashing
        if (distanceSwiped != 0 && canAttack)
        {
            StartCoroutine(Dash());
        }
    }

    public IEnumerator Dash()
    {
        distanceSwiped *= swipeMultiplier;
        if (distanceSwiped > maxDistance)
            distanceSwiped = maxDistance;

        Vector3 vectorApplied = transform.position + transform.forward * distanceSwiped;

        rb.AddForce(vectorApplied);

        yield return new WaitForSeconds(0.2f);
        
        distanceSwiped = 0;
        autoWalk = true;
    }

    
    void AutoWalk()
    {

        Vector3 vector_autoWalk = transform.position + transform.forward * autoWalkSpeed;
        rb.MovePosition(vector_autoWalk);
    }
    
    public void Stop(LeanFinger leanFinger)
    {
        if (leanFinger.Tap)
        {
            autoWalk = false;
            canAttack = true;
            rb.velocity = Vector3.zero;
        }
    }

    public void RotatePlayer(Vector2 delta)
    {
        if (delta.sqrMagnitude > 0.0f)
        {
            Angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
            var factor = LeanTouch.GetDampenFactor(-1, Time.deltaTime);
            currentAngle = Mathf.LerpAngle(currentAngle, Angle, factor);

            if(Angle < 0) spriteRenderer.flipX = true;
            else spriteRenderer.flipX = false;

            float cameraAngle = Camera.main.transform.eulerAngles.y;
            float productAngle = cameraAngle + currentAngle;
            Quaternion lookAt = Quaternion.Euler(0, productAngle, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, 500 * Time.deltaTime);
            currentAngle = 0;
        }

    }

    

}
