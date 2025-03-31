using System;
using System.Collections.Generic;

namespace AgendaMVC_WPF.agendaDB;

public partial class Contact
{
    public int Id { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telephone { get; set; }

    public string? Adresse { get; set; }

    public DateOnly? DateNaissance { get; set; }

    public DateTime? DateCreation { get; set; }

    public virtual ICollection<ReseauxSociaux> ReseauxSociaux { get; set; } = new List<ReseauxSociaux>();
}
