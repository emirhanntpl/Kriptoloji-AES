using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Models
{
    public class NetworkPacket
    {
        public string Type { get; set; } = "text";     
        public string Mode { get; set; } = "unsecure"; 
        public string? FileName { get; set; }          
        public string? Content { get; set; }           
    }



}
}
