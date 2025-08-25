using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.Utilties;

public class CustomModConfigItemAttribute<T> : CustomModConfigItemAttribute
{
    public CustomModConfigItemAttribute() : base(typeof(T))
    {

    }
}
