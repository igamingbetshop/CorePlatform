using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.AutomationTest.Models
{
    public class Condition
    {
        public List<ConditionItem> ConditionItems { get; set; }

        public override string ToString() 
        {
            return JsonConvert.SerializeObject(ConditionItems);

        }
    }

    public class ConditionItem
    {
        public int OperationTypeId{ get; set; }
        public string StringValue { get; set; }
    }
}
