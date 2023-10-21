using System;
using System.Collections;
using UnityEngine;
using OmnicatLabs.StatefulObject;
using OmnicatLabs.Tween;
using OmnicatLabs.Timers;
using OmnicatLabs.Audio;

namespace OmnicatLabs.CharacterControllers
{
    public abstract class CharacterState : IState
    {
        protected CharacterController controller;
        protected Rigidbody rb;
        protected AnimationTriggers triggers;
        protected static Vector3 lastMovementDir;
        protected static bool lastSprinting = false;

        public virtual void OnStateInit<T>(StatefulObject<T> self) where T : IState
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
            private Timer timer;
            private float targetSpeed;
            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                /*List<string> footsteps = new List<string>();
                footsteps.Add("Footstep");
                footsteps.Add("Footstep2");
                footsteps.Add("Footstep3");
                footsteps.Add("Footstep4");*/

                base.OnStateEnter(self);
                //AudioManager.Instance.Play("Footstep");

                //TimerManager.Instance.CreateTimer(controller.footstepInterval, () => AudioManager.Instance.Play("Footstep"), out timer, true);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                
                    //TimerManager.Instance.Stop(timer);
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                //TODO correct logic for whether on an up or down slope just need to integrate
                //var result = Vector3.Dot(controller.slopeHit.normal, controller.transform.forward);
                //if (result < 0f)
                //{
                //    Debug.Log((result, "Upslope"));
                //}
                //else if (result > 0f)
                //{
                //    Debug.Log((result, "Downslope"));
                //}

                targetSpeed = controller.sprinting && controller.currentStamina > 0f ? controller.moveSpeed * controller.sprintMultiplier : controller.moveSpeed;
                //Debug.Log(targetSpeed);
                if (!controller.onSlope)
                {
                    rb.AddRelativeForce(controller.movementDir * targetSpeed * Time.deltaTime, ForceMode.Impulse);
                }
                else if (controller.onSlope)
                {
                    targetSpeed = controller.sprinting && controller.currentStamina > 0f ? controller.slopeSpeed * controller.sprintMultiplier : controller.slopeSpeed;
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
                lastSprinting = controller.sprinting;
                if (controller.sprinting && controller.sprintUsesStamina)
                {
                    controller.currentStamina -= controller.staminaReductionRate * Time.deltaTime;
                    controller.staminaSlider.value = controller.currentStamina;
                    if (controller.currentStamina < 0f)
                    {
                        controller.currentStamina = 0f;
                        targetSpeed = controller.onSlope ? controller.slopeSpeed : controller.moveSpeed;
                    }
                }
                if (controller.movementDir != Vector3.zero)
                {
                    lastMovementDir = controller.movementDir;
                }
                if (controller.movementDir.z <= 0)
                {
                    controller.sprinting = false;
                }

                if (controller.movementDir == Vector3.zero)
                {
                    controller.ChangeState(CharacterStates.Idle);
                }

                if (controller.sprinting && controller.isCrouching && !controller.onSlope)
                {
                    controller.ChangeState(CharacterStates.Slide);
                }

                if (controller.isCrouching && !controller.sprinting)
                {
                    controller.ChangeState(CharacterStates.Crouching);
                }

                if (rb.velocity.magnitude >= 5)
                {
                    rb.velocity = rb.velocity.normalized * 5;
                }
                //reset velocity every frame since we don't want to build any acceleration
                //rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
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

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);

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

                if ((controller.wallLeft || controller.wallRight) && controller.movementDir == Vector3.forward && !controller.isGrounded)
                {
                    controller.ChangeState(CharacterStates.WallRun);
                }
            }

