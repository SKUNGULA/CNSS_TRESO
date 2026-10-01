using ClosedXML.Excel;
using CNSS_TRESO.Application.Configuration;
using CNSS_TRESO.Application.Treasury.Reporting;
using CNSS_TRESO.Domain.Auditing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CNSS_TRESO.Infrastructure.Reporting;

public sealed class ClosedXmlExcelExporter : IExcelExporter
{
    private readonly ReportBranding _branding;
    private readonly ILogger<ClosedXmlExcelExporter> _logger;

    public ClosedXmlExcelExporter(IOptions<ReportBranding> branding, ILogger<ClosedXmlExcelExporter> logger)
    {
        _branding = branding.Value;
        _logger = logger;
    }

    private void ApplyLetterhead(IXLWorksheet sheet, string? costCenterLabel, int totalCols)
    {
        var logoPath = _branding.LogoPath;
        if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
        {
            try
            {
                using var logoStream = File.OpenRead(logoPath);
                sheet.AddPicture(logoStream, "logo-cnss.jpg")
                    .MoveTo(sheet.Cell(1, 1))
                    .WithSize(150, 60);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Le logo CNSS n'a pas pu être ajouté à l'export Excel depuis {LogoPath}.", logoPath);
            }
        }
        else _logger.LogWarning("Le fichier du logo CNSS est introuvable pour l'export Excel : {LogoPath}.", logoPath);

        sheet.Column(1).Width = Math.Max(sheet.Column(1).Width, 22);

        sheet.Range(1, 2, 1, totalCols).Merge();
        sheet.Cell(1, 2).Value = _branding.Organization;
        sheet.Cell(1, 2).Style.Font.Bold = true;
        sheet.Cell(1, 2).Style.Font.FontSize = 13;
        sheet.Cell(1, 2).Style.Font.FontColor = XLColor.FromHtml("#0A3D8F");
        sheet.Cell(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        sheet.Cell(1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Range(2, 2, 2, totalCols).Merge();
        sheet.Cell(2, 2).Value = $"Centre de frais : {costCenterLabel ?? "(tous)"}";
        sheet.Cell(2, 2).Style.Font.Bold = true;
        sheet.Cell(2, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Row(1).Height = 22;
        sheet.Row(2).Height = 20;
        sheet.Row(3).Height = 18;

        var sep = sheet.Range(3, 1, 3, totalCols);
        sep.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        sep.Style.Border.BottomBorderColor = XLColor.FromHtml("#2E5C8A");
    }

    private static void ApplyLandscape(IXLWorksheet sheet, int totalCols, int lastRow, int headerRow)
    {
        var ps = sheet.PageSetup;
        ps.PageOrientation = XLPageOrientation.Landscape;
        ps.PaperSize = XLPaperSize.A4Paper;
        ps.FitToPages(1, 0);
        ps.Margins.SetTop(0.6).SetBottom(0.6).SetLeft(0.4).SetRight(0.4);
        ps.CenterHorizontally = true;
        ps.SetRowsToRepeatAtTop(headerRow, headerRow);
        ps.PrintAreas.Add(sheet.Range(1, 1, lastRow, totalCols).RangeAddress.ToString());
        ps.Footer.Left.AddText("Imprime le " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        ps.Footer.Center.AddText("CNSS - Tresorerie");
        ps.Footer.Right.AddText("Page &P / &N");
    }

    public byte[] ExportCashReport(CashReport report)
    {
        using var workbook = new XLWorkbook();
        var isCash = string.Equals(report.JournalType, "C", StringComparison.OrdinalIgnoreCase);
        var title = isCash ? "LIVRE DE CAISSE" : "LIVRE DE BANQUE";
        var sheet = workbook.Worksheets.Add(isCash ? "Caisse" : "Banque");
        const int TotalCols = 10;

        ApplyLetterhead(sheet, report.CostCenterLabel ?? report.CostCenter, TotalCols);

        var header = $"{title} {report.JournalCode} {report.JournalLabel?.ToUpperInvariant()} DU {report.FromDate:dd/MM/yyyy} AU {report.ToDate:dd/MM/yyyy}";
        sheet.Range(4, 1, 4, TotalCols).Merge();
        sheet.Cell(4, 1).Value = header;
        sheet.Cell(4, 1).Style.Font.Bold = true;
        sheet.Cell(4, 1).Style.Font.FontSize = 14;
        sheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");

        sheet.Cell(5, 1).Value = "Solde initial :";
        sheet.Cell(5, 2).Value = report.OpeningBalance;
        sheet.Cell(5, 4).Value = "Total recettes :";
        sheet.Cell(5, 5).Value = report.TotalReceipts;
        sheet.Cell(5, 7).Value = "Total depenses :";
        sheet.Cell(5, 8).Value = report.TotalExpenses;
        sheet.Cell(5, 9).Value = "Solde final :";
        sheet.Cell(5, 10).Value = report.ClosingBalance;
        sheet.Range(5, 1, 5, 10).Style.Font.Bold = true;
        sheet.Range(5, 2, 5, 2).Style.NumberFormat.Format = "#,##0.00";
        sheet.Range(5, 5, 5, 5).Style.NumberFormat.Format = "#,##0.00";
        sheet.Range(5, 8, 5, 8).Style.NumberFormat.Format = "#,##0.00";
        sheet.Range(5, 10, 5, 10).Style.NumberFormat.Format = "#,##0.00";

        var headerRow = 7;
        var headers = new[] { "Date", "Journal", "N° Document", "N° Pièce", "Compte", "Libellé", "Bénéficiaire", "Recette", "Dépense", "Solde" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E5C8A");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        sheet.Row(headerRow).Height = 22;

        var r = headerRow + 1;
        var firstDataRow = r;
        foreach (var row in report.Rows)
        {
            sheet.Cell(r, 1).Value = row.MovementDate.ToString("dd/MM/yyyy");
            sheet.Cell(r, 2).Value = row.JournalCode;
            sheet.Cell(r, 3).Value = row.DocumentNumber;
            sheet.Cell(r, 4).Value = row.PieceNumber;
            sheet.Cell(r, 5).Value = row.Account;
            sheet.Cell(r, 6).Value = row.Description;
            sheet.Cell(r, 7).Value = row.Beneficiary;
            sheet.Cell(r, 8).Value = row.AmountReceipt;
            sheet.Cell(r, 9).Value = row.AmountExpense;
            sheet.Cell(r, 10).Value = row.RunningBalance;
            sheet.Range(r, 8, r, 10).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(r, 6).Style.Alignment.WrapText = true;
            if (row.IsOpening)
            {
                sheet.Range(r, 1, r, 10).Style.Font.Italic = true;
                sheet.Range(r, 1, r, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3CD");
            }
            else if ((r - firstDataRow) % 2 == 1)
            {
                sheet.Range(r, 1, r, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F7FA");
            }
            r++;
        }

        var totalRow = r + 1;
        sheet.Cell(totalRow, 6).Value = "TOTAUX";
        sheet.Cell(totalRow, 8).Value = report.TotalReceipts;
        sheet.Cell(totalRow, 9).Value = report.TotalExpenses;
        sheet.Cell(totalRow, 10).Value = report.ClosingBalance;
        sheet.Range(totalRow, 1, totalRow, 10).Style.Font.Bold = true;
        sheet.Range(totalRow, 1, totalRow, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
        sheet.Range(totalRow, 8, totalRow, 10).Style.NumberFormat.Format = "#,##0.00";

        var grid = sheet.Range(headerRow, 1, totalRow, 10);
        grid.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        grid.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        grid.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var sig = report.Signatories;
        var sigRow = totalRow + 4;
        WriteSignatory(sheet, sigRow, 1, sig.OfficerTitle, sig.OfficerName);
        WriteSignatory(sheet, sigRow, 4, sig.CenterTitle, sig.CenterName);
        WriteSignatory(sheet, sigRow, 7, sig.LeftTitle, sig.LeftName);
        WriteSignatory(sheet, sigRow, 9, sig.RightTitle, sig.RightName);

        sheet.Column(1).Width = 12;
        sheet.Column(2).Width = 14;
        sheet.Column(3).Width = 12;
        sheet.Column(4).Width = 12;
        sheet.Column(5).Width = 18;
        sheet.Column(6).Width = 45;
        sheet.Column(7).Width = 28;
        sheet.Column(8).Width = 15;
        sheet.Column(9).Width = 15;
        sheet.Column(10).Width = 16;

        sheet.SheetView.FreezeRows(headerRow);
        ApplyLandscape(sheet, TotalCols, sigRow + 2, headerRow);
        sheet.PageSetup.Header.Left.AddText(title + " - " + (report.JournalCode ?? string.Empty));
        sheet.PageSetup.Header.Right.AddText(report.FromDate.ToString("dd/MM/yyyy") + " - " + report.ToDate.ToString("dd/MM/yyyy"));

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteSignatory(IXLWorksheet sheet, int row, int col, string? title, string? name)
    {
        sheet.Cell(row, col).Value = title ?? string.Empty;
        sheet.Cell(row, col).Style.Font.Bold = true;
        sheet.Cell(row + 2, col).Value = name ?? string.Empty;
        sheet.Cell(row + 2, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
    }

    public byte[] ExportTiersReport(TiersReport report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Tiers");
        const int TotalCols = 9;

        ApplyLetterhead(sheet, report.CostCenterLabel ?? report.CostCenter, TotalCols);

        var header = $"JOURNAL DES TIERS DU {report.FromDate:dd/MM/yyyy} AU {report.ToDate:dd/MM/yyyy}";
        sheet.Range(4, 1, 4, TotalCols).Merge();
        sheet.Cell(4, 1).Value = header;
        sheet.Cell(4, 1).Style.Font.Bold = true;
        sheet.Cell(4, 1).Style.Font.FontSize = 14;
        sheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");

        sheet.Range(5, 1, 5, TotalCols).Merge();
        sheet.Cell(5, 1).Value = $"Journal : {report.JournalCode ?? "(tous)"}    Sens : {report.Direction ?? "(R+D)"}";
        sheet.Cell(5, 1).Style.Font.Italic = true;
        sheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var headerRow = 7;
        var headers = new[] { "Date", "Journal", "N° Doc", "N° Pièce", "Compte", "Libellé", "Sens", "Monnaie", "Montant" };
        for (var i = 0; i < headers.Length; i++)
        {
            var c = sheet.Cell(headerRow, i + 1);
            c.Value = headers[i];
            c.Style.Font.Bold = true;
            c.Style.Font.FontColor = XLColor.White;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E5C8A");
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        sheet.Row(headerRow).Height = 22;

        var r = headerRow + 1;
        foreach (var row in report.Rows)
        {
            sheet.Range(r, 1, r, TotalCols).Merge();
            sheet.Cell(r, 1).Value = $"TIERS : {row.TiersName}  ({row.Count} mouvement(s))";
            sheet.Cell(r, 1).Style.Font.Bold = true;
            sheet.Cell(r, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
            r++;

            foreach (var d in row.Details)
            {
                sheet.Cell(r, 1).Value = d.MovementDate.ToString("dd/MM/yyyy");
                sheet.Cell(r, 2).Value = d.JournalCode;
                sheet.Cell(r, 3).Value = d.DocumentNumber;
                sheet.Cell(r, 4).Value = d.PieceNumber;
                sheet.Cell(r, 5).Value = d.Account;
                sheet.Cell(r, 6).Value = d.Description;
                sheet.Cell(r, 7).Value = d.Direction;
                sheet.Cell(r, 8).Value = d.CurrencyCode;
                sheet.Cell(r, 9).Value = d.Amount;
                sheet.Cell(r, 9).Style.NumberFormat.Format = "#,##0.00";
                sheet.Cell(r, 6).Style.Alignment.WrapText = true;
                r++;
            }

            sheet.Cell(r, 6).Value = "Sous-totaux";
            sheet.Cell(r, 7).Value = $"R: {row.TotalReceipts:N2}";
            sheet.Cell(r, 8).Value = $"D: {row.TotalExpenses:N2}";
            sheet.Cell(r, 9).Value = row.Net;
            sheet.Range(r, 6, r, 9).Style.Font.Bold = true;
            sheet.Range(r, 6, r, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F7FA");
            sheet.Range(r, 6, r, 9).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            sheet.Cell(r, 9).Style.NumberFormat.Format = "#,##0.00";
            r += 2;
        }

        sheet.Cell(r, 5).Value = "TOTAUX GENERAUX";
        sheet.Cell(r, 7).Value = report.GrandTotalReceipts;
        sheet.Cell(r, 8).Value = report.GrandTotalExpenses;
        sheet.Cell(r, 9).Value = report.GrandNet;
        sheet.Range(r, 1, r, 9).Style.Font.Bold = true;
        sheet.Range(r, 1, r, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
        sheet.Range(r, 7, r, 9).Style.NumberFormat.Format = "#,##0.00";
        sheet.Range(r, 1, r, 9).Style.Border.TopBorder = XLBorderStyleValues.Medium;

        var grid = sheet.Range(headerRow, 1, r, TotalCols);
        grid.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        grid.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        grid.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Column(1).Width = 12;
        sheet.Column(2).Width = 14;
        sheet.Column(3).Width = 12;
        sheet.Column(4).Width = 12;
        sheet.Column(5).Width = 16;
        sheet.Column(6).Width = 40;
        sheet.Column(7).Width = 10;
        sheet.Column(8).Width = 12;
        sheet.Column(9).Width = 16;

        sheet.SheetView.FreezeRows(headerRow);
        ApplyLandscape(sheet, TotalCols, r + 1, headerRow);
        sheet.PageSetup.Header.Left.AddText("JOURNAL DES TIERS");
        sheet.PageSetup.Header.Right.AddText(report.FromDate.ToString("dd/MM/yyyy") + " - " + report.ToDate.ToString("dd/MM/yyyy"));

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportAuditEntries(IReadOnlyList<AuditEntry> entries, string? username, string? action, DateTime? fromUtc, DateTime? toUtc)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Audit");
        const int TotalCols = 6;

        ApplyLetterhead(sheet, null, TotalCols);

        sheet.Range(4, 1, 4, TotalCols).Merge();
        sheet.Cell(4, 1).Value = "JOURNAL D'AUDIT";
        sheet.Cell(4, 1).Style.Font.Bold = true;
        sheet.Cell(4, 1).Style.Font.FontSize = 14;
        sheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(username)) filters.Add($"Utilisateur: {username}");
        if (!string.IsNullOrWhiteSpace(action)) filters.Add($"Action: {action}");
        if (fromUtc.HasValue) filters.Add($"Du: {fromUtc:yyyy-MM-dd HH:mm}");
        if (toUtc.HasValue) filters.Add($"Au: {toUtc:yyyy-MM-dd HH:mm}");
        sheet.Range(5, 1, 5, TotalCols).Merge();
        sheet.Cell(5, 1).Value = filters.Count == 0 ? "Tous les évènements" : string.Join(" | ", filters);
        sheet.Cell(5, 1).Style.Font.Italic = true;
        sheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var headerRow = 7;
        string[] headers = { "Date (UTC)", "Utilisateur", "Action", "Type", "Cible", "Détails" };
        for (var i = 0; i < headers.Length; i++)
        {
            var c = sheet.Cell(headerRow, i + 1);
            c.Value = headers[i];
            c.Style.Font.Bold = true;
            c.Style.Font.FontColor = XLColor.White;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E5C8A");
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        sheet.Row(headerRow).Height = 22;

        var r = headerRow + 1;
        var firstDataRow = r;
        foreach (var e in entries)
        {
            sheet.Cell(r, 1).Value = e.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss");
            sheet.Cell(r, 2).Value = e.Username;
            sheet.Cell(r, 3).Value = e.Action;
            sheet.Cell(r, 4).Value = e.EntityType;
            sheet.Cell(r, 5).Value = e.EntityId ?? string.Empty;
            sheet.Cell(r, 6).Value = e.Details ?? string.Empty;
            sheet.Cell(r, 6).Style.Alignment.WrapText = true;
            if ((r - firstDataRow) % 2 == 1)
                sheet.Range(r, 1, r, TotalCols).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F7FA");
            r++;
        }

        var lastRow = Math.Max(r - 1, headerRow);
        sheet.Range(headerRow, 1, lastRow, TotalCols).SetAutoFilter();
        var grid = sheet.Range(headerRow, 1, lastRow, TotalCols);
        grid.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        grid.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        grid.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Column(1).Width = 20;
        sheet.Column(2).Width = 18;
        sheet.Column(3).Width = 22;
        sheet.Column(4).Width = 18;
        sheet.Column(5).Width = 22;
        sheet.Column(6).Width = 60;

        sheet.SheetView.FreezeRows(headerRow);
        ApplyLandscape(sheet, TotalCols, lastRow, headerRow);
        sheet.PageSetup.Header.Left.AddText("JOURNAL D'AUDIT");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
