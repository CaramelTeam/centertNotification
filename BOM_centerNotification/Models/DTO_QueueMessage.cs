using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BOM_centerNotification.Models
{
    public class DTO_QueueMessage
    {
        [Key]
        public int id { get; set; }
        public string? nameUser { get; set; }
        public string? message { get; set; }
        public bool confirmRead { get; set; }
        public DateTime registeNotification { get; set; }
        public string? userSend { get; set; }
    }
}
