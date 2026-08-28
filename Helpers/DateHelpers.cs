#nullable disable

namespace VontaGenta.Helpers
{

    public class Month
    {
        public int ID { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }

        public Month(int id, string shortName, string longName)
        {
            ID = id;
            ShortName = shortName;
            LongName = longName;
        }
    }

    public class Months
    {
        public List<Month> List;

        public Months()
        {
            List = new List<Month> 
            {
                new Month(1,"Jan","January"),
                new Month(2,"Feb","February"),
                new Month(3,"Mar","March"),
                new Month(4,"Apr","April"),
                new Month(5,"May","May"),
                new Month(6,"Jun","June"),
                new Month(7,"Jul","July"),
                new Month(8,"Aug","August"),
                new Month(9,"Sep","September"),
                new Month(10,"Oct","October"),
                new Month(11,"Nov","November"),
                new Month(12,"Dec","December"),
            };
        }
    }

}
