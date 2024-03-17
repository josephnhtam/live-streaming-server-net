using k8s.Models;
using LiveStreamingServerNet.KubernetesPod.Utilities.Contracts;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Serialization;

namespace LiveStreamingServerNet.KubernetesPod.Utilities
{
    public class PodPatcherBuilder : IPodPatcherBuilder
    {
        private readonly JsonPatchDocument<V1Pod> _doc;

        public PodPatcherBuilder()
        {
            _doc = new JsonPatchDocument<V1Pod>();
            _doc.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
        }

        public IPodPatcherBuilder SetLabel(string key, string value)
        {
            _doc.Replace(x => x.Metadata.Labels[key], value);
            return this;
        }

        public IPodPatcherBuilder RemoveLabel(string key)
        {
            _doc.Remove(x => x.Metadata.Labels[key]);
            return this;
        }

        public IPodPatcherBuilder SetAnnotation(string key, string value)
        {
            _doc.Replace(x => x.Metadata.Annotations[key], value);
            return this;
        }

        public IPodPatcherBuilder RemoveAnnotation(string key)
        {
            _doc.Remove(x => x.Metadata.Annotations[key]);
            return this;
        }

        public JsonPatchDocument<V1Pod> Build()
        {
            return _doc;
        }
    }
}
