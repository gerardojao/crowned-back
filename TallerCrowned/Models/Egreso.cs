using System;
using System.Collections.Generic;

namespace FamilyApp.Models;

public partial class Egreso
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    //public virtual ICollection<FichaEgreso> FichaEgresos { get; } = new List<FichaEgreso>();
}
