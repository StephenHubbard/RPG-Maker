﻿using RPG.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Saving;
using RPG.Attributes;
using System;
using Cinemachine;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {

        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        [SerializeField] float maxNavPathLength = 40f;

        float turnSmoothVelocity;
        NavMeshAgent navMeshAgent;
        Health health;
        public Transform mainCam;
        public GameObject cinemachine;
        private CinemachineFreeLook freeLook;


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        private void Start()
        {
            if (gameObject.tag == "Player")
            {
                freeLook = cinemachine.GetComponent<CinemachineFreeLook>();
            }
        }

        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
            WasdMove();
            navigateCameraAngle();
            if (gameObject.tag == "Player")
            {
                UpdateAnimatorPlayer();
            }
            else
            {
                UpdateAnimator();
            }
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if (!hasPath) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;
            if (GetPathLength(path) > maxNavPathLength) return false;

            return true;
        }

        private void navigateCameraAngle()
        {
            if (gameObject.tag == "Player")
            {
                if (Input.GetMouseButton(1))
                {
                    freeLook.m_XAxis.m_InputAxisName = "Mouse X";
                    freeLook.m_YAxis.m_InputAxisName = "Mouse Y";
                }
                else if (!Input.GetMouseButton(1))
                {
                    freeLook.m_XAxis.m_InputAxisName = "";
                    freeLook.m_YAxis.m_InputAxisName = "";
                    freeLook.m_XAxis.m_InputAxisValue = 0;
                    freeLook.m_YAxis.m_InputAxisValue = 0;
                }
            }
        }


        public void WasdMove()
        {
            if (gameObject.tag == "Player")
            {
                float turnSmoothTime = 0.05f;
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Math.Sign(Input.GetAxis("Vertical"));
                Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

                if (direction.magnitude >= .01f)
                {
                    float targetAngle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + mainCam.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    navMeshAgent.Move(moveDirection.normalized * maxSpeed * Time.deltaTime);
                    //navMeshAgent.speed = maxSpeed;
                    //navMeshAgent.velocity = direction;
                    GetComponent<ActionScheduler>().CancelCurrentAction();
                }
            }
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = Mathf.Max(Mathf.Abs(localVelocity.z), Mathf.Abs(localVelocity.x));
            GetComponent<Animator>().SetFloat("forwardSpeed", (speed * maxSpeed));
        }

        private void UpdateAnimatorPlayer()
        {
            // very hacky way to ensure movement of some kind is being passed to the animator
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Math.Sign(Input.GetAxis("Vertical"));
            Vector3 direction = new Vector3(horizontal, 0f, vertical);

            Vector3 velocity2 = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity2);
            float speed = Mathf.Max(Mathf.Abs(localVelocity.z), Mathf.Abs(localVelocity.x));

            float velocity = Mathf.Max(Mathf.Abs(direction.z), Mathf.Abs(direction.x));

            GetComponent<Animator>().SetFloat("forwardSpeed", Mathf.Max(velocity, speed) * maxSpeed);
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            navMeshAgent.isStopped = true;
        }

        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if (path.corners.Length < 2) return total;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return total;
        }

        public object CaptureState()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["position"] = new SerializableVector3(transform.position);
            data["rotation"] = new SerializableVector3(transform.eulerAngles);
            return data;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)state;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = ((SerializableVector3)data["position"]).ToVector();
            transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
            GetComponent<NavMeshAgent>().enabled = true;

        }
    }
}
