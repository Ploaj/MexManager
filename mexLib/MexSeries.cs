using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mexLib
{
    public class MexSeries
    {
        [Category("General"), DisplayName("Name"), Description("Name of the series")]
        public string Name { get; set; } = "";

        // TODO: 3d emblem

        public override string ToString()
        {
            return Name;
        }
    }
}
