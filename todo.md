# Important Things
- Custom Allocator
  - Maximum number of simultaneous memory allocations can be very low
  - Custom allocator to split up a single allocation for many objects
  - From [Staging buffer - Vulkan Tutorial](https://vulkan-tutorial.com/en/Vertex_buffers/Staging_buffer)
- Dedicated Transfer Queue
  - Queue specifically for transferring
  - From [Staging buffer - Vulkan Tutorial](https://vulkan-tutorial.com/en/Vertex_buffers/Staging_buffer)