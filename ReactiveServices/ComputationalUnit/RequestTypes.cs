using MessageBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkDispatcher.Infrastructure
{
    public class ExecutarOperacao : Message<ExecutarOperacao>
    {

    }

    public  class Job
    {
        public MessageId MessageId { get; set; }
        public DateTime CreationDate { get; set; }
        public TimeSpan Validity { get; set; }

        public Job()
        {
            CreationDate = DateTime.Now;
        }

        public bool IsExpired()
        {
            return (DateTime.Now - CreationDate) > Validity;
        }
    }
}
