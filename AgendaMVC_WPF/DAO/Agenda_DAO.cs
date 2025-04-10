using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgendaMVC_WPF.agendaDB;

namespace AgendaMVC_WPF.DAO
{
    public class Agenda_DAO
    {
        private readonly AgendaContext _context;

        public Agenda_DAO()
        {
            _context = new AgendaContext();
        }

        #region Contacts

        public List<Contact> GetAllContacts()
        {
            using (var context = new AgendaContext())
            {
                var AllContacts = context.Contacts
                    .Include(c => c.ReseauxSociaux)
                    .ToList();
                return AllContacts;
            }
        }

        public Contact GetContactById(int id)
        {
            using (var context = new AgendaContext())
            {
                return context.Contacts
                    .Include(c => c.ReseauxSociaux)
                    .FirstOrDefault(c => c.Id == id);
            }
        }

        public Contact GetContactByEmail(string email)
        {
            using (var context = new AgendaContext())
            {
                return context.Contacts
                    .Include(c => c.ReseauxSociaux)
                    .FirstOrDefault(c => c.Email == email);
            }
        }

        public List<Contact> SearchContacts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllContacts();

            using (var context = new AgendaContext())
            {
                return context.Contacts
                    .Include(c => c.ReseauxSociaux)
                    .Where(c => c.Nom.Contains(searchTerm) ||
                                c.Prenom.Contains(searchTerm) ||
                                (c.Email != null && c.Email.Contains(searchTerm)) ||
                                (c.Telephone != null && c.Telephone.Contains(searchTerm)))
                    .ToList();
            }
        }

        public bool AddContact(Contact contact)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    context.Contacts.Add(contact);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout du contact: {ex.Message}");
                return false;
            }
        }

        public bool UpdateContact(Contact contact)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    context.Contacts.Update(contact);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour du contact: {ex.Message}");
                return false;
            }
        }

        public bool DeleteContact(int id)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    var contact = context.Contacts.Find(id);
                    if (contact == null)
                        return false;

                    // Supprimer d'abord les réseaux sociaux associés
                    var reseauxSociaux = context.ReseauxSociauxes.Where(rs => rs.ContactId == id).ToList();
                    context.ReseauxSociauxes.RemoveRange(reseauxSociaux);

                    context.Contacts.Remove(contact);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression du contact: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Tâches

        public List<Tach> GetAllTaches()
        {
            using (var context = new AgendaContext())
            {
                return context.Taches.ToList();
            }
        }

        public Tach GetTacheById(int id)
        {
            using (var context = new AgendaContext())
            {
                return context.Taches.FirstOrDefault(t => t.Id == id);
            }
        }

        public List<Tach> GetTachesByStatus(string status)
        {
            using (var context = new AgendaContext())
            {
                return context.Taches
                    .Where(t => t.Statut == status)
                    .ToList();
            }
        }

        public Category GetCategorieById(int id)
        {
            using (var context = new AgendaContext())
            {
                return context.Categories.FirstOrDefault(c => c.Id == id);
            }
        }

        public bool AddTache(Tach tache)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    context.Taches.Add(tache);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout de la tâche: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTache(Tach tache)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    context.Taches.Update(tache);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour de la tâche: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTache(int id)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    var tache = context.Taches.Find(id);
                    if (tache == null)
                        return false;

                    context.Taches.Remove(tache);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression de la tâche: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Catégories

        public List<Category> GetAllCategories()
        {
            using (var context = new AgendaContext())
            {
                return context.Categories.ToList();
            }
        }

        public bool AddCategorie(Category categorie)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    context.Categories.Add(categorie);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout de la catégorie: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Réseaux Sociaux

        public List<ReseauxSociaux> GetReseauxSociauxByContactId(int contactId)
        {
            using (var context = new AgendaContext())
            {
                return context.ReseauxSociauxes
                    .Where(rs => rs.ContactId == contactId)
                    .ToList();
            }
        }

        public bool AddReseauSocial(ReseauxSociaux reseauSocial)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    context.ReseauxSociauxes.Add(reseauSocial);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout du réseau social: {ex.Message}");
                return false;
            }
        }

        public bool DeleteReseauSocial(int id)
        {
            try
            {
                using (var context = new AgendaContext())
                {
                    var reseauSocial = context.ReseauxSociauxes.Find(id);
                    if (reseauSocial == null)
                        return false;

                    context.ReseauxSociauxes.Remove(reseauSocial);
                    int result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression du réseau social: {ex.Message}");
                return false;
            }
        }

        #endregion

        // Méthode utilitaire pour convertir DateOnly en DateTime et vice-versa
        public static DateTime? DateOnlyToDateTime(DateOnly? dateOnly)
        {
            if (dateOnly.HasValue)
                return new DateTime(dateOnly.Value.Year, dateOnly.Value.Month, dateOnly.Value.Day);
            return null;
        }

        public static DateOnly? DateTimeToDateOnly(DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return new DateOnly(dateTime.Value.Year, dateTime.Value.Month, dateTime.Value.Day);
            return null;
        }
    }
}

