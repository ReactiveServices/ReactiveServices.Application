//using System.Diagnostics;
//using Matrix;
//using Newtonsoft.Json;
//using NLog;
//using ReactiveServices.ComputationalUnit.Work;
//using ReactiveServices.MessageBus;
//using ReactiveServices.SharedMemory;

//namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
//{
//    public class MatrixParsingWorker : Worker
//    {
//        public MatrixParsingWorker(ISubscriptionBus subscriptionBus, IPublishingBus publishingBus, 
//            IRequestBus requestBus, IResponseBus responseBus, ISharedMemory sharedMemory) 
//            : base(subscriptionBus, publishingBus, requestBus, responseBus, sharedMemory)
//        {
//        }

//        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        protected override bool TryExecute()
//        {
//            return DeserializeMatrix();
//        }

//        private bool DeserializeMatrix()
//        {
//            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new JavaScriptComplexContractResolver() };
//            var parsingRequest = ExecutingJob as MatrixParsingRequest;
//            Debug.Assert(parsingRequest != null, "parsingRequest != null");
//            var serializedMatrix = parsingRequest.SerializedMatrix;
//            var parsedMatrix = JsonConvert.DeserializeObject<ComplexMatrix>(
//                serializedMatrix,
//                jsonSerializerSettings
//            );
//            parsingRequest.ParsedMatrix = parsedMatrix.AsTuplesOfDoubles();

//            return true;
//        }

//        protected override void Complete()
//        {
//            PublishMatrixReversingRequest();
//        }

//        private void PublishMatrixReversingRequest()
//        {
//            var parsingRequest = ExecutingJob as MatrixParsingRequest;
//            var reversingRequest = TimedMessage.New<MatrixReversingRequest>();
//            reversingRequest.ParsingRequest = parsingRequest;
//            Publish(reversingRequest, JobStatus.Pending.TopicId());
//            Debug.Assert(parsingRequest != null, "parsingRequest != null");
//            Log.Info("PublishMatrixReversingRequest of id {0} for MatrixParsingRequest of id {1}.", reversingRequest.MessageId, parsingRequest.MessageId);
//        }

//        protected override void Fail()
//        {
//        }
//    }

//    public class MatrixReversingWorker : Worker
//    {
//        public MatrixReversingWorker(ISubscriptionBus subscriptionBus, IPublishingBus publishingBus, 
//            IRequestBus requestBus, IResponseBus responseBus, ISharedMemory sharedMemory) 
//            : base(subscriptionBus, publishingBus, requestBus, responseBus, sharedMemory)
//        {
//        }

//        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        protected override bool TryExecute()
//        {
//            return ReverseMatrix();
//        }

//        private bool ReverseMatrix()
//        {
//            var reversingRequest = ExecutingJob as MatrixReversingRequest;
//            if (IsCancellationRequested)
//                return false;

//            Debug.Assert(reversingRequest != null, "reversingRequest != null");
//            var parsedMatrix = ComplexMatrix.FromTuplesOfDoubles(reversingRequest.ParsingRequest.ParsedMatrix);
//            if (IsCancellationRequested)
//                return false;

//            var reversedMatrix = parsedMatrix.Inverse();
//            if (IsCancellationRequested)
//                return false;

//            reversingRequest.ReversedMatrix = reversedMatrix.AsTuplesOfDoubles();
//            return true;
//        }

//        protected override void Complete()
//        {
//            var reversingRequest = ExecutingJob as MatrixReversingRequest;
//            Debug.Assert(reversingRequest != null, "reversingRequest != null");
//            Log.Info("PublishCompletionEvent for MatrixReversingRequest of id {0} and MatrixParsingRequest of id {1}.", reversingRequest.MessageId, reversingRequest.ParsingRequest.MessageId);
//        }

//        protected override void Fail()
//        {
//        }
//    }
//}