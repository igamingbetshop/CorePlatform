using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Filters
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
        //{
        //    get => OperationTypeId;
        //    set
        //    {
        //        if (!Enum.IsDefined(typeof(FilterOperations), value))
        //            throw new Exception("Wrong operation type");
        //        OperationTypeId = value;
        //    }
        //}
        public string StringValue { get; set; }
    }
}
