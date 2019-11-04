using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sample.Connector
{
    public class ValidationResponse
    {
        [JsonProperty("Status")]
        public bool Status;
        [JsonProperty("Message")]
        public string Message;
        public ValidationResponse(bool status, string message = null)
        {
            Status = status;
            Message = message;
        }
    }
    public class JobCreationResponse
    {
        [JsonProperty("IsJobCreated")]
        public bool IsJobCreated;
        [JsonProperty("Reason")]
        public string Reason;
        public JobCreationResponse(bool isjobcreated, string reason = null)
        {
            IsJobCreated = isjobcreated;
            Reason = reason;
        }
    }
}
