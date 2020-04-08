using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthTest.Models
{
    public class ChatConfig
    {
        public string Token { get; set; }

        public string Domain { get; set; }

        public string UserId { get; set; }

        public string ClientId { get; set; }
        
        public string TenantId { get; set; }
    }
}
