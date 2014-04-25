using Faker;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLipsum.Core;
using System;

namespace Nkv.Tests.Fixtures
{
    public enum BookCategory
    {
        Fiction,
        NonFiction,
        TextBook
    }

    public class Book : Entity
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string[] Authors { get; set; }
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Pages { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BookCategory Category { get; set; }

        private static Random _rand = new Random();

        public static Book Generate()
        {
            var lipsumGenerator = new LipsumGenerator();
            return new Book
            {
                Key = Guid.NewGuid().ToString(),
                Title = lipsumGenerator.GenerateSentences(1, Sentence.Short)[0],
                Abstract = lipsumGenerator.GenerateParagraphs(1, Paragraph.Medium)[0],
                Authors = new string[] { NameFaker.Name(), NameFaker.Name() },
                Price = 9999m * (decimal)_rand.NextDouble() + 0.99m,
                ReleaseDate = DateTimeFaker.BirthDay(),
                Pages = _rand.Next(5, 3000),
                Category = EnumFaker.SelectFrom<BookCategory>()
            };
        }
    }
}
