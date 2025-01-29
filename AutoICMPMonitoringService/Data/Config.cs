using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_ICMP_Monitoring.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Config
    {
        [JsonProperty]
        public static string tokenBot = "5615169018:AAHTdF_9GtH35QOGkI3TROFS3YAYwv1KLJM";
        [JsonProperty]
        public static long idGroup = -1001598917322;
        [JsonProperty]
        public static int recheckCount = 2;
        [JsonProperty]
        public static int tickTimeSec = 120;
    }
}
