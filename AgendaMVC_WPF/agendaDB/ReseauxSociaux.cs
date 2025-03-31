using System;
using System.Collections.Generic;

namespace AgendaMVC_WPF.agendaDB;

public partial class ReseauxSociaux
{
    public int Id { get; set; }

    public int ContactId { get; set; }

    public string Plateforme { get; set; } = null!;

    public string Lien { get; set; } = null!;

    public virtual Contact Contact { get; set; } = null!;
}
