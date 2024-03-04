using System;
using System.Collections.Generic;
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

        public virtual void OnStateLateUpdate<T>(StatefulObject<T> self) where T : IState
        {

        }

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
            private string[] footsteps = new string[] { "Footstep", "Footstep2", "Footstep3", "Footstep4" };
            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);

                if (!AudioManager.Instance.IsPlaying("Footstep") && !AudioManager.Instance.IsPlaying("Footstep2") && !AudioManager.Instance.IsPlaying("Footstep3") && !AudioManager.Instance.IsPlaying("Footstep4"))
                {
                    AudioManager.Instance.Play(footsteps[UnityEngine.Random.Range(0, footsteps.Length)]);
                }

                if (ArmController.Instance.anim != null)
                ArmController.Instance.anim.SetBool("Walking", true);

                TimerManager.Instance.CreateTimer(controller.footstepInterval, () => AudioManager.Instance.Play(footsteps[UnityEngine.Random.Range(0, footsteps.Length)]), out timer, true);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                TimerManager.Instance.Stop(timer);
                if (ArmController.Instance.anim != null)
                    ArmController.Instance.anim.SetBool("Walking", false);
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
                if (timer != null)
                    timer.amountOfTime = controller.sprinting ? controller.sprintStepInterval : controller.footstepInterval;

                lastSprinting = controller.sprinting;
                if (controller.sprinting && controller.sprintUsesStamina)
                {
                    controller.ChangeStamina(controller.currentStamina - (controller.staminaReductionRate * Time.deltaTime));
                    //controller.currentStamina -= controller.staminaReductionRate * Time.deltaTime;
                    //controller.staminaSlider.value = controller.currentStamina;
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

                if (controller.sprinting && controller.shouldCrouch && !controller.onSlope)
                {
                    controller.ChangeState(CharacterStates.Slide);
                }

                if (controller.shouldCrouch && !controller.sprinting)
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

                if (controller.shouldCrouch)
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
                //Wall Running
                if ((controller.wallLeft || controller.wallRight) && controller.movementDir.z > 0 && controller.canWallRun)
                {
                    controller.ChangeState(CharacterStates.WallRun);
                }


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

                rb.AddForce((Vector3.up ) * controller.baseJumpForce, ForceMode.Impulse);
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
                    if (controller.shouldCrouch)
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
                Debug.DrawRay(startPos, Vector3.up * controller.originalHeight, Color.green, 99f);
                return !Physics.Raycast(startPos, Vector3.up, controller.originalHeight, ~LayerMask.NameToLayer("Player"));
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
                controller.isCrouching = false;
                controller.modelCollider.TweenHeight(originalColHeight, controller.toCrouchSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
                controller.camHolder.transform.TweenYPos(controller.originalHeight, controller.toCrouchSpeed, null, null, EasingFunctions.Ease.EaseOutQuart);
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
                if (!controller.isCrouching && controller.shouldCrouch)
                {
                    controller.modelCollider.TweenHeight(controller.crouchHeight, controller.toCrouchSpeed, () => { rb.AddForce(Vector3.down * 100f, ForceMode.Impulse); }, EasingFunctions.Ease.EaseOutQuart);
                    controller.camHolder.transform.TweenYPos(controller.crouchHeight, controller.toCrouchSpeed, null, () => { rb.AddForce(Vector3.down * 500f * Time.deltaTime, ForceMode.Force); }, EasingFunctions.Ease.EaseOutQuart);
                    controller.isCrouching = true;
                }

                if (controller.isCrouching && !controller.shouldCrouch)
                {
                    if (CanStand())
                    {
                        controller.modelCollider.TweenHeight(originalColHeight, controller.toCrouchSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
                        controller.camHolder.transform.TweenYPos(controller.originalHeight, controller.toCrouchSpeed, null, null, EasingFunctions.Ease.EaseOutQuart);
                        controller.isCrouching = false;
                        controller.ChangeState(CharacterStates.Idle);
                    }
                    else
                    {
                        controller.isCrouching = true;
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
                AudioManager.Instance.Play("PlayerSlide");
                controller.modelCollider.TweenHeight(controller.crouchHeight, controller.slideTransitionSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
                controller.camHolder.transform.TweenYPos(controller.crouchHeight, controller.slideTransitionSpeed, () => { }, () => { rb.AddForce(Vector3.down * 1000f * Time.deltaTime, ForceMode.Force); }, EasingFunctions.Ease.EaseOutQuart);
                //controller.mainCam.transform.TweenPosition(new Vector3(controller.mainCam.transform.position.x, controller.crouchHeight, controller.mainCam.transform.position.z), controller.toCrouchSpeed, () => { }, EasingFunctions.Ease.EaseOutQuart);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                AudioManager.Instance.Stop("PlayerSlide");
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
            private float reduction;
            private Timer timer;
            private float originalFOV;
            private float originalZTilt;

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);

                controller.onAirJump.AddListener(WallJump);
                originalFOV = controller.vCam.m_Lens.FieldOfView;
                originalZTilt = controller.vCam.m_Lens.Dutch;


                controller.wallRunning = true;
                controller.canWallRun = false;
                TimerManager.Instance.CreateTimer(controller.maxWallRunTime, () => controller.ChangeState(CharacterStates.Falling), out timer);

                controller.camHolder.GetComponent<CameraEffects>().AdjustFOV(controller.wallRunFOV);
                if (controller.wallLeft) controller.camHolder.GetComponent<CameraEffects>().AdjustTilt(-controller.wallRunCameraTilt);
                else controller.camHolder.GetComponent<CameraEffects>().AdjustTilt(controller.wallRunCameraTilt);
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                controller.wallRunning = false;
                controller.onAirJump.RemoveListener(WallJump);
                TimerManager.Instance.Stop(timer);
                controller.camHolder.GetComponent<CameraEffects>().AdjustFOV(originalFOV);
                controller.camHolder.GetComponent<CameraEffects>().AdjustTilt(originalZTilt);
            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                //The force along the wall
                rb.AddForce(wallForward * controller.wallRunSpeed, ForceMode.Force);

                //Calculate direction toward wall and apply small force to keep attached
                if (!(controller.wallLeft && controller.movementDir.x > 0) && !(controller.wallRight && controller.movementDir.x < 0))
                    rb.AddForce(-wallNormal * 100f, ForceMode.Force);

                //Downward force while running
                rb.AddForce(Vector3.down * controller.wallFallForce, ForceMode.Force);

                //Checks for a movmement based escape from the state
                MovementCancelCheck();
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {
                wallNormal = controller.wallRight ? controller.rightWallHit.normal : controller.leftWallHit.normal;
                wallForward = -Vector3.Cross(wallNormal, controller.transform.up);

                //Invert wallForward depending on what direction we approach the wall
                if ((controller.transform.forward - wallForward).magnitude > (controller.transform.forward - -wallForward).magnitude)
                    wallForward = -wallForward;

                //Caps movement velocity on the wall
                if (rb.velocity.magnitude > 5f)
                {
                    rb.velocity = rb.velocity.normalized * 5f;
                }

                CancelCheck();
            }

            private void MovementCancelCheck()
            {
                if ((controller.wallLeft && controller.movementDir.x > 0) || (controller.wallRight && controller.movementDir.x < 0))
                {
                    rb.AddRelativeForce(controller.movementDir * 100f, ForceMode.Force);
                    controller.ChangeState(CharacterStates.Falling);
                }
            }

            private void WallJump()
            {
                if ((timer.amountOfTime - timer.timeRemaining) > controller.minTimeToWallJump)
                {
                    Vector3 forceToApply = controller.transform.up * controller.wallJumpForce + wallNormal * controller.wallJumpSideForce;
                    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.y);
                    rb.AddForce(forceToApply, ForceMode.Impulse);
                    controller.ChangeState(CharacterStates.Falling);
                }
            }

            private void CancelCheck()
            {
                if (controller.movementDir.z <= 0 || (!controller.wallLeft && !controller.wallRight))
                {
                    controller.ChangeState(CharacterStates.Falling);
                }
            }
        }

        public class GrappleState : CharacterState
        {
            /* TODO
             * Point specific grapple
             * Special FOV and woosh effects
             * tune speed
             * cooldown
             * make sure you can only use it again after landing
             * adjust arm point slightly
             * Locking camera rot might help the feel
             */
            private Vector3 grapplePoint;
            private Spring spring;
            private Vector3 currentGrapplePosition;
            private bool shouldMove = false;
            private bool doFalling = false;

            public override void OnStateInit<T>(StatefulObject<T> self)
            {
                base.OnStateInit(self);

                controller.onGrounded.AddListener(Test);
            }

            private void Test()
            {
                controller.canGrapple = true;
                Debug.Log("Reset");
            }

            public override void OnStateEnter<T>(StatefulObject<T> self)
            {
                base.OnStateEnter(self);

                rb.velocity = Vector3.zero;
                controller.grappling = true;
                spring = new Spring();

                if (!controller.isGrounded)
                {
                    doFalling = true;
                }

                RaycastHit hit;
                if (Physics.Raycast(controller.mainCam.transform.position, controller.mainCam.transform.forward, out hit, controller.maxGrappleDistance, controller.grappleableLayers))
                {
                    grapplePoint = hit.point;
                }
                else
                {
                    grapplePoint = controller.mainCam.transform.position + controller.mainCam.transform.forward * controller.maxGrappleDistance;
                }
            }

            public override void OnStateUpdate<T>(StatefulObject<T> self)
            {

            }

            public override void OnStateFixedUpdate<T>(StatefulObject<T> self)
            {
                if (shouldMove)
                {
                    if (rb.position != grapplePoint)
                    {
                        Vector3 newPos = Vector3.MoveTowards(controller.rb.position, grapplePoint, 20f * Time.deltaTime);
                        rb.MovePosition(newPos);
                    }
                    else
                    {
                        StopGrapple();
                    }
                }
                else if (doFalling)
                {
                    rb.AddForce(Vector3.down * 20f, ForceMode.Force);
                }
            }

            public override void OnStateLateUpdate<T>(StatefulObject<T> self)
            {
                base.OnStateLateUpdate(self);

                DrawRope();
            }

            public override void OnStateExit<T>(StatefulObject<T> self)
            {
                shouldMove = false;
                controller.grappling = false;
                currentGrapplePosition = controller.barrelPoint.position;
                spring.Reset();
                controller.cableRenderer.positionCount = 0;
            }

            private void StopGrapple()
            {
                shouldMove = false;
                controller.grappling = false;
                controller.ChangeState(CharacterStates.Falling);
            }

            private void DrawRope()
            {
                if (!controller.grappling)
                {
                    currentGrapplePosition = controller.barrelPoint.position;
                    spring.Reset();
                    controller.cableRenderer.positionCount = 0;
                    return;
                }

                if (controller.cableRenderer.positionCount == 0)
                {
                    spring.SetVelocity(controller.velocity);
                    controller.cableRenderer.positionCount = controller.ropeQuality;
                }

                spring.SetDamper(controller.ropeDamper);
                spring.SetStrength(controller.strength);
                spring.Update(Time.deltaTime);

                var up = Quaternion.LookRotation((grapplePoint - controller.barrelPoint.position).normalized) * Vector3.up;

                currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

                for (int i = 0; i < controller.ropeQuality; i++)
                {
                    var delta = i / (float)controller.ropeQuality;
                    var offset = up * 
                        controller.waveHeight * 
                        Mathf.Sin(delta * controller.waveCount * Mathf.PI) * 
                        spring.Value * 
                        controller.effectCurve.Evaluate(delta);
                    controller.cableRenderer.SetPosition(i, Vector3.Lerp(controller.barrelPoint.position, currentGrapplePosition, delta) + offset);
                }

                if (Vector3.Distance(currentGrapplePosition, grapplePoint) <= 0.1f)
                {
                    shouldMove = true;
                }
            }
        }

    }
}

