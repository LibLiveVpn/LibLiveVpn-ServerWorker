namespace LibLiveVpn_ServerWorker.Application.Models
{
    public class CommandResult
    {
        public int StatusCode { get; set; }

        public string Details { get; set; } = null!;

        public bool IsSuccess()
        {
            return StatusCode == 0;
        }
    }
}
