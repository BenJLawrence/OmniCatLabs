using UnityEngine;

namespace OmnicatLabs.Input {
  
    public interface IState {
        void Initialize();
        void Enter();
        void Update();
        void Exit();
    }

    public abstract class PlayerControllerBase : MonoBehaviour {
        public abstract IState[] States { get; }

        protected virtual void Start() {
            StateRegistry.Register(gameObject, States);
        }
    }

    public class PlayerController : PlayerControllerBase {
        public override IState[] States => new IState[] { new MoveState(), };

        protected override void Start() {
            base.Start();


        }
    }
}
