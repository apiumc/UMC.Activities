using System;
using UMC.Data;

namespace UMC.Activities.Entities
{
    public partial class PageConfig : Record
    {
        public Guid? Id
        {
            get; set;
        }
        public String Value
        {
            get; set;
        }
        public String Name
        {
            get; set;
        }
        public String GroupBy
        {
            get; set;
        }
        public int? Sequence
        {
            get; set;
        }
        public Guid? AppKey
        {
            get;set;
        }


    }
}