using UnityEngine;
using System.Linq;
using Unity.Netcode;

namespace OmnicatLabs.Input {
  
    public abstract class IState {
        public IState(PlayerControllerBase _controller) {
            //controllerObject = controller.gameObject;
            controller = _controller;
        }

        protected GameObject controllerObject;
        protected PlayerControllerBase controller;
        public abstract void Initialize();
        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract void FixedUpdate();
    }

    public abstract class PlayerControllerBase : NetworkBehaviour {
        public abstract IState[] States { get; }
        public IState currentState {
            get; private set;
        }

        protected virtual void Start() {
            currentState = States[0];
            //StateRegistry.Register(gameObject, States);
            foreach (var state in States)
            {
                state.Initialize();
            }
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
