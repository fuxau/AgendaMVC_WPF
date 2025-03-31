using System;
using System.Collections.Generic;

namespace AgendaMVC_WPF.agendaDB;

public partial class Tach
{
    public int Id { get; set; }

    public string Titre { get; set; } = null!;

    public string? Description { get; set; }

    public string? Statut { get; set; }

    public DateOnly? DateEcheance { get; set; }

    public int? CategorieId { get; set; }

    public DateTime? DateCreation { get; set; }
}
