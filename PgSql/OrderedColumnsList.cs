using System.Collections.Generic;
using Newtonsoft.Json;

namespace doob.PgSql
{
    public class OrderedColumnsList
    {

        [JsonProperty]
        private List<Column> _columns = new List<Column>();
        private object _lock = new object();

        public void Add(Column column)
        {

            if (!column.Position.HasValue || _columns.Count < column.Position.Value)
            {
                _columns.Add(column);
            }
            else
            {
                _columns.Insert(column.Position.Value, column);
                for (int i = 0; i < _columns.Count; i++)
                {
                    _columns[i].Position = i;
                }
                ReorderList();
            }

        }

        public List<Column> GetOrderedColums()
        {
            return _columns;
        }

        private void ReorderList()
        {

            var newList = new List<Column>();

            foreach (var col in _columns)
            {
                if (!col.Position.HasValue || newList.Count < col.Position.Value)
                {
                    newList.Add(col);
                }
                else
                {
                    newList.Insert(col.Position.Value, col);
                    for (int i = 0; i < newList.Count; i++)
                    {
                        newList[i].Position = i;
                    }
                }

                _columns = newList;
            }


        }

        internal OrderedColumnsList Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<OrderedColumnsList>(json);
        }
    }
}
