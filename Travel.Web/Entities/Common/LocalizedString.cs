namespace Travel.Web.Entities.Common
{
    public class LocalizedString
    {
        public string Tr { get; set; } = string.Empty;
        public string En { get; set; } = string.Empty;

        // Geçerli thread kültürüne göre metni dönen pratik metot
        public string Value => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower() == "en" ? En : Tr;
    }
}
