using ReactiveServices.ComputationalUnit.Work;
using System;
using System.Runtime.Serialization;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    public class MatrixParsingRequest : Job
    {
        [DataMember]
        public string SerializedMatrix { get; set; }
        [DataMember]
        public Tuple<double, double>[,] ParsedMatrix{ get; set; }
    }

    public class MatrixReversingRequest : Job
    {
        [DataMember]
        public MatrixParsingRequest ParsingRequest { get; set; }
        [DataMember]
        public Tuple<double, double>[,] ReversedMatrix { get; set; }
        [DataMember]
        public string SerializedReversedMatrix { get; set; }
    }
}
