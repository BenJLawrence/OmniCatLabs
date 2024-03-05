using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using OmnicatLabs.OmniEnum;

namespace OmnicatLabs.StatefulObject
{
    /// <summary>
    /// Used to specify a default state to be used upon entry into the state machine
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultStateAttribute : Attribute
    {

    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class StateAnimation : Attribute
    {
        public string animName;

        public StateAnimation(string _animName)
        {
            animName = _animName;
        }
    }

    public interface IState
    {
        /// <summary>
        /// Called one time upon the first time entering the state. Useful for setting up listeners or acquiring references.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">A reference to the controller object</param>
        public void OnStateInit<T>(StatefulObject<T> self) where T : IState;
        /// <summary>
        /// Called once upon entry every time this state is entered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">A reference to the controller object</param>
        public void OnStateEnter<T>(StatefulObject<T> self) where T : IState;
        /// <summary>
        /// Called once every frame. Identical to Unity Update.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">A reference to the controller object</param>
        public void OnStateUpdate<T>(StatefulObject<T> self) where T : IState;
        /// <summary>
        /// Called at a fixed framerate. Identical to Unity FixedUpdate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">A reference to the controller object</param>
        public void OnStateFixedUpdate<T>(StatefulObject<T> self) where T : IState;
        /// <summary>
        /// Called once every time this state is exited
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">A reference to the controller object</param>
        public void OnStateExit<T>(StatefulObject<T> self) where T : IState;
        /// <summary>
        /// Identical to Unity LateUpdate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">A reference to the controller object</param>
        public void OnStateLateUpdate<T>(StatefulObject<T> self) where T : IState;
    }

    public class AnimationTriggers
    {
        public List<string> exit;
        public List<string> start;
        public List<string> update;

        public enum TriggerFlag
        {
            Start = 0,
            Update = 1,
            Exit = 2,
        }

        public AnimationTriggers(List<string> _start = null, List<string> _update = null, List<string> _exit = null)
        {
            exit = _exit;
            start = _start;
            update = _update;
        }

        /// <summary>
        /// A group of triggers that a state can call automatically at the appropriate time.<para></para>
        /// In order to ensure your triggers are bound to the correct parameters try defining a parameter like this: (exit: "MyExitTrigger") which will bind to exit
        /// </summary>
        /// <param name="_start"></param>
        /// <param name="_update"></param>
        /// <param name="_exit"></param>
        public AnimationTriggers(string _start = null, string _update = null, string _exit = null)
        {
            start = new List<string> { _start };
            update = new List<string> { _update };
            exit = new List<string> { _exit };
        }

        /// <summary>
        /// Sets all triggers for the appropriate flags
        /// </summary>
        /// <param name="animator">Animator to trigger on</param>
        /// <param name="flags">Represents what trigger categories to set</param>
        public void TriggerAll(Animator animator, TriggerFlag flag)
        {
            switch (flag)
            {
                case TriggerFlag.Start:
                    start.ForEach(trigger => animator.SetTrigger(trigger));
                    break;
                case TriggerFlag.Update:
                    update.ForEach(trigger => animator.SetTrigger(trigger));
                    break;
                case TriggerFlag.Exit:
                    exit.ForEach(trigger => animator.SetTrigger(trigger));
                    break;
            }
        }

        /// <summary>
        /// Sets the trigger for all triggers in all categories. Only use this when you are dealing with single categories i.e: only using start while exit and update are both empty
        /// </summary>
        /// <param name="animator">Animator to trigger on</param>
        public void TriggerAll(Animator animator)
        {
            foreach (var trigger in start)
            {
                animator.SetTrigger(trigger);
            }

            foreach (var trigger in update)
            {
                animator.SetTrigger(trigger);
            }

            foreach (var trigger in exit)
            {
                animator.SetTrigger(trigger);
            }
        }

        /// <summary>
        /// Resets the trigger for all triggers in all categories. Only use this when you are dealing with single categories i.e: only using start while exit and update are both empty
        /// </summary>
        /// <param name="animator">Animator to trigger on</param>
        public void ResetAll(Animator animator)
        {
            foreach (var trigger in start)
            {
                animator.ResetTrigger(trigger);
            }

            foreach (var trigger in update)
            {
                animator.ResetTrigger(trigger);
            }

            foreach (var trigger in exit)
            {
                animator.ResetTrigger(trigger);
            }
        }

        /// <summary>
        /// Sets all triggers for the appropriate flags
        /// </summary>
        /// <param name="animator">Animator to trigger on</param>
        /// <param name="flags">Represents what trigger categories to set</param>
        public void ResetAll(Animator animator, TriggerFlag flag)
        {
            switch (flag)
            {
                case TriggerFlag.Start:
                    start.ForEach(trigger => animator.ResetTrigger(trigger));
                    break;
                case TriggerFlag.Update:
                    update.ForEach(trigger => animator.ResetTrigger(trigger));
                    break;
                case TriggerFlag.Exit:
                    exit.ForEach(trigger => animator.ResetTrigger(trigger));
                    break;
            }
        }
    }

    public class State<T> : OmniEnum<State<T>, T> where T : IState
    {
        internal bool firstTime = true;
        internal string animName;

        public static implicit operator State<T>(T data)
        {
            var x = new State<T>();
            x.data = data;

            return x;
        }

        public void OverrideState(T newData)
        {
            data = newData;
        }
    }

    public class StatefulObject<T> : MonoBehaviour where T : IState
    {
        protected State<T> state;

        internal Animator animator;
        public bool isPaused { get; protected set; } = false;

        protected void SetPause(bool value)
        {
            isPaused = value;
        }

        protected virtual void Awake()
        {
            //Getting the fields of the type that inherits from us.
            //Because each generic definition of a State is treated as a different type, there should only be one class that inherits from the State hence index 0
            var fieldList = Assembly.GetAssembly(typeof(State<T>)).GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(State<T>))).ToList()[0].GetFields();
            for (int i = 0; i < fieldList.Length; i++)
            {
                var field = (State<T>)fieldList[i].GetValue(null);
                field.data.OnStateInit(this);
            }
            var stateDefault = new List<FieldInfo>(fieldList).Find(field => Attribute.IsDefined(field, typeof(DefaultStateAttribute)));
            if (stateDefault != null)   //check to make sure the states we check had a default attribute
            {
                state = (State<T>)stateDefault.GetValue(null);
                //state.data.OnStateInit(this);
                state.data.OnStateEnter(this);
            }

            if (!GetComponentInChildren<Animator>() || !GetComponentInChildren<Animator>().runtimeAnimatorController)
            {
                //Debug.LogError("State Animations were used when there is no Animator Component on the same or children object(s) as the StatefulObject. Ensure that you have added an Animator Component and it has a valid controller assigned.");
            }
            else
            {
                animator = GetComponentInChildren<Animator>();
            }

            foreach (var field in fieldList)
            {
                if (Attribute.IsDefined(field, typeof(StateAnimation)))
                {
                    var animName = field.GetCustomAttribute(typeof(StateAnimation));
                    if (animName is StateAnimation anim)
                    {
                        var s = (State<T>)state.Find(s => s.name == field.Name);
                        s.animName = anim.animName;
                    }
                }

            }
        }

        protected virtual void Update()
        {
            if (!isPaused)
                state.data.OnStateUpdate(this);
        }

        protected virtual void FixedUpdate()
        {
            if (!isPaused)
                state.data.OnStateFixedUpdate(this);
        }

        protected virtual void LateUpdate()
        {
            if (!isPaused)
                state.data.OnStateLateUpdate(this);
        }

        public void ChangeState(State<T> newState)
        {
            //call the exit on the current state
            state.data.OnStateExit(this);

            //change the state
            state = newState;

            if (state.firstTime)
            {
                //state.data.OnStateInit(this);
                state.firstTime = false;
            }


            //call enter on the new state in IStates
            newState.data.OnStateEnter(this);
            if (newState.animName != null)
                animator.Play(newState.animName);
        }
    }
}