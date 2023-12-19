using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;

namespace API.Entities;

public class Group
{
    public Group() // Empty constructor for entity framework, when schema is created for db so that when it creates this new 'Group' class, it won't try to pass 'Name' as well
    {
        
    }
    public Group(string name)
    {
        Name = name;
    }

    [Key] // Name is a primary key. Means you cannot add to groups with the same name in the db. Have to be unique
    public string Name { get; set; }
    public ICollection<Connection> Connections { get; set; } = new List<Connection>(); // ICollection allows iteration over a list but also allows features such as add/remove from a list
}
