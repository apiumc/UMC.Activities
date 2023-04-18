using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Activities.Entities
{
    public partial class PageItem
    {
        static Action<PageItem, object>[] _SetValues = new Action<PageItem, object>[13];
        static string[] _Columns = new string[13];
        static PageItem()
        {
            _Columns[0] = "AppKey";
            _SetValues[0] = (r, t) => r.AppKey = Reflection.ParseObject(t, r.AppKey);
            _Columns[1] = "Click";
            _SetValues[1] = (r, t) => r.Click = Reflection.ParseObject(t, r.Click);
            _Columns[2] = "Data";
            _SetValues[2] = (r, t) => r.Data = Reflection.ParseObject(t, r.Data);
            _Columns[3] = "design_id";
            _SetValues[3] = (r, t) => r.design_id = Reflection.ParseObject(t, r.design_id);
            _Columns[4] = "for_id";
            _SetValues[4] = (r, t) => r.for_id = Reflection.ParseObject(t, r.for_id);
            _Columns[5] = "Id";
            _SetValues[5] = (r, t) => r.Id = Reflection.ParseObject(t, r.Id);
            _Columns[6] = "ItemDesc";
            _SetValues[6] = (r, t) => r.ItemDesc = Reflection.ParseObject(t, r.ItemDesc);
            _Columns[7] = "ItemName";
            _SetValues[7] = (r, t) => r.ItemName = Reflection.ParseObject(t, r.ItemName);
            _Columns[8] = "ModifiedDate";
            _SetValues[8] = (r, t) => r.ModifiedDate = Reflection.ParseObject(t, r.ModifiedDate);
            _Columns[9] = "Seq";
            _SetValues[9] = (r, t) => r.Seq = Reflection.ParseObject(t, r.Seq);
            _Columns[10] = "Style";
            _SetValues[10] = (r, t) => r.Style = Reflection.ParseObject(t, r.Style);
            _Columns[11] = "Type";
            _SetValues[11] = (r, t) => r.Type = Reflection.ParseObject(t, r.Type);
            _Columns[12] = "value_id";
            _SetValues[12] = (r, t) => r.value_id = Reflection.ParseObject(t, r.value_id);

        }
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "AppKey", this.AppKey);
            AppendValue(action, "Click", this.Click);
            AppendValue(action, "Data", this.Data);
            AppendValue(action, "design_id", this.design_id);
            AppendValue(action, "for_id", this.for_id);
            AppendValue(action, "Id", this.Id);
            AppendValue(action, "ItemDesc", this.ItemDesc);
            AppendValue(action, "ItemName", this.ItemName);
            AppendValue(action, "ModifiedDate", this.ModifiedDate);
            AppendValue(action, "Seq", this.Seq);
            AppendValue(action, "Style", this.Style);
            AppendValue(action, "Type", this.Type);
            AppendValue(action, "value_id", this.value_id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[13];
            cols[0] = RecordColumn.Column("AppKey", this.AppKey);
            cols[1] = RecordColumn.Column("Click", this.Click);
            cols[2] = RecordColumn.Column("Data", this.Data);
            cols[3] = RecordColumn.Column("design_id", this.design_id);
            cols[4] = RecordColumn.Column("for_id", this.for_id);
            cols[5] = RecordColumn.Column("Id", this.Id);
            cols[6] = RecordColumn.Column("ItemDesc", this.ItemDesc);
            cols[7] = RecordColumn.Column("ItemName", this.ItemName);
            cols[8] = RecordColumn.Column("ModifiedDate", this.ModifiedDate);
            cols[9] = RecordColumn.Column("Seq", this.Seq);
            cols[10] = RecordColumn.Column("Style", this.Style);
            cols[11] = RecordColumn.Column("Type", this.Type);
            cols[12] = RecordColumn.Column("value_id", this.value_id);
            return cols;
        }

    }
}

