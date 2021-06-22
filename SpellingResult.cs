using System;
using System.Collections.Generic;
using System.Text;

namespace BingSpellCheck
{
    class SpellingResult
    {
        public string ClientId { get; set; }

        public string TraceId { get; set; }

        public SpellingResponseBody Text { get; set; }

        public string CorrectedText { get; set; }
    }
}