            protected void DoAirJump()
            {
                if (controller.extraJumpUnlocked)
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
                rb.AddForce((Vector3.up + rb.velocity.normalized) * controller.baseJumpForce , ForceMode.Impulse);
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
            public CrouchWalkState(AnimationTriggers triggers) : base(triggers) { }

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
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
                    if (controller.isCrouching)
                        controller.ChangeState(CharacterStates.Crouching);
                    else controller.ChangeState(CharacterStates.Idle);
                }
                //if (!controller.isCrouching)
                //{
                //    triggers.TriggerAll(controller.animator, AnimationTriggers.TriggerFlag.Exit);
                //    triggers.ResetAll(controller.animator, AnimationTriggers.TriggerFlag.Start);

                //    var time = 0f;
                //    time += Time.deltaTime / controller.toCrouchSpeed;

                //    //controller.modelCollider.height = Mathf.Lerp(controller.modelCollider.height, originalColliderHeight, time);
                //    //lerpPos = Mathf.Lerp(controller.mainCam.transform.localPosition.y, originalCamPos, time);

                //    if (Mathf.Approximately(lerpPos, lastLerpPos))
                //    {
                //        lerpFinished = true;
                //    }

                //    controller.mainCam.transform.localPosition = new Vector3(controller.mainCam.transform.localPosition.x, lerpPos, controller.mainCam.transform.localPosition.z);
                //    lastLerpPos = lerpPos;

                //    if (lerpFinished)
                //        controller.ChangeState(CharacterStates.Idle);
                //}

