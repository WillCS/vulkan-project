namespace Project {
    public interface IState {
        IInputHandler InputHandler {
            get;
        }

        void Update();
    }

    public class TestState : IState {
        private IInputHandler inputHandler;

        public IInputHandler InputHandler {
            get => this.inputHandler;
        }

        public TestState() {
            this.inputHandler = new TestInputHandler();
        }

        public void Update() {
            
        }
    }
}