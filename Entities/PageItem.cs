

using System;
using UMC.Data;

namespace UMC.Activities.Entities
{
    public partial class PageItem : Record
    {

        public Guid? Id
        {
            get; set;
        }
        public int? Type
        {
            get; set;
        }
        public Guid? design_id
        {
            get; set;
        }
        public Guid? for_id
        {
            get; set;
        }

        public Guid? value_id
        {
            get; set;
        }


        public int? Seq
        {
            get; set;
        }


        public String ItemName
        {
            get; set;
        }


        public String ItemDesc
        {
            get; set;
        }


        public String Click
        {
            get; set;
        }


        public String Style
        {
            get; set;
        }


        public String Data
        {
            get; set;
        }


        public DateTime? ModifiedDate
        {
            get; set;
        }
        public Guid? AppKey
        {
            get; set;
        }

    }
}