using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arksplorer
{
    ///// <summary>
    ///// All row's need a reference to their source data's column positions - so when we are processing things like
    ///// mouse over events (which come from unknown UI element references) we can trace back to what kind of data this is
    ///// e.g. if PrimaryDataVisual contains wild dinos and the SecondaryDataVisual contains tamed ones, we end up
    ///// with 2 data sets with different column definitions - which to use in a pop up? Row itself can tell us!
    ///// </summary>
    //public class Data
    //{
    //    public DataTable DataTable { get; set; }
    //    public ColumnPositions ColumnPositions { get; set; }
    //}
}
