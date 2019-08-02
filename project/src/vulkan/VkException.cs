using Vk = Vulkan;

namespace Project.Vulkan {
    public class VkException : System.Exception {
        private Vk.Result result;
        public VkException(string message, Vk.Result result) : base(message) {
            this.result = result;
        }

        public VkException(string message, Vk.ResultException inner) : base(message, inner) {
            this.result = inner.Result;
        }

        protected VkException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info, context) { 
        }
    }
}