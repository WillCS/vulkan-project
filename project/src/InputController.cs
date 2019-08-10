namespace Project {
    public delegate void InputEvent(IInput input);
    
    public class InputController {

        private Window window;

        private KeyboardController keyboard;

        public InputController(Window window) {
            this.keyboard = new KeyboardController(window);

            this.keyboard.KeyboardInputEvent += this.processInputEvent;

            this.window = window;
        }

        private void processInputEvent(IInput input) {
            this.window.Program.State.InputHandler.HandleInput(input);
        }
    }

    public interface IInput {

    }

    public enum InputTypes {
        Continuous,
        Toggle,
        OnPress,
        OnRelease
    }
}