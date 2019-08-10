# vulkan-project
I wanted to learn how Vulkan works, so I'm playing around with it here.  
I'm not quite sure what this project will turn into, if anything, but in the process of building it I've learned a bunch of stufff, 
including:
* Interop in C# - Dealing with unmanaged code, marshalling objects to and from managed memory.
* Reflection in C# - I've had to "work around" some weird shortcomings in the Vulkan and GLFW bindings I'm using, which required
creating and populating some objects using reflection.
* Using Vulkan - It's **SO** much more complicated than OpenGL and DirectX are, but I feel like I have at least somewhat of a
better understanding of how the GPU works now? I'm sure I'll learn more as I keep going. I'd like to build a nice C# wrapper around
it.
* Using Mono, developing with C# outside of Visual Studio and on Linux - This project has been built entirely on Linux, and I 
expected that getting a .NET development environment up and running would be a colossal pain, but it's actually been really
straightforward. I have a nice little VSCode based development environment going and everything's been really easy. Mono has worked
seamlessly with the rest of the C# pipeline in VSCode.
* Unit Testing using NUnit - I'm actually writing tests!
* Linear Algebra - Playing around with transformations is helping me brush up on my linear algebra. It's been a while since I
learned and I've gotten rusty.
