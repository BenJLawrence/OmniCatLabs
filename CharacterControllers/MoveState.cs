using UnityEngine;

namespace OmnicatLabs.Input {
    public class MoveState : IState {
        public float speed;

        public MoveState(PlayerControllerBase _controller) : base(_controller) {
        }

        public override void Enter() {
            
        }

        public override void Exit() {
            throw new System.NotImplementedException();
        }

        public override void Initialize() {
            
        }

        public override void Update() {
            throw new System.NotImplementedException();
        }

        public override void FixedUpdate() {
            throw new System.NotImplementedException();
        }
    }
}