                //reset velocity every frame since we don't want to build any acceleration
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }

        public class CrouchState : CharacterState
        {
            public CrouchState(AnimationTriggers _triggers) : base(_triggers) { }

            //private float originalCamHeight;
            private float originalColHeight;
            private bool inCrouch = false;

            public bool CanStand()
            {
                Vector3 startPos = controller.transform.position + controller.modelCollider.center;
                Debug.DrawRay(startPos, Vector3.up * originalColHeight, Color.green, 99f);
                return !Physics.Raycast(startPos, Vector3.up, originalColHeight, ~LayerMask.NameToLayer("Player"));
            }

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
                //originalCamHeight = controller.mainCam.transform.position.y;
                originalColHeight = controller.modelCollider.height;
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);
                //triggers.TriggerAll(controller.animator, AnimationTriggers.TriggerFlag.Start);
                
                //if (!CanStand())
                //{
                //    inCrouch = true;
                //}
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                inCrouch = false;
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

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                float targetSpeed = controller.onSlope ? controller.slopeSpeed * controller.crouchSpeedModifier : controller.moveSpeed * controller.crouchSpeedModifier;
                if (!controller.onSlope)
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

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                if (!inCrouch && controller.isCrouching)
                {
                    controller.modelCollider.TweenHeight(controller.crouchHeight, controller.toCrouchSpeed, () => { rb.AddForce(Vector3.down * 100f, ForceMode.Impulse); }, EasingFunctions.Ease.EaseOutQuart);
                    controller.camHolder.transform.TweenYPos(controller.crouchHeight, controller.toCrouchSpeed, null, () => { rb.AddForce(Vector3.down * 500f * Time.deltaTime, ForceMode.Force); }, EasingFunctions.Ease.EaseOutQuart);
                    inCrouch = true;
                }

                if (inCrouch && !controller.isCrouching)
                {
                    if (CanStand())
                    {
                        controller.modelCollider.TweenHeight(originalColHeight, controller.toCrouchSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
                        controller.camHolder.transform.TweenYPos(controller.originalHeight, controller.toCrouchSpeed, null, null, EasingFunctions.Ease.EaseOutQuart);
                        inCrouch = false;
                        controller.ChangeState(CharacterStates.Idle);
                    }
                }

                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }

        public class SlideState : CharacterState
        {
            private float originalHeight;
            //private float originalCamPos;
            private bool sliding = false;
            private float falloff;
            private Vector3 slideDir;
            private bool shouldSlide = false;
            private bool goingToCrouch = false;

            public bool CanStand()
            {
                Vector3 startPos = controller.transform.position + controller.modelCollider.center;
                Debug.DrawRay(startPos, Vector3.up * originalHeight, Color.green, 99f);
                return !Physics.Raycast(startPos, Vector3.up, originalHeight, ~LayerMask.NameToLayer("Player"));
            }

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
                originalHeight = controller.modelCollider.height;
                //originalCamPos = controller.mainCam.transform.position.y;
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);
                falloff = controller.slideSpeed;
                slideDir = controller.transform.forward;

                controller.modelCollider.TweenHeight(controller.crouchHeight, controller.slideTransitionSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
                controller.camHolder.transform.TweenYPos(controller.crouchHeight, controller.slideTransitionSpeed, () => { }, () => { rb.AddForce(Vector3.down * 1000f * Time.deltaTime, ForceMode.Force); }, EasingFunctions.Ease.EaseOutQuart);
                //controller.mainCam.transform.TweenPosition(new Vector3(controller.mainCam.transform.position.x, controller.crouchHeight, controller.mainCam.transform.position.z), controller.toCrouchSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                //Debug.Log("Called");
                if (!goingToCrouch)
                {
                    controller.modelCollider.TweenHeight(originalHeight, controller.slideTransitionSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
                    controller.camHolder.transform.TweenYPos(controller.originalHeight, controller.slideTransitionSpeed, () => { }, null, EasingFunctions.Ease.EaseOutQuart);
                }

                //controller.isCrouching = false;
                //controller.slideKeyDown = false;
                //controller.mainCam.transform.TweenPosition(new Vector3(controller.mainCam.transform.position.x, originalCamPos, controller.mainCam.transform.position.z), controller.toCrouchSpeed, () => Debug.Log("Completed"), EasingFunctions.Ease.EaseOutQuart);
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                if (shouldSlide)
                {
                    sliding = true;
                    rb.AddForce(slideDir * controller.slideSpeed * falloff * Time.deltaTime);

                    falloff *= controller.slideSpeedReduction;

                    if (controller.slideUsesStamina)
                    {
                        controller.currentStamina -= controller.staminaReductionRate * Time.deltaTime;
                        controller.staminaSlider.value = controller.currentStamina;
                        if (controller.currentStamina < 0f)
                        {
                            controller.currentStamina = 0f;
                        }
                    }
                }
                else
                {
                    sliding = false;
                }
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                shouldSlide = controller.slideKeyDown && falloff > controller.slideStopThreshold && !controller.onSlope && controller.currentStamina > 0f;

                bool canStand = CanStand();

                if (!sliding && !shouldSlide)
                {
                    //controller.movementDir = lastMovementDir;
                    if (canStand)
                    {
                        goingToCrouch = false;
                        controller.ChangeState(CharacterStates.Idle);
                    }
                    else
                    {
                        goingToCrouch = true;
                        controller.ChangeState(CharacterStates.Crouching);
                    }

                }
                if (rb.velocity.magnitude > 5)
                {
                    rb.velocity = rb.velocity.normalized * 5;
                }
            }
        }

        public class WallRunState : CharacterState
        {
            private Vector3 wallNormal;
            private Vector3 wallForward;

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);
                wallNormal = controller.wallRight ? controller.rightWallHit.normal : controller.leftWallHit.normal;
                wallForward = -Vector3.Cross(wallNormal, controller.transform.up);
                rb.useGravity = false;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                controller.wallRunning = true;
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                //rb.useGravity = true;
                controller.wallRunning = false;
                Debug.Log("Exited");
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                rb.AddForce(wallForward * controller.wallRunSpeed, ForceMode.Force);
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                if (!controller.wallLeft || !controller.wallRight)
                {
                    controller.ChangeState(CharacterStates.Falling);
                }

                if (rb.velocity.magnitude > 5f)
                {
                    rb.velocity = rb.velocity.normalized * 5f;
                }
            }
        }
    }
}

