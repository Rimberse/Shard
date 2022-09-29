namespace Shard.API.Models
{
    public class NotFoundSpecification
    {
        public string type { get; set; }
        public string title { get; set; }
        public int status { get; set; }
        public string detail { get; set; }
        public string instance { get; set; }
        public string additionalProp1 { get; set; }
        public string additionalProp2 { get; set; }
        public string additionalProp3 { get; set; }

       /* internal NotFoundSpecification()
        {
            type = "string";
            title = "string";
            status = 0;
            detail = "string";
            instance = "string";
            additionalProp1 = "string";
            additionalProp2 = "string";
            additionalProp3 = "string";
        }*/
    }
}
