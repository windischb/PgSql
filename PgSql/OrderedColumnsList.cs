using System.Collections.Generic;
using Newtonsoft.Json;

namespace doob.PgSql
{
    public class OrderedColumnsList
    {

        [JsonProperty]
        private List<Column> Columns = new List<Column>();
        private object _lock = new object();

        public void Add(Column column)
        {

            if (!column.Position.HasValue)
            {
                Columns.Add(column);
            }
            else
            {
                Columns.Insert(column.Position.Value, column);
                for (int i = 0; i < Columns.Count; i++)
                {
                    Columns[i].Position = i;
                }
                ReorderList();
            }

        }

        public List<Column> GetOrderedColums()
        {
            return Columns;
        }

        private void ReorderList()
        {

            var newList = new List<Column>();

            foreach (var col in Columns)
            {
                if (!col.Position.HasValue)
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

                Columns = newList;
            }


        }

        internal OrderedColumnsList Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<OrderedColumnsList>(json);
        }
    }
}
