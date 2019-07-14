using System.Collections.Generic;
using Vk = Vulkan;

namespace Game.Vulkan {
    public class InstanceBuilder {
        private Vk.ApplicationInfo appInfo;
        private Vk.InstanceCreateInfo instanceInfo;
        private List<string> extensions;
        private List<string> validationLayers;
        public InstanceBuilder() {
            this.appInfo = new Vk.ApplicationInfo();
            this.instanceInfo = new Vk.InstanceCreateInfo();
            this.extensions = new List<string>();
            this.validationLayers = new List<string>();
        }

        public void SetApplicationName(string name) {
            this.appInfo.ApplicationName = name;
        }

        public void SetEngineName(string name) {
            this.appInfo.EngineName = name;
        }

        public void SetApplicationVersion(uint version) {
            this.appInfo.ApplicationVersion = version;
        }

        public void SetEngineVersion(uint version) {
            this.appInfo.EngineVersion = version;
        }

        public void SetApiVersion(uint version) {
            this.appInfo.ApiVersion = version;
        }

        public void EnableExtension(string name) {
            this.extensions.Add(name);
        }

        public void EnableExtensions(IEnumerable<string> names) {
            this.extensions.AddRange(names);
        }

        public void EnableValidationLayer(string name) {
            this.validationLayers.Add(name);
        }

        public void EnableValidationLayers(IEnumerable<string> names) {
            this.validationLayers.AddRange(names);
        }

        public Vk.Instance Create() {
            this.instanceInfo.ApplicationInfo = this.appInfo;

            this.instanceInfo.EnabledExtensionNames = this.extensions.ToArray();
            this.instanceInfo.EnabledExtensionCount = (uint) this.extensions.Count;

            if(this.validationLayers.Count != 0) {
                this.instanceInfo.EnabledLayerNames = this.validationLayers.ToArray();
                this.instanceInfo.EnabledLayerCount = (uint) this.validationLayers.Count;
            }
            
            return new Vk.Instance(this.instanceInfo);
        }
    }
}