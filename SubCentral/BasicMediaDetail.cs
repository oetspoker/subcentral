using System.IO;

namespace SubCentral
{
    public struct BasicMediaDetail
    {
        public int? MediaId { get; set; }
        public string ImdbID { get; set; }
        public FileInfo File { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
    }
}
