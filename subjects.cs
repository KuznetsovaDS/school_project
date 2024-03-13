using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;

namespace WpfApp1
{
    internal class subjects : ObservableCollection<string>
    {
        public void AddRange(IEnumerable<string> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }
    }
}

