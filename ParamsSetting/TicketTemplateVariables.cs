public static class TicketTemplateVariables
{
    public static Dictionary<string, string> Build(
        Tiket tiket,
        User user,
        string messageText)
    {
        return new Dictionary<string, string>
        {
            ["TicketNumber"] = tiket.Oid.ToString(),
            ["ClientName"] = tiket.Author?.Name,
            ["UserName"] = user?.Name,
            ["Status"] = tiket.State?.Name,
            ["Priority"] = tiket.Preorety?.Name,
            ["Deadline"] = tiket.DueDate?.ToString("dd.MM.yyyy"),
            ["MessageText"] = messageText,
        //    ["TicketUrl"] = $"{settings?.BaseUrl}/tickets/{tiket.Oid}",
         //   ["CompanyName"] = settings?.CompanyName,
        //    ["LogoUrl"] = settings?.LogoUrl,
        //    ["CompanyFooter"] = settings?.EmailFooterHtml
        };
    }
}
