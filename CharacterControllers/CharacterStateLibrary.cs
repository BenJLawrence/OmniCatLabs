using System;
using System.Collections;
using UnityEngine;
using OmnicatLabs.StatefulObject;
using OmnicatLabs.Tween;

namespace OmnicatLabs.CharacterControllers
{
    public abstract class CharacterState : IState
    {
        protected CharacterController controller;
        protected Rigidbody rb;
        protected AnimationTriggers triggers;

        public virtual void OnStateStart<T>(StatefulObject<T> self) where T : IState
        {
            controller = self.GetComponent<CharacterController>();
            rb = controller.GetComponent<Rigidbody>();
        }
        public virtual void OnStateEnter<T>(StatefulObject<T> self) where T : IState
        {

        }
        public abstract void OnStateUpdate<T>(StatefulObject<T> self) where T : IState;
        public abstract void OnStateExit<T>(StatefulObject<T> self) where T : IState;
        public abstract void OnStateFixedUpdate<T>(StatefulObject<T> self) where T : IState;

        public CharacterState(AnimationTriggers _triggers)
        {
            triggers = _triggers;
        }
        public CharacterState() { }
    }

    public class CharacterStateLibrary
    {
        public class MoveState : CharacterState
        {
            public override void OnStateStart<T>(StatefulObject<T> self)
            {
                base.OnStateStart(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                float targetSpeed = controller.sprinting ? controller.moveSpeed * controller.sprintMultiplier : controller.moveSpeed;
                if (!controller.onSlope && controller.groundAngle == 0)
                {
                    rb.AddRelativeForce(controller.movementDir * targetSpeed * Time.deltaTime, ForceMode.Impulse);
                }
                else if (controller.onSlope)
                {
                    //rb.velocity = GetSlopeMoveDir() * targetSpeed * Time.deltaTime;

                    if (controller.maintainVelocity)
                    {
                        rb.velocity = GetSlopeMoveDir() * targetSpeed * Time.deltaTime;
                    }
                    else
                    {
                        //Multiply the normal speed by the cosine of the angle between the slope surface and world up, in radians, to simulate the steepness of the slope
                        float angle = Vector3.Angle(controller.slopeHit.normal, Vector3.up);
                        float slopeMultiplier = Mathf.Cos(angle * Mathf.Deg2Rad);
                        float newTarget = slopeMultiplier * targetSpeed;
                        rb.velocity = GetSlopeMoveDir() * newTarget * Time.deltaTime;
                    }
                }
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {

                if (controller.movementDir.z <= 0)
                {
                    controller.sprinting = false;
                }

                if (controller.movementDir == Vector3.zero)
                {
                    controller.ChangeState(CharacterStates.Idle);
                }

                if (controller.isCrouching)
                {
                    controller.ChangeState(CharacterStates.Crouching);
                }

                if (controller.isCrouching && controller.sprinting)
                {
                    controller.ChangeState(CharacterStates.Slide);
                }

                //reset velocity every frame since we don't want to build any acceleration
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }

            private Vector3 GetSlopeMoveDir()
            {
                ////Check if facing downhill by comparing whether the dot product is positive which if true means we can invert the movement direction
                //float dotProduct = Vector3.Dot(controller.slopeHit.normal.normalized, controller.transform.forward);
                //Debug.Log("Dot:" + dotProduct);
                //var dir = Vector3.ProjectOnPlane(dotProduct > 0 ? controller.movementDir : -controller.movementDir, controller.slopeHit.normal).normalized;
                //return dir;

                Vector3 adjustedDir = controller.transform.TransformDirection(controller.movementDir);
                return Vector3.ProjectOnPlane(adjustedDir, controller.slopeHit.normal.normalized);
            }
        }

        public class IdleState : CharacterState
        {
            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                controller = self.GetComponent<CharacterController>();
                rb = controller.GetComponent<Rigidbody>();

                rb.velocity = Vector3.zero;
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                if (controller.movementDir != Vector3.zero)
                {
                    controller.ChangeState(CharacterStates.Moving);
                }

                if (controller.isCrouching)
                {
                    controller.ChangeState(CharacterStates.Crouching);
                }
            }
        }

