using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Linq;
//using Cinemachine;

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

        public static void TweenLocalYRotation(this Transform transform, float toAngle, float amountOfTime, UnityAction onComplete = null, UnityAction onTick = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Transform starting = transform;
            float startingY = transform.localEulerAngles.y;

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime && !tween.completed)
                {
                    if (onTick != null)
                        onTick.Invoke();

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

        public static void TweenLocalZRotation(this Transform transform, float toAngle, float amountOfTime, UnityAction onComplete = null, UnityAction onTick = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Transform starting = transform;
            float startingZ = transform.localEulerAngles.z;

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime && !tween.completed)
                {
                    if (onTick != null)
                        onTick.Invoke();

                    transform.localRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, EasingFunctions.GetEasingFunction(easing).Invoke(startingZ, ClosestRotation(startingZ, toAngle), tween.timeElapsed / tween.tweenTime)));
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, toAngle);
                    tween.completed = true;
                }
            }));
        }

        public static void TweenFOV(this Camera camera, float newFOV, float amountOfTime, UnityAction onComplete = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            float startingFOV = camera.fieldOfView;

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, camera, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime)
                {
                    if (camera != null)
                    {
                        camera.fieldOfView = EasingFunctions.GetEasingFunction(easing).Invoke(startingFOV, newFOV, tween.timeElapsed / tween.tweenTime);
                        tween.timeElapsed += Time.deltaTime;
                    }
                }
                else
                {
                    if (camera != null)
                        camera.fieldOfView = newFOV;
                    tween.completed = true;
                }
            }));
        }

        //public static void TweenFOV(this CinemachineVirtualCamera camera, float newFOV, float amountOfTime, UnityAction onComplete = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        //{
        //    float startingFOV = camera.m_Lens.FieldOfView;

        //    OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, camera, (tween) =>
        //    {
        //        if (tween.timeElapsed < tween.tweenTime)
        //        {
        //            if (camera != null)
        //            {
        //                camera.m_Lens.FieldOfView = EasingFunctions.GetEasingFunction(easing).Invoke(startingFOV, newFOV, tween.timeElapsed / tween.tweenTime);
        //                tween.timeElapsed += Time.deltaTime;
        //            }
        //        }
        //        else
        //        {
        //            if (camera != null)
        //                camera.m_Lens.FieldOfView = newFOV;
        //            tween.completed = true;
        //        }
        //    }));
        //}

        //public static void TweenDutch(this CinemachineVirtualCamera camera, float newDutch, float amountOfTime, UnityAction onComplete = null, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        //{
        //    float startingDutch = camera.m_Lens.Dutch;

        //    OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, camera, (tween) =>
        //    {
        //        if (tween.timeElapsed < tween.tweenTime)
        //        {
        //            if (camera != null)
        //            {
        //                camera.m_Lens.Dutch = EasingFunctions.GetEasingFunction(easing).Invoke(startingDutch, newDutch, tween.timeElapsed / tween.tweenTime);
        //                tween.timeElapsed += Time.deltaTime;
        //            }
        //        }
        //        else
        //        {
        //            if (camera != null)
        //                camera.m_Lens.Dutch = newDutch;
        //            tween.completed = true;
        //        }
        //    }));
        //}

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

        public static void TweenScale(this Transform transform, Vector3 newScale, float amountOfTime, UnityAction onComplete, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            Vector3 startingScale = transform.localScale;
            float x = transform.localScale.x;
            float y = transform.localScale.y;
            float z = transform.localScale.z;

            OmniTween.tweens.Add(new Tween(amountOfTime, onComplete, transform, (tween) =>
            {
                if (tween.timeElapsed < tween.tweenTime)
                {
                    transform.localScale = new Vector3(
                        EasingFunctions.GetEasingFunction(easing).Invoke(x, newScale.x, tween.timeElapsed / tween.tweenTime),
                        EasingFunctions.GetEasingFunction(easing).Invoke(y, newScale.y, tween.timeElapsed / tween.tweenTime),
                        EasingFunctions.GetEasingFunction(easing).Invoke(z, newScale.z, tween.timeElapsed / tween.tweenTime)
                        );
                    tween.timeElapsed += Time.deltaTime;
                }
                else
                {
                    transform.localScale = newScale;
                    tween.completed = true;
                }
            }));
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
        public bool isPaused = false;

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

    public class ValueTween
    {
        public float tweenTime;
        public float timeElapsed = 0f;
        public UnityAction<Tween> tweenAction;
        public bool completed = false;
        public bool markedForRemove = false;
        public UnityAction onComplete;
        public bool isPaused = false;
    }

    public class OmniTween : MonoBehaviour
    {
        public static List<Tween> tweens = new List<Tween>();
        public static List<Vector3> test = new List<Vector3>();

        public static void TweenVector(ref Vector3 valueToChange, Vector3 start, Vector3 end, float amountOfTime, UnityAction onComplete, EasingFunctions.Ease easing = EasingFunctions.Ease.Linear)
        {
            test.Add(valueToChange);
        }

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

        public static void PauseTween<T>(T component) where T : Component
        {
            foreach (Tween tween in tweens)
            {
                if (tween.component == component)
                {
                    tween.isPaused = true;
                }
            }
        }

        public static void Resume<T>(T component) where T : Component
        {
            foreach (Tween tween in tweens)
            {
                if (tween.component == component)
                {
                    tween.isPaused = false;
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < tweens.Count; i++)
            {
                if (!tweens[i].markedForRemove && !tweens[i].completed && !tweens[i].isPaused)
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

