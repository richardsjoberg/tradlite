using dto.endpoint.clientsentiment;

namespace Tradlite.Models.Ig
{
    public class SentimentResponse
    {
        public ClientSentiment Sentiment { get; set; }
        public ClientSentimentList RelatedSentiment { get; set; }
    }
}