        public class AirJumpingState : CharacterState
        {
            private float airTime = 0f;

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);
                airTime = 0f;

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce((Vector3.up * controller.multiJumpForce), ForceMode.Impulse);
                controller.currentJumpAmount++;
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                if (airTime < controller.jumpDuration && controller.jumpKeyDown && controller.extendMultiJumps)
                {
                    rb.AddForce(Vector3.up * controller.extendedMultiJumpForce * Time.deltaTime, ForceMode.Impulse);
                    airTime += Time.deltaTime;
                }
                else
                {
                    controller.ChangeState(CharacterStates.Falling);
                }
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }
        }

        public class FallingState : CharacterState
        {
            private Vector3 horizontalVelocityCheck;
            private float reduction;
            private float currentTime;
            private bool canFall = false;

            public override void OnStateStart<T>(StatefulObject<T> self)
            {
                base.OnStateStart(self);

                controller.onAirJump.AddListener(DoAirJump);
                controller.onGrounded.AddListener(HandleGrounded);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);

                canFall = false;
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                currentTime += Time.deltaTime;
                if (currentTime > controller.coyoteTime)
                {
                    //Debug.Log("Happening");
                    //handles the extra downward force when falling
                    rb.AddForce(Vector3.down * controller.fallForce * Time.deltaTime, ForceMode.Force);
                }

                if (controller.movementDir != Vector3.zero)
                {
                    rb.AddRelativeForce(controller.movementDir * controller.inAirMoveSpeed * Time.deltaTime, ForceMode.Force);
                    reduction = controller.inAirMoveSpeed;

                }
                else if (controller.instantAirStop)
                {
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                }
                else
                {
                    //slow down over by time by multiplying with small numbers
                    reduction *= controller.slowDown;
                    rb.AddRelativeForce(controller.movementDir * reduction * Time.deltaTime, ForceMode.Force);
                }
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                //Velocity cap since when adding our in air force we could theoretically ramp speed forever
                if (new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude > controller.maxInAirSpeed)
                {
                    horizontalVelocityCheck = new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized * controller.maxInAirSpeed;
                    horizontalVelocityCheck.y = rb.velocity.y;
                    rb.velocity = horizontalVelocityCheck;
                }

                if (controller.onSlope)
                {
                    controller.ChangeState(CharacterStates.Idle);
                }
            }

            protected void DoAirJump()
            {
                controller.ChangeState(CharacterStates.AirJump);
            }

            //Called when the player hits the ground
            public void HandleGrounded()
            {
                controller.currentJumpAmount = 0;
                currentTime = 0f;

                if (controller.lockOnLanding)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                }

