using Nkv.Attributes;
using NLipsum.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv.Tests.Fixtures
{
    [Table("BlogPosts")]
    public class BlogEntry : Entity
    {
        public string Title { get; set; }
        public string Content { get; set; }

        private static Random _rand = new Random();

        public static BlogEntry Generate()
        {
            var lipsumGenerator = new LipsumGenerator();
            return new BlogEntry
            {
                Key = Guid.NewGuid().ToString(),
                Title = lipsumGenerator.GenerateSentences(1, Sentence.Short)[0],
                Content = lipsumGenerator.GenerateParagraphs(5, Paragraph.Medium)[0]
            };
        }
    }
}
