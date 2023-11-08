using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Linq;

namespace OmnicatLabs.Tween
{
    public static class TransformExtensions
    {
        private static float ClosestRotation(float from, float to)
        {
            float minusWhole = 0 - (360 - to);
            float plusWhole = 360 + to;
            float toDiffAbs = Mathf.Abs(to - from);
            float minusDiff = Mathf.Abs(minusWhole - from);
            float plusDiff = Mathf.Abs(plusWhole - from);
            if (toDiffAbs < minusDiff && toDiffAbs < plusDiff)
            {
                return to;
            }
            else
            {
                if (minusDiff < plusDiff)
                {
                    return minusWhole;
                }
                else
                {
                    return plusWhole;
                }
            }
        }

        public static void RealTweenYRot(this Transform transform, float toAngle, float amountOfTime, UnityAction onComplete = null, UnityAction onTick = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Transform starting = transform;
            float startingY = transform.localEulerAngles.y;
            

            //Tween tween = OmniTween.tweens.Find(tween => tween.component == transform);
            //if (tween != null && tween.component == transform)
            //{
            //    tween.completed = true;
            //    Debug.Log(tween);
            //}

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime && !tween.completed)
                {
                    if (onTick != null)
                        onTick.Invoke();
                    //transform.localPosition = new Vector3(transform.localPosition.x, EasingFunctions.GetEasingFunction(easing).Invoke(startingY, toAngle, tween.timeElapsed / tween.tweenTime), transform.localPosition.z);

                    //rotation = 
                    //transform.localRotation = Quaternion.Slerp(starting.localRotation, Quaternion.Euler(new Vector3(transform.localEulerAngles.x, ClosestRotation(transform.localEulerAngles.y, toAngle), transform.localEulerAngles.z)), tween.timeElapsed / tween.tweenTime);

                    //float adjustedRotation = ClosestRotation(starting.localEulerAngles.y, toAngle);
                    //var newRot = EasingFunctions.GetEasingFunction(easing).Invoke(starting.localEulerAngles.y, adjustedRotation, tween.timeElapsed / tween.tweenTime);

                    //var rotVec = new Vector3(transform.localEulerAngles.x, newRot, transform.localEulerAngles.z);

                    //transform.localRotation = Quaternion.Euler(rotVec); //Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotVec), tween.timeElapsed / tween.tweenTime);
                    transform.localRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, EasingFunctions.GetEasingFunction(easing).Invoke(startingY, ClosestRotation(startingY, toAngle), tween.timeElapsed / tween.tweenTime), transform.localEulerAngles.z));
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, toAngle, transform.localEulerAngles.z);
                    tween.completed = true;
                }
            }));
        }

        public static void TweenYRot(this Transform transform, float speed, float amountOfTime, UnityAction onComplete = null, UnityAction onTick = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Transform starting = transform;
            //Tween tween = OmniTween.tweens.Find(tween => tween.component == transform);
            //if (tween != null && tween.component == transform)
            //{
            //    tween.completed = true;
            //    Debug.Log(tween);
            //}

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {

                if (tween.timeElapsed < tween.tweenTime && !tween.completed)
                {
                    if (onTick != null)
                        onTick.Invoke();
                    transform.Rotate(speed * Vector3.up * Time.deltaTime);
                    //transform.position = new Vector3(transform.position.x, EasingFunctions.GetEasingFunction(easing).Invoke(startingY, newY, tween.timeElapsed / tween.tweenTime), transform.position.z);
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    //transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                    tween.completed = true;
                }
            }));
        }

        public static void TweenZRot(this Transform transform, float speed, float amountOfTime, UnityAction onComplete = null, UnityAction onTick = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Transform starting = transform;
            //Tween tween = OmniTween.tweens.Find(tween => tween.component == transform);
            //if (tween != null && tween.component == transform)
            //{
            //    tween.completed = true;
            //    Debug.Log(tween);
            //}

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime && !tween.completed)
                {
                    if (onTick != null)
                        onTick.Invoke();
                    transform.Rotate(speed * Vector3.forward * Time.deltaTime);
                    //transform.position = new Vector3(transform.position.x, EasingFunctions.GetEasingFunction(easing).Invoke(startingY, newY, tween.timeElapsed / tween.tweenTime), transform.position.z);
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    //transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                    tween.completed = true;
                }
            }));
        }

        public static void FadeIn(this CanvasGroup cg, float amountOfTime, UnityAction onComplete = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, cg, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime)
                {
                    if (cg != null)
                    {
                        cg.alpha = EasingFunctions.GetEasingFunction(easing).Invoke(0f, 1f, tween.timeElapsed / tween.tweenTime);
                        tween.timeElapsed += Time.deltaTime;
                    }
                }
                else
                {
                    if (cg != null)
                        cg.alpha = 1f;
                    tween.completed = true;
                }
            }));
        }

        public static void FadeOut(this CanvasGroup cg, float amountOfTime, UnityAction onComplete = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, cg, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime)
                {
                    cg.alpha = EasingFunctions.GetEasingFunction(easing).Invoke(1f, 0f, tween.timeElapsed / tween.tweenTime);
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    cg.alpha = 0f;
                    tween.completed = true;
                }
            }));
        }

        public static void TweenYPos(this Transform transform, float newY, float amountOfTime, UnityAction onComplete = null, UnityAction onTick = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            float startingY = transform.localPosition.y;
            //Tween tween = OmniTween.tweens.Find(tween => tween.component == transform);
            //if (tween != null && tween.component == transform)
            //{
            //    tween.completed = true;
            //    Debug.Log(tween);
            //}


            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime && !tween.completed)
                {
                    if (onTick != null)
                        onTick.Invoke();

                    transform.localPosition = new Vector3(transform.localPosition.x, EasingFunctions.GetEasingFunction(easing).Invoke(startingY, newY, tween.timeElapsed / tween.tweenTime), transform.localPosition.z);
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
                    tween.completed = true;
                }
            }));
        }

        public static void TweenPosition(this Transform transform, Vector3 newPosition, float amountOfTime, UnityAction onComplete, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Vector3 startingPos = transform.position;
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime)
                {
                    transform.position = new Vector3(
                        EasingFunctions.GetEasingFunction(easing).Invoke(x, newPosition.x, tween.timeElapsed / tween.tweenTime),
                        EasingFunctions.GetEasingFunction(easing).Invoke(y, newPosition.y, tween.timeElapsed / tween.tweenTime),
                        EasingFunctions.GetEasingFunction(easing).Invoke(z, newPosition.z, tween.timeElapsed / tween.tweenTime)
                        );
                    //transform.position = Vector3.Lerp(startingPos, newPosition, tween.timeElapsed / tween.tweenTime);
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    transform.position = newPosition;
                    tween.completed = true;
                }
            }));
            //float startingVal = 1f;
            //float endingVal = 0f;
            //float timeElapsed = 0f;
            //float tempval = 0f;
            //    tempval = Mathf.Lerp(startingVal, endingVal, timeElapsed / amountOfTime);
            //    timeElapsed += Time.deltaTime;
            //    Debug.Log(tempval);
        }
    }

    public static class CapsuleColliderExtensions
    {
        public static void TweenHeight(this CapsuleCollider col, float newHeight, float amountOfTime, UnityAction onComplete, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            float startingHeight = col.height;
            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, col, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime)
                {
                    col.height = EasingFunctions.GetEasingFunction(easing).Invoke(startingHeight, newHeight, tween.timeElapsed / tween.tweenTime);
                    //transform.position = Vector3.Lerp(startingPos, newPosition, tween.timeElapsed / tween.tweenTime);
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    col.height = newHeight;
                    tween.completed = true;
                }
            }));
        }
    }

    public class Tween
    {
        public Component component;
        public float tweenTime;
        public float timeElapsed = 0f;
        public UnityAction<Tween> tweenAction;
        public bool completed = false;
        public bool markedForRemove = false;
        public UnityAction onComplete;
        public virtual void DoTween()
        {
            tweenAction.Invoke(this);
        }

        public Tween(float _tweenTime, UnityAction _onComplete, Component _component, UnityAction<Tween> _tweenAction)
        {
            component = _component;
            tweenTime = _tweenTime;
            tweenAction = _tweenAction;
            onComplete = _onComplete;
        }
    }

    //public class ValueTween : Tween
    //{
    //    private EasingFunctions.Ease easing;
    //    private float value;
    //    private float initialValue;
    //    private float finalValue;
    //    public override void DoTween()
    //    {
    //        if (timeElapsed < tweenTime)
    //        {
    //            value = EasingFunctions.GetEasingFunction(easing).Invoke(initialValue, finalValue, timeElapsed / tweenTime);
    //            timeElapsed += Time.deltaTime;
    //        }
    //        else
    //        {
    //            value = finalValue;
    //            completed = true;
    //        }
    //    }

    //    public ValueTween(ref float _value, float _finalValue, float _tweenTime, UnityAction _onComplete, EasingFunctions.Ease _easing) : base(_tweenTime, _onComplete, (tween) => { }) 
    //    {
    //        initialValue = _value;
    //        value = _value;
    //        finalValue = _finalValue;
    //        easing = _easing;
    //    }
    //}

    public class OmniTween : MonoBehaviour
    {
        public static List<Tween> tweens = new List<Tween>();

        public static void TweenValue(ref float valueToChange, float finalValue, float amountOfTime, UnityAction onComplete, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            //float startingVal = valueToChange;


            //tweens.Add(new Tween(amountOfTime, onComplete, (tween) =>
            //{
            //    if (tween.timeElapsed < tween.tweenTime)
            //    {
            //        valueToChange = EasingFunctions.GetEasingFunction(easing).Invoke(startingVal, finalValue, tween.timeElapsed / tween.tweenTime);
            //        tween.timeElapsed += Time.deltaTime;
            //        Debug.Log(valueToChange);
            //    }
            //    else
            //    {
            //        valueToChange = finalValue;
            //        tween.completed = true;
            //    }
            //}));
            //tweens.Add(new ValueTween(ref valueToChange, finalValue, amountOfTime, onComplete, easing));
        }

        public static void CancelTween<T>(T component, bool callCompleteCallbacks = false) where T : Component
        {
            foreach (Tween tween in tweens)
            {
                if (tween.component == component)
                {
                    if (callCompleteCallbacks && tween.onComplete != null) tween.onComplete.Invoke();
                    tween.markedForRemove = true;
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < tweens.Count; i++)
            {
                if (!tweens[i].markedForRemove && !tweens[i].completed)
                    tweens[i].DoTween();

                if (tweens[i].completed && tweens[i].onComplete != null)
                {
                    tweens[i].onComplete.Invoke();
                }
            }

            tweens.RemoveAll(tween => tween.completed || tween.markedForRemove);
        }
    }
}

