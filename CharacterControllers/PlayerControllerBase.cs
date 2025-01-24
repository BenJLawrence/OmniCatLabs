using UnityEngine;
using System.Linq;

namespace OmnicatLabs.Input {
  
    public abstract class IState {
        protected GameObject controllerObject;
        protected PlayerControllerBase controller;
        protected virtual void Initialize() {
            controllerObject = StateRegistry.AcquireController(this);
            controller = controllerObject.GetComponent<PlayerControllerBase>();
        }
        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract void FixedUpdate();
    }

    public abstract class PlayerControllerBase : MonoBehaviour {
        public abstract IState[] States { get; }
        public IState currentState {
            get; private set;
        }

        protected virtual void Start() {
            currentState = States[0];
            StateRegistry.Register(gameObject, States);
        }

        public T GetState<T>() where T: IState => States.OfType<T>().FirstOrDefault();

        public void ChangeState<T>() where T: IState {
            currentState.Exit();
            currentState = GetState<T>();

            //LMAO why did I make this
            //currentState = StateRegistry.AcquireSpecific<T>(gameObject).Unwrap();

            currentState.Enter();
        }

        public virtual void Update() {
            currentState.Update();
        }

        public virtual void FixedUpdate() {
            currentState.FixedUpdate();
        }
    }

    //public class PlayerController : PlayerControllerBase {
    //    public override IState[] States => new IState[] { new MoveState(), };

    //    protected override void Start() {
    //        base.Start();
    //        Debug.Log(GetState<MoveState>().speed);
    //    }
    //}
}
