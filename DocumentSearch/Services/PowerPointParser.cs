using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace DocumentSearch.Services;

public class PowerPointParser : IPowerPointParser
{
    public string ExtractText(string filePath)
    {
        try
        {
            using var presentationDocument = PresentationDocument.Open(filePath, false);
            var presentationPart = presentationDocument.PresentationPart;
            if (presentationPart == null || presentationPart.Presentation == null)
                return string.Empty;

            var slideIdList = presentationPart.Presentation.SlideIdList;
            if (slideIdList == null)
                return string.Empty;

            var sb = new StringBuilder();
            int slideIndex = 1;

            foreach (var slideId in slideIdList.Elements<SlideId>())
            {
                if (string.IsNullOrEmpty(slideId.RelationshipId))
                    continue;

                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                if (slidePart?.Slide != null)
                {
                    var slideText = ExtractTextFromSlide(slidePart.Slide);
                    if (!string.IsNullOrWhiteSpace(slideText))
                    {
                        sb.Append($"---PAGE_{slideIndex}---");
                        sb.Append(slideText);
                        sb.Append(" ");
                    }
                }
                slideIndex++;
            }

            return sb.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractTextFromSlide(Slide slide)
    {
        var slideText = new StringBuilder();
        foreach (var text in slide.Descendants<A.Text>())
        {
            if (!string.IsNullOrWhiteSpace(text.Text))
            {
                slideText.Append(text.Text).Append(" ");
            }
        }
        return slideText.ToString().Trim();
    }
}
