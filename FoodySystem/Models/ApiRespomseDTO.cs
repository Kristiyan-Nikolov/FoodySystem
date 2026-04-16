using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoodySystem.Models
{
    internal class ApiRespomseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
        [JsonPropertyName("foodId")]
        public string FoodId { get; set; }
    }
}
