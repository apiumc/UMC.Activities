using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Activities.Entities
{
    public partial class PageConfig
    {
        static Action<PageConfig, object>[] _SetValues = new Action<PageConfig, object>[6];
        static string[] _Columns = new string[6];
        static PageConfig()
        {
            _Columns[0] = "AppKey";
            _SetValues[0] = (r, t) => r.AppKey = Reflection.ParseObject(t, r.AppKey);
            _Columns[1] = "GroupBy";
            _SetValues[1] = (r, t) => r.GroupBy = Reflection.ParseObject(t, r.GroupBy);
            _Columns[2] = "Id";
            _SetValues[2] = (r, t) => r.Id = Reflection.ParseObject(t, r.Id);
            _Columns[3] = "Name";
            _SetValues[3] = (r, t) => r.Name = Reflection.ParseObject(t, r.Name);
            _Columns[4] = "Sequence";
            _SetValues[4] = (r, t) => r.Sequence = Reflection.ParseObject(t, r.Sequence);
            _Columns[5] = "Value";
            _SetValues[5] = (r, t) => r.Value = Reflection.ParseObject(t, r.Value);

        }
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "AppKey", this.AppKey);
            AppendValue(action, "GroupBy", this.GroupBy);
            AppendValue(action, "Id", this.Id);
            AppendValue(action, "Name", this.Name);
            AppendValue(action, "Sequence", this.Sequence);
            AppendValue(action, "Value", this.Value);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[6];
            cols[0] = RecordColumn.Column("AppKey", this.AppKey);
            cols[1] = RecordColumn.Column("GroupBy", this.GroupBy);
            cols[2] = RecordColumn.Column("Id", this.Id);
            cols[3] = RecordColumn.Column("Name", this.Name);
            cols[4] = RecordColumn.Column("Sequence", this.Sequence);
            cols[5] = RecordColumn.Column("Value", this.Value);
            return cols;
        }

    }
}

