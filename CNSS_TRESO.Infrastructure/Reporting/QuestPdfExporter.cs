using System.Globalization;
using System.Text;
using CNSS_TRESO.Application.Configuration;
using CNSS_TRESO.Application.Treasury.Reporting;
using Microsoft.Extensions.Options;

namespace CNSS_TRESO.Infrastructure.Reporting;

public sealed class QuestPdfExporter : IPdfExporter
{
    private readonly ReportBranding _branding;

    public QuestPdfExporter(IOptions<ReportBranding> branding)
    {
        _branding = branding.Value;
    }

    public byte[] ExportCashReport(CashReport report)
    {
        const int rowsPerPage = 25;
        var logo = PdfJpeg.TryLoad(_branding.LogoPath);
        var pages = report.Rows.Chunk(rowsPerPage).Select(rows => rows.ToArray()).ToList();
        if (pages.Count == 0) pages.Add([]);

        var contents = pages.Select((rows, index) => BuildPage(report, rows, index + 1, pages.Count, index == pages.Count - 1, logo is not null)).ToList();
        return CreateDocument(contents, logo);
    }

    private string BuildPage(CashReport report, IReadOnlyList<CashReportRow> rows, int pageNumber, int pageCount, bool isLastPage, bool hasLogo)
    {
        var isCash = string.Equals(report.JournalType, "C", StringComparison.OrdinalIgnoreCase);
        var title = isCash ? "LIVRE DE CAISSE" : "LIVRE DE BANQUE";
        var content = new StringBuilder();

        if (hasLogo) content.AppendLine("q 62 0 0 38 32 544 cm /Im1 Do Q");
        var textStart = hasLogo ? 105 : 32;
        WriteText(content, textStart, 565, 11, _branding.Organization);
        WriteText(content, textStart, 550, 8, $"Centre de frais : {report.CostCenterLabel ?? report.CostCenter ?? "(tous)"}");
        content.AppendLine("32 542 m 810 542 l S");
        WriteText(content, 210, 525, 11, $"{title} {report.JournalCode} {report.JournalLabel}");
        WriteText(content, 310, 512, 8, $"DU {report.FromDate:dd/MM/yyyy} AU {report.ToDate:dd/MM/yyyy}");
        WriteText(content, 32, 495, 8, $"Solde initial : {report.OpeningBalance:N2}");
        WriteText(content, 215, 495, 8, $"Total recettes : {report.TotalReceipts:N2}");
        WriteText(content, 410, 495, 8, $"Total depenses : {report.TotalExpenses:N2}");
        WriteText(content, 615, 495, 8, $"Solde final : {report.ClosingBalance:N2}");

        var headers = new[] { "Date", "N Doc", "N Piece", "Compte", "Libelle", "Beneficiaire", "Recette", "Depense", "Solde" };
        var columns = new[] { 32, 80, 134, 188, 270, 500, 610, 674, 738 };
        for (var index = 0; index < headers.Length; index++)
            WriteText(content, columns[index], 477, 7, headers[index]);
        content.AppendLine("32 472 m 810 472 l S");

        var y = 459;
        foreach (var row in rows)
        {
            WriteText(content, columns[0], y, 7, row.MovementDate.ToString("dd/MM/yyyy"));
            WriteText(content, columns[1], y, 7, Truncate(row.DocumentNumber, 9));
            WriteText(content, columns[2], y, 7, Truncate(row.PieceNumber, 9));
            WriteText(content, columns[3], y, 7, Truncate(row.Account, 14));
            WriteText(content, columns[4], y, 7, Truncate(row.Description, 42));
            WriteText(content, columns[5], y, 7, Truncate(row.Beneficiary, 19));
            WriteText(content, columns[6], y, 7, row.AmountReceipt.ToString("N2"));
            WriteText(content, columns[7], y, 7, row.AmountExpense.ToString("N2"));
            WriteText(content, columns[8], y, 7, row.RunningBalance.ToString("N2"));
            y -= 13;
        }

        if (isLastPage)
        {
            content.AppendLine($"32 {y + 5} m 810 {y + 5} l S");
            WriteText(content, 540, y - 8, 8, "TOTAUX");
            WriteText(content, columns[6], y - 8, 8, report.TotalReceipts.ToString("N2"));
            WriteText(content, columns[7], y - 8, 8, report.TotalExpenses.ToString("N2"));
            WriteText(content, columns[8], y - 8, 8, report.ClosingBalance.ToString("N2"));

            var signatoryY = y - 38;
            var signatories = new (int X, string? Title, string? Name)[]
            {
                (32, report.Signatories.OfficerTitle, report.Signatories.OfficerName),
                (230, report.Signatories.CenterTitle, report.Signatories.CenterName),
                (428, report.Signatories.LeftTitle, report.Signatories.LeftName),
                (626, report.Signatories.RightTitle, report.Signatories.RightName),
            };
            foreach (var signatory in signatories)
            {
                WriteText(content, signatory.X, signatoryY, 7, signatory.Title);
                content.AppendLine($"{signatory.X} {signatoryY - 18} m {signatory.X + 145} {signatoryY - 18} l S");
                WriteText(content, signatory.X, signatoryY - 28, 7, signatory.Name);
            }
        }

        WriteText(content, 350, 20, 7, $"CNSS - Tresorerie | Page {pageNumber} / {pageCount}");
        return content.ToString();
    }

