using System;
using System.Collections.Generic;

namespace Project {
    public interface IInputHandler {
        void HandleInput(IInput input);
    }

    public class TestInputHandler : IInputHandler {
        private Dictionary<IInput, Bindings> bindings;

        public TestInputHandler() {
            this.bindings = new Dictionary<IInput, Bindings>();

        }

        public void HandleInput(IInput input) {
            Console.WriteLine(input.GetType());
        }
    }

    public enum Bindings {
        Forwards,
        Backwards,
        Left,
        Right
    }
}