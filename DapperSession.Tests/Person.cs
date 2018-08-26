using System.ComponentModel.DataAnnotations.Schema;

namespace DapperSession.Tests
{
    [Table("Person")]
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
    }
}