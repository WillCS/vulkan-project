using System;

namespace Project {
    public delegate void KeyEvent(GLFW.Keys key, int scanCode, GLFW.InputState state, GLFW.ModifierKeys mods);
    
    public class KeyboardController {
        public event InputEvent KeyboardInputEvent;

        public KeyboardController(Window window) {
            window.KeyEvent += this.processKeyEvent;
        }

        private void processKeyEvent(GLFW.Keys keys, int scanCode, GLFW.InputState state, GLFW.ModifierKeys mods) {
            this.KeyboardInputEvent.Invoke(new KeyboardInput(keys, state, mods));
        }
    }

    public class KeyboardInput : IInput {
        public readonly GLFW.Keys         key;
        public readonly GLFW.ModifierKeys mods;
        public readonly GLFW.InputState   state;

        public KeyboardInput(GLFW.Keys key, GLFW.InputState state, GLFW.ModifierKeys mods) {
            this.key   = key;
            this.state = state;
            this.mods  = mods;
        }
    }
}