using System;


namespace UMC.Activities.Entities
{
    public class Design_Config
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


    }
}