    private static byte[] CreateDocument(IReadOnlyList<string> contents, PdfJpeg? logo)
    {
        var objects = new List<byte[]>
        {
            ToBytes("<< /Type /Catalog /Pages 2 0 R >>"),
            ToBytes($"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(0, contents.Count).Select(index => $"{4 + index * 2} 0 R"))}] /Count {contents.Count} >>"),
            ToBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>")
        };
        var imageObject = logo is null ? 0 : 4 + contents.Count * 2;

        for (var index = 0; index < contents.Count; index++)
        {
            var pageObject = 4 + index * 2;
            var contentObject = pageObject + 1;
            var content = contents[index];
            var imageResource = logo is null ? string.Empty : $" /XObject << /Im1 {imageObject} 0 R >>";
            objects.Add(ToBytes($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 841.89 595.28] /Resources << /Font << /F1 3 0 R >>{imageResource} >> /Contents {contentObject} 0 R >>"));
            objects.Add(ToBytes($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream"));
        }

        if (logo is not null)
            objects.Add(Combine(ToBytes($"<< /Type /XObject /Subtype /Image /Width {logo.Width} /Height {logo.Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {logo.Data.Length} >>\nstream\n"), logo.Data, ToBytes("\nendstream")));

        using var stream = new MemoryStream();
        Write(stream, "%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(stream.Position);
            Write(stream, $"{index + 1} 0 obj\n");
            stream.Write(objects[index], 0, objects[index].Length);
            Write(stream, "\nendobj\n");
        }

        var xref = stream.Position;
        Write(stream, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) Write(stream, $"{offset:D10} 00000 n \n");
        Write(stream, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return stream.ToArray();
    }

    private static void WriteText(StringBuilder content, int x, int y, int size, string? text) =>
        content.AppendLine($"BT /F1 {size} Tf {x} {y} Td ({Escape(text)}) Tj ET");

    private static string Escape(string? text)
    {
        var normalized = (text ?? string.Empty).Normalize(NormalizationForm.FormD);
        var ascii = new string(normalized.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && character <= 127).ToArray());
        return ascii.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }

    private static string Truncate(string? value, int length) =>
        string.IsNullOrWhiteSpace(value) || value.Length <= length ? value ?? string.Empty : value[..(length - 3)] + "...";

    private static void Write(Stream stream, string value)
    {
        var bytes = ToBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static byte[] ToBytes(string value) => Encoding.ASCII.GetBytes(value);

    private static byte[] Combine(params byte[][] parts)
    {
        var length = parts.Sum(part => part.Length);
        var result = new byte[length];
        var offset = 0;
        foreach (var part in parts)
        {
            Buffer.BlockCopy(part, 0, result, offset, part.Length);
            offset += part.Length;
        }
        return result;
    }

    private sealed record PdfJpeg(byte[] Data, int Width, int Height)
    {
        public static PdfJpeg? TryLoad(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !string.Equals(Path.GetExtension(path), ".jpg", StringComparison.OrdinalIgnoreCase)) return null;
            var data = File.ReadAllBytes(path);
            for (var index = 2; index + 9 < data.Length; index++)
            {
                if (data[index] != 0xFF || data[index + 1] is not (>= 0xC0 and <= 0xC3)) continue;
                var height = (data[index + 5] << 8) + data[index + 6];
                var width = (data[index + 7] << 8) + data[index + 8];
                return width > 0 && height > 0 ? new PdfJpeg(data, width, height) : null;
            }
            return null;
        }
    }
}