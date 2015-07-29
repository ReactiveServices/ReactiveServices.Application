using System;
using System.Numerics;
using Newtonsoft.Json.Serialization;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    public class JavaScriptComplexContractResolver : DefaultContractResolver
    {
        public static readonly JavaScriptComplexContractResolver Instance = new JavaScriptComplexContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(Complex))
                contract.Converter = new JavaScriptComplexConverter();

            return contract;
        }
    }
}