using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Unnati.Models;

namespace Unnati.Helper
{
    public class PDFGenerator
    {
        public async static Task<Document> createPdfContent<T>(IList<T> items, string[] columnNames)
        //where T : IUserModel, IProducts
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Set margins for the page
                    page.Margin(50);
                    
                    // Header section
                    page.Header().Element(Block).Text(GetHeaderText<T>())
                        .FontSize(40)
                        .FontColor(Color.FromRGB(234, 244, 251))
                        .AlignCenter().Bold();

                    page.Background(Color.FromRGB(189, 220, 241)); // Light blue background

                    // Space between the header and the content
                    page.Content().PaddingVertical(20);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var columnName in columnNames)
                            {
                                columns.RelativeColumn(1); // Adjust this to set specific column widths if necessary
                            }
                        });

                        // Table header with bold and larger font, padding applied to the cell
                        table.Header(header =>
                        {
                            foreach (var columnName in columnNames)
                            {
                                header.Cell().Border(0.4f)
                                    .BorderColor(Color.FromRGB(200, 200, 200))
                                    .Background(Color.FromRGB(200, 200, 200))
                                    .Element(Block).Text(columnName)
                                    .FontSize(14).Bold()
                                    .FontColor(Color.FromRGB(54, 69, 79));
                            }
                        });

                        // Add rows dynamically for each user
                        foreach (var item in items)
                        {
                            foreach (var property in typeof(T).GetProperties())
                            {
                                // Get property value and handle null values gracefully
                                var value = property.GetValue(item)?.ToString() ?? string.Empty;

                                table.Cell().Border(0.4f)
                                    .BorderColor(Color.FromRGB(200, 200, 200))
                                    .Element(Block).Text(value).FontSize(12);
                            }
                        }
                    });
                });
            });

            // document.ShowInPreviewer();

            return document;
        }

        private static IContainer Block(IContainer container)
        {
            return container
                .Border(1)
                .AlignCenter();
        }

        private static string GetHeaderText<T>()
        {
            if (typeof(T) == typeof(UserModel))
            {
                return "User List Report";
            }
            else if (typeof(T) == typeof(Products))
            {
                return "Product List Report";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
