namespace DurableFunctionOrchestration.Models
{
    internal class ApprovalRequest
    {
        public bool Approved { get; set; }
        public string InstanceId { get; set; }
    }
}
