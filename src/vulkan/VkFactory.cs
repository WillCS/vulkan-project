using System;
using System.Collections.Generic;
using Vk = Vulkan;

namespace Project.Vulkan {
    public class InstanceBuilder {

        private Vk.ApplicationInfo appInfo;
        private Vk.InstanceCreateInfo instanceInfo;
        private List<string> extensions;
        private List<string> validationLayers;

        public InstanceBuilder() {
            this.appInfo = new Vk.ApplicationInfo();
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
            instanceInfo = new Vk.InstanceCreateInfo();
            
            instanceInfo.ApplicationInfo = this.appInfo;

            instanceInfo.EnabledExtensionNames = this.extensions.ToArray();
            instanceInfo.EnabledExtensionCount = (uint) this.extensions.Count;

            if(validationLayers.Count != 0) {
                instanceInfo.EnabledLayerNames = this.validationLayers.ToArray();
            }

            instanceInfo.EnabledLayerCount = (uint) this.validationLayers.Count;
            
            return new Vk.Instance(instanceInfo);
        }
    }

    public class LogicalDeviceBuilder {

        private Vk.PhysicalDeviceFeatures deviceFeatures;
        private List<Vk.DeviceQueueCreateInfo> queueInfos;
        private List<string> extensions;
        private List<string> validationLayers;

        public LogicalDeviceBuilder() {
            this.queueInfos         = new List<Vk.DeviceQueueCreateInfo>();
            this.extensions         = new List<string>();
            this.validationLayers   = new List<string>();
        }

        public void SetFeatures(Vk.PhysicalDeviceFeatures features) {
            this.deviceFeatures = features;
        }

        public void EnableQueue(Vk.DeviceQueueCreateInfo queueInfo) {
            this.queueInfos.Add(queueInfo);
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

        public Vk.Device Create(Vk.PhysicalDevice physicalDevice) {
            Vk.DeviceCreateInfo deviceInfo = new Vk.DeviceCreateInfo();

            deviceInfo.QueueCreateInfos = this.queueInfos.ToArray();
            deviceInfo.QueueCreateInfoCount = (uint) this.queueInfos.Count;

            deviceInfo.EnabledFeatures = this.deviceFeatures;

            if(this.extensions.Count != 0) {
                deviceInfo.EnabledExtensionNames = this.extensions.ToArray();
            }

            deviceInfo.EnabledExtensionCount = (uint) this.extensions.Count;

            if(this.validationLayers.Count != 0) {
                deviceInfo.EnabledLayerNames = this.validationLayers.ToArray();
            }

            deviceInfo.EnabledLayerCount = (uint) this.validationLayers.Count;

            return physicalDevice.CreateDevice(deviceInfo);
        }
    }
}