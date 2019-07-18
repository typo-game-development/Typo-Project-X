﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TorquePIDController : MonoBehaviour
{
    private readonly VectorPid angularVelocityController = new VectorPid(0.7766f, 0, 0.2553191f);
    private readonly VectorPid headingController = new VectorPid(15.244681f, 0, 0.06382979f);

    private Rigidbody rigidBody;

    private Vector3 angularVelocityError;
    private Vector3 angularVelocityCorrection;
    private Vector3 desiredHeading;
    private Vector3 currentHeading;
    private Vector3 headingError;
    private Vector3 headingCorrection;


    private float stopTime = 1f;
    private float distanceX = 0f;
    private float distanceY = 0f;
    private float velocity = 1f;
    private Vector2 position = Vector2.zero;
    public Transform target;

    public void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        if (target != null)
        {
            angularVel = rigidBody.angularVelocity * -1;
            angularVelocityError = rigidBody.angularVelocity * -1;
            Debug.DrawRay(transform.position, rigidBody.angularVelocity * 10, Color.black);

            angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, Time.deltaTime);
            Debug.DrawRay(transform.position, angularVelocityCorrection, Color.green);

            rigidBody.AddTorque(angularVelocityCorrection);

            desiredHeading = target.position - transform.position;
            Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

            currentHeading = transform.up;
            Debug.DrawRay(transform.position, currentHeading * 15, Color.blue);

            headingError = Vector3.Cross(currentHeading, desiredHeading);
            headingCorrection = headingController.Update(headingError, Time.deltaTime);

            rigidBody.AddTorque(headingCorrection);
        }

    }
    public Vector3 angularVel;
    public bool canMove = false;
    IEnumerator StopMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(Time.deltaTime * time);
        canMove = true;
    }
}