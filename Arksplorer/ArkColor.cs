using System;
using System.Collections;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Arksplorer
{
    public class ArkColor //: IComparable<ArkColor>
    {
        public int Id { get; set; }
        public string Hex { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public Brush Color { get; set; }

        //public int CompareTo(ArkColor otherArkColor)
        //{
        //    return (this.Id > otherArkColor.Id) ? 1 : (this.Id == otherArkColor.Id ? 0 : -1);
        //}
    }

    //public class SortArkColorAscendingHelper : IComparer
    //{
    //    int IComparer.Compare(object arkColor1, object arkColor2)
    //    {
    //        ArkColor a = (ArkColor)arkColor1;
    //        ArkColor b = (ArkColor)arkColor2;

    //        return (a.Id > b.Id) ? 1 : (a.Id == b.Id ? 0 : -1);
    //    }
    //}

    //public class SortArkColorDescendingHelper : IComparer
    //{
    //    int IComparer.Compare(object arkColor1, object arkColor2)
    //    {
    //        ArkColor a = (ArkColor)arkColor1;
    //        ArkColor b = (ArkColor)arkColor2;

    //        return (a.Id < b.Id) ? 1 : (a.Id == b.Id ? 0 : -1);
    //    }
    //}
}
