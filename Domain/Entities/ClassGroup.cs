namespace TeachTether.Domain.Entities
{
    public class ClassGroup
    {
        public int Id { get; set; }
        public int GradeYear { get; set; }
        public char Section { get; set; }
        public int HomeroomTeacherId { get; set; }
        public int SchoolId { get; set; }
    }
}