                controller.ChangeState(CharacterStates.Idle);
            }
        }

        public class SprintState : CharacterState
        {
            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);


            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                rb.AddRelativeForce(controller.movementDir * controller.moveSpeed * controller.sprintMultiplier * Time.deltaTime, ForceMode.Impulse);
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                if (controller.movementDir == Vector3.zero)
                {
                    controller.ChangeState(CharacterStates.Idle);
                }

                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }

        public class JumpState : CharacterState
        {
            private float airTime = 0f;

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                controller = self.GetComponent<CharacterController>();
                rb = controller.GetComponent<Rigidbody>();
                airTime = 0f;

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce((Vector3.up * controller.baseJumpForce), ForceMode.Impulse);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                if (airTime < controller.jumpDuration && controller.jumpKeyDown && controller.extendJumps)
                {
                    rb.AddForce(Vector3.up * controller.extendedJumpForce * Time.deltaTime, ForceMode.Impulse);
                    airTime += Time.deltaTime;
                }
                else
                {
                    controller.ChangeState(CharacterStates.Falling);
                }
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {

            }
        }

        public class SlopeState : CharacterState
        {
            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                throw new NotImplementedException();
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                throw new NotImplementedException();
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                throw new NotImplementedException();
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                throw new NotImplementedException();
            }
        }

        public class CrouchWalkState : CharacterState
        {
            private float lerpPos;
            private float lastLerpPos;
            private bool lerpFinished = false;

            public CrouchWalkState(AnimationTriggers triggers) : base(triggers) { }

            public override void OnStateStart<T>(StatefulObject<T> self)
            {
                base.OnStateStart(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                float targetSpeed = controller.moveSpeed * controller.crouchSpeedModifier;
                if (!controller.onSlope && controller.groundAngle == 0)
                {
                    rb.AddRelativeForce(controller.movementDir * targetSpeed * Time.deltaTime, ForceMode.Impulse);
                }
                else if (controller.onSlope)
                {
                    if (controller.maintainVelocity)
                    {
                        rb.velocity = GetSlopeMoveDir() * targetSpeed * Time.deltaTime;
                    }
                    else
                    {
                        //Multiply the normal speed by the cosine of the angle between the slope surface and world up, in radians, to simulate the steepness of the slope
                        float angle = Vector3.Angle(controller.slopeHit.normal, Vector3.up);
                        float slopeMultiplier = Mathf.Cos(angle * Mathf.Deg2Rad);
                        float newTarget = slopeMultiplier * targetSpeed;
                        rb.velocity = GetSlopeMoveDir() * newTarget * Time.deltaTime;
                    }
                }
            }

            private Vector3 GetSlopeMoveDir()
            {
                ////Check if facing downhill by comparing whether the dot product is positive which if true means we can invert the movement direction
                //float dotProduct = Vector3.Dot(controller.slopeHit.normal.normalized, controller.transform.forward);
                //Debug.Log("Dot:" + dotProduct);
                //var dir = Vector3.ProjectOnPlane(dotProduct > 0 ? controller.movementDir : -controller.movementDir, controller.slopeHit.normal).normalized;
                //return dir;

                Vector3 adjustedDir = controller.transform.TransformDirection(controller.movementDir);
                return Vector3.ProjectOnPlane(adjustedDir, controller.slopeHit.normal.normalized);
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                if (controller.movementDir == Vector3.zero)
                {
                    controller.ChangeState(CharacterStates.Crouching);
                }
                if (!controller.isCrouching)
                {
                    triggers.TriggerAll(controller.animator, AnimationTriggers.TriggerFlag.Exit);
                    triggers.ResetAll(controller.animator, AnimationTriggers.TriggerFlag.Start);

                    var time = 0f;
                    time += Time.deltaTime / controller.toCrouchSpeed;

                    //controller.modelCollider.height = Mathf.Lerp(controller.modelCollider.height, originalColliderHeight, time);
                    //lerpPos = Mathf.Lerp(controller.mainCam.transform.localPosition.y, originalCamPos, time);

                    if (Mathf.Approximately(lerpPos, lastLerpPos))
                    {
                        lerpFinished = true;
                    }

                    controller.mainCam.transform.localPosition = new Vector3(controller.mainCam.transform.localPosition.x, lerpPos, controller.mainCam.transform.localPosition.z);
                    lastLerpPos = lerpPos;

                    if (lerpFinished)
                        controller.ChangeState(CharacterStates.Idle);
                }

                //reset velocity every frame since we don't want to build any acceleration
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }

        public class CrouchState : CharacterState
        {
            public CrouchState(AnimationTriggers _triggers) : base(_triggers) { }

            private float currentCrouchTime;
            private float lastLerpPos;
            private float lerpPos;
            private bool lerpFinished = false;
            private bool triggered = false;
            private float originalCamHeight;
            private float originalColHeight;
            private bool inCrouch = false;

            /*TODO
             * Shrink collider with crouch
             * Transition to crouch move state
             */

            public override void OnStateStart<T>(StatefulObject<T> self)
            {
                base.OnStateStart(self);
                originalCamHeight = controller.mainCam.transform.position.y;
                originalColHeight = controller.modelCollider.height;
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);

                currentCrouchTime = controller.toCrouchSpeed;
                lerpFinished = false;
                triggered = false;
                triggers.TriggerAll(controller.animator, AnimationTriggers.TriggerFlag.Start);
                //originalCamPos = controller.mainCam.transform.localPosition;
                //originalColliderHeight = controller.modelCollider.height;
                
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                if (!inCrouch && controller.isCrouching)
                {
                    controller.modelCollider.TweenHeight(controller.crouchHeight, controller.toCrouchSpeed, () => Debug.Log("Completed Collider"), EasingFunctions.Ease.EaseOutQuart);
                    controller.mainCam.transform.TweenPosition(new Vector3(controller.mainCam.transform.position.x, controller.crouchHeight, controller.mainCam.transform.position.z), controller.toCrouchSpeed, () => Debug.Log("Completed"), EasingFunctions.Ease.EaseOutQuart);
                    inCrouch = true;
                }

                if (inCrouch && !controller.isCrouching)
                {
                    controller.modelCollider.TweenHeight(originalColHeight, controller.toCrouchSpeed, () => Debug.Log("Completed Collider"), EasingFunctions.Ease.EaseOutQuart);
                    controller.mainCam.transform.TweenPosition(
                        new Vector3(controller.mainCam.transform.position.x, originalCamHeight, controller.mainCam.transform.position.z),
                        controller.toCrouchSpeed,
                        () => Debug.Log("Completed"), EasingFunctions.Ease.EaseOutQuart
                        );
                    inCrouch = false;
                }

                //if (controller.isCrouching)
                //{
                //    triggers.TriggerAll(controller.animator, AnimationTriggers.TriggerFlag.Start);
                //    triggers.ResetAll(controller.animator, AnimationTriggers.TriggerFlag.Exit);

                //    //var time = 0f;
                //    //time += Time.deltaTime / controller.toCrouchSpeed;

                //    //var camPos = Mathf.Lerp(controller.mainCam.transform.localPosition.y, controller.crouchHeight, time);
                //    //controller.modelCollider.height = Mathf.Lerp(controller.modelCollider.height, controller.crouchHeight, time);
                //    //controller.mainCam.transform.localPosition = new Vector3(controller.mainCam.transform.localPosition.x, camPos, controller.mainCam.transform.localPosition.z);

                //}
                //else
                //{
                //    triggers.TriggerAll(controller.animator, AnimationTriggers.TriggerFlag.Exit);
                //    triggers.ResetAll(controller.animator, AnimationTriggers.TriggerFlag.Start);

                //    var time = 0f;
                //    time += Time.deltaTime / controller.toCrouchSpeed;

                //    controller.modelCollider.height = Mathf.Lerp(controller.modelCollider.height, originalColliderHeight, time);
                //    lerpPos = Mathf.Lerp(controller.mainCam.transform.localPosition.y, originalHeight, time);

                //    if (Mathf.Approximately(lerpPos, lastLerpPos))
                //    {
                //        lerpFinished = true;
                //    }

                //    controller.mainCam.transform.localPosition = new Vector3(controller.mainCam.transform.localPosition.x, lerpPos, controller.mainCam.transform.localPosition.z);
                //    lastLerpPos = lerpPos;
                //}


                //if (!controller.isCrouching && lerpFinished)
                //{
                //    controller.ChangeState(CharacterStates.Idle);
                //}

                //if (controller.isCrouching && controller.movementDir != Vector3.zero && lerpFinished)
                //{
                //    controller.ChangeState(CharacterStates.CrouchWalk);
                //}
            }
        }

        public class SlideState : CharacterState
        {
            public override void OnStateStart<T>(StatefulObject<T> self)
            {
                base.OnStateStart(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);

                controller.modelCollider.TweenHeight(controller.crouchHeight, controller.toCrouchSpeed, () => Debug.Log("Completed Collider"), EasingFunctions.Ease.EaseOutQuart);
                rb.AddForce(controller.transform.forward * controller.slideSpeed, ForceMode.Impulse);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
               
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                
            }
        }
    }
}

