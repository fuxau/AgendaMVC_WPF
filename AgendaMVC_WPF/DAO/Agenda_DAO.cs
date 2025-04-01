using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // si tu utilises EF Core
using AgendaMVC_WPF.agendaDB;

namespace AgendaMVC_WPF.DAO
{
    public class Agenda_DAO
    {
        private readonly AgendaContext _context;

        // Tu peux injecter le contexte ou l'instancier directement
        public Agenda_DAO()
        {
            _context = new AgendaContext();
        }

        // Exemple de méthodes CRUD pour "Contact"
        public List<Contact> GetAllContacts()
        {
            using (var context = new AgendaContext())
            {
                var AllContacts = context.Contacts.ToList();
                return AllContacts;
            }
        }

        public async Task<Contact> GetContactByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        public string AddContact(Contact contact)
        {
            using (var Context = new AgendaContext())
            {
                Context.Contacts.Add(contact);
                Context.SaveChanges();
            }
            return "Contact ajouté avec succès";
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }

        // Idem pour Tache, Category, RéseauxSociaux, etc.
    }
}
